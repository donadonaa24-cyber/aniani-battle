namespace AniBato.Battle
{
    public static class BalanceMigration
    {
        public static BattleRuleDefinition Rules(BattleRuleDefinition value)
        {
            if (value.SchemaVersion >= 5) return value;
            var fresh = PhaseFiveBDefaults.Rules(); var gauge = value.Gauge;
            if (gauge.OnHit == 14 || gauge.OnHit == 12) gauge.OnHit = fresh.Gauge.OnHit;
            if (gauge.OnDamaged == 10 || gauge.OnDamaged == 8) gauge.OnDamaged = fresh.Gauge.OnDamaged;
            if (gauge.OnCounter == 20) gauge.OnCounter = fresh.Gauge.OnCounter;
            if (gauge.OnJustGuard == 18) gauge.OnJustGuard = fresh.Gauge.OnJustGuard;
            if (gauge.OnJustDodge == 18) gauge.OnJustDodge = fresh.Gauge.OnJustDodge;
            value.Gauge = gauge;
            if (value.HeartRecoveryFrames == 0) value.HeartRecoveryFrames = fresh.HeartRecoveryFrames;
            if (value.HeartRecoveryKnockback.X == 0 && value.HeartRecoveryKnockback.Y == 0) value.HeartRecoveryKnockback = fresh.HeartRecoveryKnockback;
            if (value.ProjectileLimitPerOwner == 0) value.ProjectileLimitPerOwner = fresh.ProjectileLimitPerOwner;
            value.SchemaVersion = 5; return value;
        }
        public static CpuDefinition Cpu(CpuDefinition value, int difficulty)
        {
            if (value.SchemaVersion >= 5) return value;
            var old = CpuDefinition.LegacyDifficulty(difficulty); var fresh = CpuDefinition.ForDifficulty(difficulty);
            if (value.GuardChance == old.GuardChance) value.GuardChance = fresh.GuardChance;
            if (value.DodgeChance == old.DodgeChance) value.DodgeChance = fresh.DodgeChance;
            if (value.CounterChance == old.CounterChance) value.CounterChance = fresh.CounterChance;
            if (value.JustChance == old.JustChance) value.JustChance = fresh.JustChance;
            if (value.SpecialChance == old.SpecialChance) value.SpecialChance = fresh.SpecialChance;
            if (value.AttackRestFrames == 0) value.AttackRestFrames = fresh.AttackRestFrames;
            if (value.WhiffApproachAttempts == 0) value.WhiffApproachAttempts = fresh.WhiffApproachAttempts;
            value.SchemaVersion = 5; return value;
        }
        public static AttackDefinition Special(AttackDefinition value, AttackDefinition old, AttackDefinition fresh)
        {
            if (value.StartupFrames == old.StartupFrames) value.StartupFrames = fresh.StartupFrames;
            if (value.RecoveryFrames == old.RecoveryFrames) value.RecoveryFrames = fresh.RecoveryFrames;
            if (value.CooldownFrames == old.CooldownFrames) value.CooldownFrames = fresh.CooldownFrames;
            if (value.GuardedRecoveryFrames == old.GuardedRecoveryFrames) value.GuardedRecoveryFrames = fresh.GuardedRecoveryFrames;
            return value;
        }
    }
}
