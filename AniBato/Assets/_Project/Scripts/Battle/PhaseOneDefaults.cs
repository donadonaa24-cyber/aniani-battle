namespace AniBato.Battle
{
    /// <summary>Asset creation and test fixtures only. Runtime values come from ScriptableObjects.</summary>
    public static class PhaseOneDefaults
    {
        public static BattleRuleDefinition Rules() => new BattleRuleDefinition
        {
            MatchSeconds = 70, InitialHearts = 3, NormalHitsPerHeart = 3, InputBufferFrames = 6,
            DoubleTapFrames = 12, ComboWindowFrames = 40, MaxComboHits = 3, MaxHeartsLostPerCombo = 1,
            DropThroughFrames = 12, DropThroughSpeed = 8, DirectionThreshold = 0.5f, EnableRingOut = true,
            SchemaVersion = 4, Defense = PhaseTwoDefaults.Defense(), Gauge = PhaseTwoDefaults.Gauge(), Shrink = StageShrinkDefinition.Default,
            ComboFinishKnockback = new BattleVector(18, 14), ComboFinishHitstunFrames = 24, FollowupImmunityFrames = 48
        };
        public static AttackDefinition Normal(string name = "通常攻撃") => new AttackDefinition
        {
            Name = name, Kind = AttackKind.Normal, IsCloseRange = true,
            StartupFrames = 4, ActiveFrames = 3, RecoveryFrames = 8,
            Hitbox = new BattleRect(new BattleVector(1.4f, 0.1f), new BattleVector(2.2f, 1.8f)),
            HitstopFrames = 4, HitstunFrames = 8, Knockback = new BattleVector(2f, 0)
        };
        public static CharacterDefinition Character(string name = "仮キャラA") => new CharacterDefinition
        {
            Name = name, HurtboxSize = new BattleVector(1.1f, 2.2f), MoveSpeed = 9.5f, DashSpeed = 17,
            GroundDashFrames = 14, AirDashFrames = 10, JumpSpeed = 20, Gravity = 72,
            MaxFallSpeed = 26, FastFallSpeed = 36, MaxJumps = 2, MaxAirDashes = 1,
            GroundNormals = new[] { Normal("地上1段"), Normal("地上2段"), Normal("地上3段") },
            AirNormals = new[] { Normal("空中1段"), Normal("空中2段"), Normal("空中3段") },
            Specials = new[] { PhaseTwoDefaults.Special(SpecialSlot.Neutral), PhaseTwoDefaults.Special(SpecialSlot.Side),
                PhaseTwoDefaults.Special(SpecialSlot.Up), PhaseTwoDefaults.Special(SpecialSlot.Down) },
            Ultimate = PhaseTwoDefaults.Ultimate()
        };
        public static StageGeometry Stage() => new StageGeometry(new[]
        {
            new StagePlatform(new BattleRect(new BattleVector(0, -0.6f), new BattleVector(36, 1.2f)), false),
            new StagePlatform(new BattleRect(new BattleVector(-8, 4), new BattleVector(7, 0.35f)), true),
            new StagePlatform(new BattleRect(new BattleVector(8, 4), new BattleVector(7, 0.35f)), true)
        }, -24, 24, -10, new BattleVector(-5, 1.1f), new BattleVector(5, 1.1f));
    }
}
