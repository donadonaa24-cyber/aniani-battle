namespace AniBato.Battle
{
    public static class PhaseTwoDefaults
    {
        public static DefenseDefinition Defense() => new DefenseDefinition
        {
            GuardReleaseFrames = 6, GuardStunFrames = 10, GuardPushSpeed = 3, JustWindowFrames = 5,
            JustGuardStunFrames = 2, JustAttackerStunFrames = 12,
            DodgeInvulnerableFrames = 12, DodgeRecoveryFrames = 12, JustDodgeRecoveryFrames = 3,
            GroundDodgeSpeed = 15, AirDodgeSpeed = 12,
            CounterWindowFrames = 8, CounterFailureFrames = 24, CounterSuccessFrames = 6,
            CounterCooldownFrames = 45, CounterTargetStunFrames = 14, CounterTargetImmunityFrames = 12,
            CounterKnockback = new BattleVector(4, 3)
        };
        public static GaugeDefinition Gauge() => new GaugeDefinition
        { Maximum = 100, UltimateCost = 100, UltimateMaxUses = 1, OnHit = 14, OnDamaged = 10, OnCounter = 20, OnJustGuard = 18, OnJustDodge = 18 };
        public static AttackDefinition Special(SpecialSlot slot)
        {
            var attack = new AttackDefinition
            {
                Name = "仮特殊 " + slot, Kind = AttackKind.Special, IsCloseRange = slot != SpecialSlot.Neutral,
                StartupFrames = 6, ActiveFrames = 8, RecoveryFrames = 18,
                Hitbox = new BattleRect(new BattleVector(1.3f, 0), new BattleVector(2.4f, 2.2f)),
                HitstopFrames = 3, HitstunFrames = 12, Knockback = new BattleVector(5, 4)
            };
            switch (slot)
            {
                case SpecialSlot.Neutral:
                    attack.SpawnProjectile = true; attack.ProjectileSpeed = 18; attack.ProjectileLifetimeFrames = 90;
                    attack.Hitbox = new BattleRect(new BattleVector(1.2f, 0.2f), new BattleVector(0.9f, 0.9f)); break;
                case SpecialSlot.Side: attack.ActiveVelocity = new BattleVector(22, 0); break;
                case SpecialSlot.Up:
                    attack.StartupFrames = 2; attack.ActiveFrames = 18; attack.ActiveVelocity = new BattleVector(4, 28);
                    attack.Hitbox = new BattleRect(new BattleVector(0, 1), new BattleVector(2, 3)); break;
                case SpecialSlot.Down:
                    attack.Hitbox = new BattleRect(default, new BattleVector(4, 2.6f)); break;
            }
            return attack;
        }
        public static AttackDefinition Ultimate() => new AttackDefinition
        {
            Name = "仮超必殺", Kind = AttackKind.Ultimate, IsCloseRange = true, PiercesNormalDodge = true,
            StartupFrames = 14, ActiveFrames = 4, RecoveryFrames = 60,
            Hitbox = new BattleRect(new BattleVector(2.2f, 0), new BattleVector(5, 3.5f)),
            HitstopFrames = 8, HitstunFrames = 24, Knockback = new BattleVector(12, 8)
        };
    }
}
