using System;

namespace AniBato.Battle
{
    public enum PracticeOpponent { Idle, Guard, Jump, Cpu }
    public sealed class PracticeOptions
    {
        public bool FreezeHearts = true, FreezeTimer = true;
        public PracticeOpponent Opponent;
        public bool DebugDefault = true;
        internal void FinishTick(BattleSimulation sim)
        {
            if (!FreezeHearts) return;
            foreach (var f in new[] { sim.P1, sim.P2 })
            {
                f.Hearts = sim.Rules.InitialHearts;
                if (f.Mode == FighterMode.KO) { f.Mode = FighterMode.Hitstun; f.HitstunRemaining = Math.Max(1, f.HitstunRemaining); }
                if (sim.Stage.IsOutside(f.Position))
                {
                    f.Position = f.PlayerIndex == 0 ? sim.Stage.SpawnP1 : sim.Stage.SpawnP2;
                    f.Velocity = default; f.Attack = null; f.Mode = FighterMode.Idle; f.HitstunRemaining = f.HitstopRemaining = 0;
                    f.DamageRecoilRemaining = 0; f.DamageRecoilSpeed = 0; f.LastDamageLostHeart = false;
                    f.Grounded = false; f.GroundPlatform = -1; f.UpSpecialUsed = false; f.JumpsUsed = f.AirDashesUsed = 0;
                }
            }
        }
        public FrameInput DummyInput(BattleSimulation sim)
        {
            if (Opponent == PracticeOpponent.Guard) return new FrameInput(held: InputButtons.Guard);
            if (Opponent == PracticeOpponent.Jump && sim.P2.Grounded) return new FrameInput(pressed: InputButtons.Jump);
            return default;
        }
    }
    public enum TutorialStep { Move, Jump, DoubleJump, NormalCombo, Directional, Specials, Guard, GuardBreak, Dodge, Counter, JustDefense, Ultimate, Stamina, Rules, Complete }
    public sealed class TutorialSession
    {
        public TutorialStep Step { get; internal set; }
        public int DirectionMask { get; private set; }
        public int SpecialMask { get; private set; }
        private BattleVector origin;
        private bool initialized;
        private long lastAttack;
        private bool spentStamina;
        public bool Complete => Step == TutorialStep.Complete;
        public void Reset() { Step = TutorialStep.Move; DirectionMask = SpecialMask = 0; initialized = spentStamina = false; lastAttack = 0; }
        public void Prepare(BattleSimulation sim)
        {
            if (!initialized) { origin = sim.P1.Position; initialized = true; }
            if (Step == TutorialStep.Ultimate) sim.P1.Gauge = sim.Rules.Gauge.Maximum;
        }
        public FrameInput CoachInput(BattleSimulation sim)
        {
            var self = sim.P2;
            if (Step == TutorialStep.GuardBreak) return new FrameInput(held: InputButtons.Guard);
            if (Step == TutorialStep.Guard || Step == TutorialStep.Dodge || Step == TutorialStep.Counter || Step == TutorialStep.JustDefense)
            {
                if (sim.Match.TickCount % 120 == 0 && self.Attack == null && self.CanControlMovement)
                {
                    self.Position = new BattleVector(sim.P1.Position.X + 2, sim.P1.Position.Y); self.Facing = -1;
                    return new FrameInput(-1, pressed: Step == TutorialStep.Counter ? InputButtons.Attack : InputButtons.Special, actionX: 0, actionY: 0);
                }
            }
            return default;
        }
        public void Observe(BattleSimulation sim, in FrameInput input)
        {
            var p = sim.P1; bool advance = false;
            if (Step == TutorialStep.Move) advance = Math.Abs(p.Position.X - origin.X) >= 1;
            if (Step == TutorialStep.Jump) advance = !p.Grounded && p.JumpsUsed == 1 && p.Velocity.Y > 0;
            if (Step == TutorialStep.DoubleJump) advance = !p.Grounded && p.JumpsUsed >= 2 && input.IsPressed(InputButtons.Jump);
            if (p.Attack != null && p.Attack.Id != lastAttack)
            {
                var a = p.Attack; lastAttack = a.Id;
                if (Step == TutorialStep.NormalCombo) advance = a.Definition.IsNormal && a.ChainIndex == 2;
                if (Step == TutorialStep.Directional)
                {
                    if (a.Definition.NormalDirection == NormalDirection.Crouch) DirectionMask |= 1;
                    if (sim.Rules.CombatInput.SingleAerialNormal ? a.Definition.NormalDirection == NormalDirection.Up : a.Definition.NormalDirection == NormalDirection.Back) DirectionMask |= 2;
                    if (a.AirborneAttack) DirectionMask |= 4;
                    advance = DirectionMask == 7;
                }
                if (Step == TutorialStep.Specials && a.Definition.IsSpecial) { SpecialMask |= 1 << (int)p.CurrentSpecial; advance = sim.Rules.CombatInput.SingleSpecial ? SpecialMask == 1 : SpecialMask == 15; }
            }
            foreach (var e in sim.Events)
            {
                if (Step == TutorialStep.Guard && e.SourcePlayer == 0 && (e.Kind == BattleEventKind.Guarded || e.Kind == BattleEventKind.JustGuard)) advance = true;
                if (Step == TutorialStep.GuardBreak && e.TargetPlayer == 0 && e.Kind == BattleEventKind.GuardBroken) advance = true;
                if (Step == TutorialStep.Dodge && e.SourcePlayer == 0 && (e.Kind == BattleEventKind.Dodged || e.Kind == BattleEventKind.JustDodge)) advance = true;
                if (Step == TutorialStep.Counter && e.SourcePlayer == 0 && e.Kind == BattleEventKind.CounterSuccess) advance = true;
                if (Step == TutorialStep.JustDefense && e.SourcePlayer == 0 && (e.Kind == BattleEventKind.JustGuard || e.Kind == BattleEventKind.JustDodge)) advance = true;
                if (Step == TutorialStep.Ultimate && e.SourcePlayer == 0 && e.Kind == BattleEventKind.UltimateStarted) advance = true;
            }
            if (Step == TutorialStep.Stamina)
            {
                if (p.Stamina < p.Definition.Stamina.Maximum - 5) spentStamina = true;
                advance = spentStamina && p.Grounded && p.Mode == FighterMode.Idle && p.Stamina >= p.Definition.Stamina.Maximum - 1;
            }
            if (Step == TutorialStep.Rules) advance = input.IsPressed(InputButtons.Attack);
            if (advance && !Complete) { Step++; lastAttack = 0; }
        }
    }
}
