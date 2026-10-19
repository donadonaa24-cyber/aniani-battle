using System.Collections.Generic;

namespace AniBato.Battle
{
    public sealed class BattleSimulation
    {
        private readonly FighterStateMachine stateMachine = new FighterStateMachine();
        private readonly MovementSystem movement = new MovementSystem();
        private readonly HitResolver hitResolver = new HitResolver();
        private readonly DamageSystem damage = new DamageSystem();
        private readonly MatchResolver matchResolver = new MatchResolver();
        private readonly List<HitContact> contacts = new List<HitContact>(2);
        private readonly List<BattleEvent> events = new List<BattleEvent>(12);
        private readonly ComboTracker comboP1 = new ComboTracker();
        private readonly ComboTracker comboP2 = new ComboTracker();
        private readonly DefenseSystem defense = new DefenseSystem();
        private readonly ProjectileSystem projectileSystem = new ProjectileSystem();
        private readonly NormalClashSystem clash = new NormalClashSystem();
        private readonly List<DefenseOutcome> outcomes = new List<DefenseOutcome>();
        public FighterState P1 { get; }
        public FighterState P2 { get; }
        public MatchState Match { get; }
        public StageGeometry Stage { get; }
        public StageShrinkController Shrink { get; }
        public BattleRuleDefinition Rules { get; }
        public IReadOnlyList<BattleEvent> Events { get; }
        public ComboTracker ComboP1 => comboP1;
        public ComboTracker ComboP2 => comboP2;
        public IReadOnlyList<Projectile> Projectiles => projectileSystem.Projectiles;
        public ICounterEffect CounterEffect { get => defense.CounterEffect; set { defense.CounterEffect = value; P1.CounterEffect = value; P2.CounterEffect = value; } }
        public int CutInRemainingFrames { get; private set; }
        private PracticeOptions practice;
        public PracticeOptions Practice
        {
            get => practice;
            set
            {
                bool leaving = practice != null && value == null; practice = value;
                P1.UnlimitedUltimates = P2.UnlimitedUltimates = practice != null;
                if (practice != null) P1.Gauge = P2.Gauge = Rules.Gauge.Maximum;
                if (leaving) { P1.Gauge = P2.Gauge = 0; P1.UltimateUses = P2.UltimateUses = 0; }
            }
        }

        public BattleSimulation(in CharacterDefinition p1, in CharacterDefinition p2, in BattleRuleDefinition rules, StageGeometry stage)
        {
            rules.Validate(); Rules = rules; Stage = (stage ?? throw new System.ArgumentNullException(nameof(stage))).Snapshot();
            Shrink = new StageShrinkController(Stage, rules.Shrink);
            P1 = new FighterState(0, p1, stage.SpawnP1, rules.InitialHearts);
            P2 = new FighterState(1, p2, stage.SpawnP2, rules.InitialHearts);
            Match = new MatchState(rules.DurationTicks); Events = events.AsReadOnly();
            // Resolve initial ground contact before the first input, so the first jump is a ground jump.
            movement.Step(P1, default, stage, rules); movement.Step(P2, default, stage, rules);
        }

