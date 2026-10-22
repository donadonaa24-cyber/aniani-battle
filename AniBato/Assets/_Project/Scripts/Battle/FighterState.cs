namespace AniBato.Battle
{
    public enum FighterMode { Idle, Move, Dash, Jump, Fall, Attack, Special, Guard, Dodge, Counter, Hitstun, Knockback, Recovery, KO, Crouch, JumpStartup, Charging, LedgeHang, LedgeClimb }

    public sealed class FighterState
    {
        public int PlayerIndex { get; }
        public CharacterDefinition Definition { get; }
        public BattleVector Position { get; internal set; }
        public BattleVector Velocity { get; internal set; }
        public int Facing { get; internal set; }
        public FighterMode Mode { get; internal set; }
        public bool Grounded { get; internal set; }
        public int GroundPlatform { get; internal set; } = -1;
        public int Hearts { get; internal set; }
        public int NormalHitCount { get; internal set; }
        public int HeartRecoveryRemaining { get; internal set; }
        public int HitstopRemaining { get; internal set; }
        public int HitstunRemaining { get; internal set; }
        public int DamageRecoilRemaining { get; internal set; }
        internal float DamageRecoilSpeed;
        public bool LastDamageLostHeart { get; internal set; }
        public int JumpsUsed { get; internal set; }
        public int LedgeSide { get; internal set; }
        public BattleVector LedgePoint { get; internal set; }
        public int LedgeAge { get; internal set; }
        public bool LedgeRegrabBlocked { get; internal set; }
        internal BattleVector LedgeStart;
        internal int LedgeJumpCarryRemaining, LedgeJumpDirection;
        public int JumpStartupRemaining { get; internal set; }
        internal float JumpStartupDirection;
        public int AirDashesUsed { get; internal set; }
        public int DashRemaining { get; internal set; }
        public int DashDirection { get; internal set; }
        public int DropThroughRemaining { get; internal set; }
        public bool FastFalling { get; internal set; }
        public int DownHeldFrames { get; internal set; }
        public int StaminaUnits { get; internal set; }
        public float Stamina => StaminaUnits / (float)StaminaSystem.UnitsPerPoint;
        public int StaminaDelayRemaining { get; internal set; }
        public int StaminaRejectedRemaining { get; internal set; }
        public int GuardLockRemaining { get; internal set; }
        public bool Crouching => Grounded && (Mode == FighterMode.Crouch || Attack != null && Attack.Definition.NormalDirection == NormalDirection.Crouch);
        public AttackInstance Attack { get; internal set; }
        public int Gauge { get; internal set; }
        public int UltimateUses { get; internal set; }
        public bool UnlimitedUltimates { get; internal set; }
        public int ChargeFrames { get; internal set; }
        public bool MaximumChargeLaunch { get; internal set; }
        internal NormalDirection ChargeDirection;
        public bool UpSpecialUsed { get; internal set; }
        public int PlacedCount { get; internal set; }
        public int ActionFrames { get; internal set; }
        public int DefenseAge { get; internal set; }
        public int GuardStunRemaining { get; internal set; }
        public int CounterCooldownRemaining { get; internal set; }
        public bool JustDodgeSucceeded { get; internal set; }
        public SpecialSlot CurrentSpecial { get; internal set; }
        public ICounterEffect CounterEffect { get; set; }
        public int ParalysisRemaining { get; internal set; }
        public int MarkRemaining { get; internal set; }
        public int SupportReuseRemaining { get; internal set; }
        public int SupportSlowRemaining { get; internal set; }
        public float SupportSlowMultiplier { get; internal set; } = 1;
        public int SupportInfiniteRemaining { get; internal set; }
        public int SupportBarrierRemaining { get; internal set; }
        public int SupportBarrierUnits { get; internal set; }
        private float SupportMoveMultiplier => SupportSlowRemaining > 0 ? SupportSlowMultiplier : 1;
        public int BuffRemaining { get => System.Math.Max(NormalBuffRemaining, SpeedBuffRemaining); internal set => NormalBuffRemaining = value; }
        internal int NormalBuffRemaining, SpeedBuffRemaining;
        internal BuffDefinition SpeedBuff;
        public BuffDefinition Buff { get; internal set; }
        private float BaseMoveMultiplier => NormalBuffRemaining > 0 ? SkillSystem.Multiplier(Buff.MoveMultiplier) : 1;
        private float BaseDashMultiplier => NormalBuffRemaining > 0 ? SkillSystem.Multiplier(Buff.DashMultiplier) : 1;
        public float MoveMultiplier => (SpeedBuffRemaining > 0 ? System.Math.Max(BaseMoveMultiplier, SkillSystem.Multiplier(SpeedBuff.MoveMultiplier)) : BaseMoveMultiplier) * SupportMoveMultiplier;
        public float DashMultiplier => (SpeedBuffRemaining > 0 ? System.Math.Max(BaseDashMultiplier, SkillSystem.Multiplier(SpeedBuff.DashMultiplier)) : BaseDashMultiplier) * SupportMoveMultiplier;
        public float StartupMultiplier => NormalBuffRemaining > 0 ? SkillSystem.Multiplier(Buff.StartupMultiplier) : 1;
        public int SpecialCooldown(SpecialSlot slot) => SpecialCooldowns[(int)slot];
        internal readonly int[] SpecialCooldowns = new int[4];
        internal readonly int[] FollowupImmunity = new int[2];
        internal readonly System.Collections.Generic.HashSet<long> PreDefenseOverlaps = new System.Collections.Generic.HashSet<long>();
        internal readonly System.Collections.Generic.HashSet<long> JustExcludedAttacks = new System.Collections.Generic.HashSet<long>();
        public int ImmunityFrom(int attacker) => FollowupImmunity[attacker];
        public bool CanControlMovement => !LedgeSystem.Attached(this) && Attack == null && HitstunRemaining == 0 && ParalysisRemaining == 0 && !ParalyzedThisTick &&
            Mode != FighterMode.Guard && Mode != FighterMode.Dodge && Mode != FighterMode.Counter && Mode != FighterMode.Recovery && Mode != FighterMode.JumpStartup && Mode != FighterMode.Charging;
        internal bool FrozenThisTick;
        public bool IsFrozenThisTick => FrozenThisTick;
        internal bool ParalyzedThisTick;
        internal readonly FighterInputBuffer InputBuffer = new FighterInputBuffer();
        public BattleRect Hurtbox
        {
            get
            {
                var size = Definition.HurtboxSize;
                if (!Crouching || Definition.CrouchHeightRatio <= 0) return new BattleRect(Position, size);
                float height = size.Y * Definition.CrouchHeightRatio;
                return new BattleRect(new BattleVector(Position.X, Position.Y - (size.Y - height) * .5f), new BattleVector(size.X, height));
            }
        }

        internal FighterState(int player, in CharacterDefinition definition, BattleVector spawn, int hearts)
        {
            PlayerIndex = player; Definition = definition.Snapshot(); Position = spawn;
            Facing = player == 0 ? 1 : -1; Hearts = hearts;
            StaminaUnits = Definition.Stamina.Maximum * StaminaSystem.UnitsPerPoint;
            CounterEffect = new DataCounterEffect(Definition.Counter);
        }
    }
}

