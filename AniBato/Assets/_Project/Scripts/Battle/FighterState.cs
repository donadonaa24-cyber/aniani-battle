namespace AniBato.Battle
{
    public enum FighterMode { Idle, Move, Dash, Jump, Fall, Attack, Special, Guard, Dodge, Counter, Hitstun, Knockback, Recovery, KO }

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
        public int JumpsUsed { get; internal set; }
        public int AirDashesUsed { get; internal set; }
        public int DashRemaining { get; internal set; }
        public int DashDirection { get; internal set; }
        public int DropThroughRemaining { get; internal set; }
        public bool FastFalling { get; internal set; }
        public AttackInstance Attack { get; internal set; }
        public int Gauge { get; internal set; }
        public int UltimateUses { get; internal set; }
        public bool UpSpecialUsed { get; internal set; }
        public int ActionFrames { get; internal set; }
        public int DefenseAge { get; internal set; }
        public int GuardStunRemaining { get; internal set; }
        public int CounterCooldownRemaining { get; internal set; }
        public bool JustDodgeSucceeded { get; internal set; }
        public SpecialSlot CurrentSpecial { get; internal set; }
        public ICounterEffect CounterEffect { get; set; }
        public int ParalysisRemaining { get; internal set; }
        public int MarkRemaining { get; internal set; }
        public int BuffRemaining { get; internal set; }
        public BuffDefinition Buff { get; internal set; }
        public float MoveMultiplier => BuffRemaining > 0 ? SkillSystem.Multiplier(Buff.MoveMultiplier) : 1;
        public float DashMultiplier => BuffRemaining > 0 ? SkillSystem.Multiplier(Buff.DashMultiplier) : 1;
        public int SpecialCooldown(SpecialSlot slot) => SpecialCooldowns[(int)slot];
        internal readonly int[] SpecialCooldowns = new int[4];
        internal readonly int[] FollowupImmunity = new int[2];
        internal readonly System.Collections.Generic.HashSet<long> PreDefenseOverlaps = new System.Collections.Generic.HashSet<long>();
        internal readonly System.Collections.Generic.HashSet<long> JustExcludedAttacks = new System.Collections.Generic.HashSet<long>();
        public int ImmunityFrom(int attacker) => FollowupImmunity[attacker];
        public bool CanControlMovement => Attack == null && HitstunRemaining == 0 && ParalysisRemaining == 0 && !ParalyzedThisTick &&
            Mode != FighterMode.Guard && Mode != FighterMode.Dodge && Mode != FighterMode.Counter && Mode != FighterMode.Recovery;
        internal bool FrozenThisTick;
        public bool IsFrozenThisTick => FrozenThisTick;
        internal bool ParalyzedThisTick;
        internal readonly FighterInputBuffer InputBuffer = new FighterInputBuffer();
        public BattleRect Hurtbox => new BattleRect(Position, Definition.HurtboxSize);

        internal FighterState(int player, in CharacterDefinition definition, BattleVector spawn, int hearts)
        {
            PlayerIndex = player; Definition = definition.Snapshot(); Position = spawn;
            Facing = player == 0 ? 1 : -1; Hearts = hearts;
            CounterEffect = new DataCounterEffect(Definition.Counter);
        }
    }
}

