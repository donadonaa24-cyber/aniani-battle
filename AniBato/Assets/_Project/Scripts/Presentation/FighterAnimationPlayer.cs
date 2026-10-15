using System;
using AniBato.Battle;

namespace AniBato.Presentation
{
    // Pure presentation data: does not affect attacks, physics, or match resolution.
    public enum AnimationState
    {
        Idle, Move, Dash, Jump, Fall, Land, Attack1, Attack2, Attack3, AirAttack,
        SpecialNeutral, SpecialSide, SpecialUp, SpecialDown, Ultimate, Guard, Dodge,
        Counter, CounterSuccess, Hitstun, Knockback, KO, Win, Lose,
        Crouch, CrouchAttack, BackAttack, AirNeutral, AirForward, AirBack, AirUp, AirDown, AirForwardUp, AirForwardDown, JumpStartup
    }
    [Serializable]
    public struct AnimationBinding
    {
        public AnimationState State;
        public string Clip;
        public int ActiveFrame;
    }
    public struct AnimationSample
    {
        public AnimationState State;
        public int Age;
        public bool FlipX;
        public bool IsAttack;
        public int AttackFrame, Startup, Active, Recovery;
        public int FrameIndex(int count, int fps, bool loop, int activeFrame)
        {
            if (count < 1) return 0;
            if (State == AnimationState.Counter) return 0;
            if (!IsAttack)
            {
                int frame = Math.Max(0, Age) * Math.Max(1, fps) / BattleRuleDefinition.TickRate;
                if (State == AnimationState.CounterSuccess) frame += Math.Max(0, activeFrame);
                return loop ? frame % count : Math.Min(count - 1, frame);
            }
            // Startup shows anticipation; active holds the extended pose; recovery retracts.
            int peak = Math.Max(0, Math.Min(count - 1, activeFrame));
            if (AttackFrame < Startup)
                return Math.Min(Math.Max(0, peak - 1), Math.Max(0, AttackFrame) * Math.Max(1, peak) / Math.Max(1, Startup));
            if (AttackFrame < Startup + Active) return peak;
            int tail = count - 1 - peak;
            return Math.Min(count - 1, peak + 1 + Math.Max(0, AttackFrame - Startup - Active) * Math.Max(0, tail - 1) / Math.Max(1, Recovery));
        }
    }
    [Serializable]
    public struct AnimationTiming
    {
        public int LandingTicks, CounterSuccessTicks, KnockoutTicks;
        public static AnimationTiming Default => new AnimationTiming { LandingTicks = 8, CounterSuccessTicks = 12, KnockoutTicks = 24 };
    }
    public sealed class FighterAnimationPlayer
    {
        private bool initialized, wasGrounded;
        private AnimationTiming timing = AnimationTiming.Default;
        public void Configure(AnimationTiming value) => timing = value;
        private long previousAttack;
        private int landingRemaining, counterRemaining, resultAge;
        public AnimationSample Sample { get; private set; }
        public void Reset(FighterState fighter, MatchState match) { initialized = false; landingRemaining = counterRemaining = resultAge = 0; Advance(fighter, match, false, false, false); }
        // Called once per fixed tick, including result ticks. Rendering only reads Sample.
        public void Advance(FighterState fighter, MatchState match, bool frozen, bool cinematic, bool counterSucceeded)
        {
            if (initialized && (frozen || cinematic)) return;
            if (initialized && !wasGrounded && fighter.Grounded) landingRemaining = timing.LandingTicks;
            wasGrounded = fighter.Grounded;
            if (counterSucceeded) counterRemaining = timing.CounterSuccessTicks;
            if (match.IsFinished) resultAge++; else resultAge = 0;
            AnimationState state = StateFor(fighter);
            if (fighter.Mode == FighterMode.Recovery && initialized && (Sample.State == AnimationState.Dodge || Sample.State == AnimationState.Guard || Sample.State == AnimationState.Counter)) state = Sample.State;
            if (match.IsFinished)
            {
                bool won = match.Result == (fighter.PlayerIndex == 0 ? MatchResult.Player1Wins : MatchResult.Player2Wins);
                state = won ? AnimationState.Win : AnimationState.Lose;
            }
            else if (counterRemaining > 0 && fighter.Attack == null && fighter.HitstunRemaining == 0) state = AnimationState.CounterSuccess;
            else if (landingRemaining > 0 && (state == AnimationState.Idle || state == AnimationState.Move)) state = AnimationState.Land;
            long attackId = fighter.Attack == null ? 0 : fighter.Attack.Id;
            var sample = new AnimationSample { State = state, FlipX = fighter.Facing < 0,
                Age = initialized && Sample.State == state && previousAttack == attackId ? Sample.Age + 1 : 0 };
            if (!match.IsFinished && fighter.Attack != null)
            {
                var attack = fighter.Attack;
                sample.IsAttack = true; sample.AttackFrame = attack.Frame;
                sample.Startup = attack.Definition.StartupFrames; sample.Active = attack.Definition.ActiveFrames; sample.Recovery = attack.Definition.RecoveryFrames;
            }
            Sample = sample; previousAttack = attackId; initialized = true;
            if (landingRemaining > 0) landingRemaining--;
            if (counterRemaining > 0) counterRemaining--;
        }
        public static AnimationState StateFor(FighterState fighter)
        {
            if (fighter.Mode == FighterMode.KO) return AnimationState.KO;
            if (fighter.HitstunRemaining > 0 || fighter.ParalysisRemaining > 0)
                return fighter.Mode == FighterMode.Knockback ? AnimationState.Knockback : AnimationState.Hitstun;
            if (fighter.Attack != null)
            {
                var attack = fighter.Attack;
                if (attack.Definition.IsUltimate) return AnimationState.Ultimate;
                // Reuse the upward key pose until the dedicated ground-up artwork is ready.
                if (attack.Definition.IsNormal && attack.Definition.NormalDirection == NormalDirection.Up) return AnimationState.AirUp;
                if (attack.Definition.IsNormal && attack.Definition.NormalDirection != NormalDirection.Weak)
                    return (AnimationState)((int)AnimationState.CrouchAttack + (int)attack.Definition.NormalDirection - 1);
                if (attack.Definition.IsNormal) return attack.AirborneAttack ? AnimationState.AirAttack :
                    attack.ChainIndex == 0 ? AnimationState.Attack1 : attack.ChainIndex == 1 ? AnimationState.Attack2 : AnimationState.Attack3;
                return (AnimationState)((int)AnimationState.SpecialNeutral + (int)fighter.CurrentSpecial);
            }
            switch (fighter.Mode)
            {
                case FighterMode.Move: return AnimationState.Move;
                case FighterMode.Dash: return AnimationState.Dash;
                case FighterMode.Jump: return AnimationState.Jump;
                case FighterMode.JumpStartup: return AnimationState.JumpStartup;
                case FighterMode.Fall: return AnimationState.Fall;
                case FighterMode.Guard: return AnimationState.Guard;
                case FighterMode.Dodge: return AnimationState.Dodge;
                case FighterMode.Counter: return AnimationState.Counter;
                case FighterMode.Crouch: return AnimationState.Crouch;
                case FighterMode.Hitstun: return AnimationState.Hitstun;
                case FighterMode.Knockback: return AnimationState.Knockback;
                default: return AnimationState.Idle;
            }
        }
    }
}