        public void Tick(in FrameInput p1, in FrameInput p2)
        {
            events.Clear();
            if (Match.IsFinished) return;
            if (Practice != null) P1.Gauge = P2.Gauge = Rules.Gauge.Maximum;
            // Cinematic ticks consume no match time, physics, status duration, or buffered actions.
            if (CutInRemainingFrames > 0) { CutInRemainingFrames--; return; }
            Match.TickCount++; if (Practice == null || !Practice.FreezeTimer) Match.RemainingTicks--;
            Shrink.Tick(Match, events);
            MovementSystem.InvalidateGround(P1, Stage); MovementSystem.InvalidateGround(P2, Stage);
            defense.CaptureOverlaps(P1, P2, Projectiles); defense.CaptureOverlaps(P2, P1, Projectiles);
            int usesP1 = P1.UltimateUses, usesP2 = P2.UltimateUses;
            long attackP1 = P1.Attack == null ? 0 : P1.Attack.Id, attackP2 = P2.Attack == null ? 0 : P2.Attack.Id;
            P1.PlacedCount = projectileSystem.PlacedCount(P1); P2.PlacedCount = projectileSystem.PlacedCount(P2);
            stateMachine.Step(P1, p1, Match.TickCount, Rules);
            stateMachine.Step(P2, p2, Match.TickCount, Rules);
            EmitAttack(P1, attackP1); EmitAttack(P2, attackP2);
            EmitUltimate(P1, usesP1); EmitUltimate(P2, usesP2);
            if (CutInRemainingFrames > 0)
            {
                matchResolver.Resolve(Match, P1, P2, Stage, Rules, events);
                if (Match.IsFinished) CutInRemainingFrames = 0;
                return;
            }
            AbilityMotionSystem.Step(P1, P2, Stage); AbilityMotionSystem.Step(P2, P1, Stage);
            // Sample both fields before moving either fighter, so player order cannot change the pull.
            var pullP1 = ProjectileAttractionSystem.Velocity(P1, Projectiles);
            var pullP2 = ProjectileAttractionSystem.Velocity(P2, Projectiles);
            movement.Step(P1, p1, Stage, Rules, pullP1); movement.Step(P2, p2, Stage, Rules, pullP2);
            projectileSystem.Step(P1, P2, Rules.ProjectileLimitPerOwner, Stage);
            if (Rules.CombatInput.NormalClashes)
            { clash.Resolve(P1, P2, Projectiles, Match.TickCount, Rules.CombatInput, events); projectileSystem.RemoveClashed(); }
            contacts.Clear();
            hitResolver.Gather(P1, P2, contacts); hitResolver.Gather(P2, P1, contacts);
            foreach (var projectile in Projectiles)
                hitResolver.Gather(projectile, projectile.Owner.PlayerIndex == 0 ? P2 : P1, contacts);
            outcomes.Clear();
            foreach (var contact in contacts) outcomes.Add(defense.Evaluate(contact, Rules));
            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                // A finish/counter earlier in this tick also blocks already gathered follow-ups.
                if (contact.Target.ImmunityFrom(contact.Attacker.PlayerIndex) > 0 || contact.Target.HeartRecoveryRemaining > 0)
                { projectileSystem.ResolvePlacedContact(contact, false); continue; }
                if (outcomes[i] == DefenseOutcome.Hit)
                    damage.Apply(contact, contact.Attacker.PlayerIndex == 0 ? comboP1 : comboP2, Match.TickCount, Rules, events, Stage);
                else defense.Apply(contact, outcomes[i], outcomes[i] == DefenseOutcome.CrouchBreak || outcomes[i] == DefenseOutcome.Guard || outcomes[i] == DefenseOutcome.JustGuard ?
                    (contact.Attacker.PlayerIndex == 0 ? comboP1 : comboP2) : (contact.Target.PlayerIndex == 0 ? comboP1 : comboP2), damage, Match.TickCount, Rules, events, Stage);
                projectileSystem.ResolvePlacedContact(contact, outcomes[i] == DefenseOutcome.Hit || outcomes[i] == DefenseOutcome.CrouchBreak);
            }
            P1.PlacedCount = projectileSystem.PlacedCount(P1); P2.PlacedCount = projectileSystem.PlacedCount(P2);
            Practice?.FinishTick(this);
            matchResolver.Resolve(Match, P1, P2, Stage, Rules, events);
        }
        private void EmitUltimate(FighterState fighter, int previousUses)
        {
            if (fighter.UltimateUses == previousUses) return;
            CutInRemainingFrames = System.Math.Max(CutInRemainingFrames, fighter.Definition.UltimateCutInFrames);
            events.Add(new BattleEvent(BattleEventKind.UltimateStarted, Match.TickCount, fighter.PlayerIndex, attackId: fighter.Attack.Id));
            events.Add(new BattleEvent(BattleEventKind.GaugeChanged, Match.TickCount, fighter.PlayerIndex, fighter.PlayerIndex, value: fighter.Gauge));
        }
        private void EmitAttack(FighterState fighter, long previousId)
        {
            if (fighter.Attack != null && fighter.Attack.Id != previousId)
                events.Add(new BattleEvent(BattleEventKind.AttackStarted, Match.TickCount, fighter.PlayerIndex,
                    attackId: fighter.Attack.Id, attackKind: fighter.Attack.Definition.Kind));
        }
    }
}
