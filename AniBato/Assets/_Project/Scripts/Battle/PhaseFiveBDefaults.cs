namespace AniBato.Battle
{
    // Current playable tuning; earlier phase fixtures retain their timing for isolated rule tests.
    public static class PhaseFiveBDefaults
    {
        public static BattleRuleDefinition Rules()
        {
            var rules = PhaseOneDefaults.Rules(); rules.SchemaVersion = 5;
            rules.HeartRecoveryFrames = 75; rules.HeartRecoveryKnockback = new BattleVector(12, 8);
            rules.ProjectileLimitPerOwner = 1;
            var gauge = rules.Gauge; gauge.OnHit = 12; gauge.OnDamaged = 8; gauge.OnCounter = 16;
            gauge.OnJustGuard = gauge.OnJustDodge = 14; rules.Gauge = gauge; return rules;
        }
        public static CharacterDefinition Character(int index)
        {
            var character = PhaseThreeDefaults.Character(index);
            for (int slot = 0; slot < 4; slot++)
            {
                var attack = character.Specials[slot];
                if (slot != (int)SpecialSlot.Up && !attack.NonDamaging)
                {
                    attack.StartupFrames += 12; attack.RecoveryFrames += 18;
                    attack.CooldownFrames = 120; attack.GuardedRecoveryFrames = System.Math.Max(attack.GuardedRecoveryFrames, 48);
                }
                // Recovery specials remain responsive for returning to the stage.
                if (slot == (int)SpecialSlot.Up) attack.CooldownFrames = 90;
                character.Specials[slot] = attack;
            }
            return character;
        }
    }
}
