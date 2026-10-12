namespace AniBato.Battle
{
    public static class PhaseThreeDefaults
    {
        public static CharacterDefinition Character(int index)
        {
            var c = PhaseOneDefaults.Character(index == 0 ? "透真" : index == 1 ? "蓮" : "千鶴");
            c.MoveSpeed = index == 0 ? 10 : index == 1 ? 10.5f : 9;
            c.DashSpeed = index == 0 ? 19 : index == 1 ? 20 : 16;
            c.JumpSpeed = index == 0 ? 21 : index == 1 ? 20 : 19;
            c.GroundDashFrames = index == 1 ? 12 : 14;
            c.AirDashFrames = index == 0 ? 12 : 10;
            c.GroundNormals = new AttackDefinition[3]; c.AirNormals = new AttackDefinition[3];
            for (int i = 0; i < 3; i++)
            {
                var a = PhaseOneDefaults.Normal(c.Name + " 地上" + (i + 1));
                a.StartupFrames = (index == 0 ? 3 : index == 1 ? 4 : 5) + i;
                a.RecoveryFrames = (index == 0 ? 7 : index == 1 ? 8 : 9) + (i == 2 ? 3 : 0);
                a.Hitbox = Rect(index == 0 ? 1.3f : index == 1 ? 1.6f : 1.8f, 0.1f,
                    index == 0 ? 2 : index == 1 ? 2.6f : 3, 1.8f);
                a.Knockback = new BattleVector(index == 0 ? 1.5f : index == 1 ? 2.5f : 2, 0);
                c.GroundNormals[i] = a;
                a.Name = c.Name + " 空中" + (i + 1); a.StartupFrames++; a.RecoveryFrames++;
                a.Hitbox = Rect(a.Hitbox.Center.X, -0.2f, a.Hitbox.Size.X, 2.2f);
                c.AirNormals[i] = a;
            }
            c.Specials = new AttackDefinition[4];
            if (index == 0) Touma(ref c);
            else if (index == 1) Ren(ref c);
            else Chizuru(ref c);
            c.UltimateCutInFrames = 30;
            return c;
        }
        private static BattleRect Rect(float x, float y, float w, float h) => new BattleRect(new BattleVector(x, y), new BattleVector(w, h));
        private static AttackDefinition Special(string name, int startup, int active, int recovery, BattleRect hitbox, BattleVector motion = default)
        {
            var a = PhaseTwoDefaults.Special(SpecialSlot.Side);
            a.Name = name; a.StartupFrames = startup; a.ActiveFrames = active; a.RecoveryFrames = recovery;
            a.Hitbox = hitbox; a.ActiveVelocity = motion; return a;
        }
        private static AttackDefinition Ultimate(string name, int startup, int active, int recovery, BattleRect hitbox, BattleVector motion = default)
        {
            var a = PhaseTwoDefaults.Ultimate(); a.Name = name;
            a.StartupFrames = startup; a.ActiveFrames = active; a.RecoveryFrames = recovery;
            a.Hitbox = hitbox; a.ActiveVelocity = motion; return a;
        }
        private static void Touma(ref CharacterDefinition c)
        {
            var pulse = Special("観測パルス", 6, 1, 18, Rect(1.1f, 0.2f, 0.9f, 0.9f));
            pulse.IsCloseRange = false; pulse.SpawnProjectile = true; pulse.ProjectileSpeed = 14;
            pulse.ProjectileLifetimeFrames = 30; pulse.MarkFrames = 90;
            c.Specials[0] = pulse;
            // The startup movement is intentionally absent; only the last short active step strikes.
            var step = Special("遮断ステップ", 3, 6, 20, Rect(1.1f, 0, 1.6f, 2), new BattleVector(28, 0));
            step.DamageDelayFrames = 4; c.Specials[1] = step;
            c.Specials[2] = Special("緊急上昇", 2, 18, 22, Rect(0, 0.8f, 1.2f, 1.5f), new BattleVector(2, 30));
            var field = Special("遮断フィールド", 4, 18, 20, Rect(0, 0, 4, 3));
            field.ProjectileEraseCount = -1; c.Specials[3] = field;
            c.Ultimate = Ultimate("境界突破", 8, 12, 66, Rect(1.3f, 0, 2.4f, 2.8f), new BattleVector(38, 0));
            c.Counter = new CounterEffectDefinition
            { Buff = new BuffDefinition { DurationFrames = 180, MoveMultiplier = 1.25f, DashMultiplier = 1.3f, StartupMultiplier = 1 } };
        }
        private static void Ren(ref CharacterDefinition c)
        {
            c.Specials[0] = Special("飛車ライン", 10, 4, 24, Rect(3.5f, 0, 7, 1.2f));
            var rush = Special("直線番長", 5, 14, 28, Rect(1.2f, 0, 2.2f, 2.2f), new BattleVector(32, 0));
            rush.GuardedRecoveryFrames = 42; c.Specials[1] = rush;
            c.Specials[2] = Special("縦抜け", 2, 18, 24, Rect(0, 1, 1.6f, 2.6f), new BattleVector(0, 29));
            var stance = Special("飛車の構え・急襲", 12, 10, 24, Rect(1.5f, 0, 2.8f, 2), new BattleVector(34, 0));
            stance.RequiresFollowup = true; stance.StanceFrames = 60; c.Specials[3] = stance;
            var cross = Ultimate("十字突破", 12, 5, 72, Rect(0, 0, 12, 2));
            cross.HasSecondaryHitbox = true; cross.SecondaryHitbox = Rect(0, 0, 2, 10); c.Ultimate = cross;
            c.Counter = new CounterEffectDefinition { KnockbackMultiplier = 2 };
        }
        private static void Chizuru(ref CharacterDefinition c)
        {
            var rice = Special("爆弾おにぎり", 8, 1, 22, Rect(1.2f, 0.3f, 0.9f, 0.9f));
            rice.IsCloseRange = false; rice.SpawnProjectile = true; rice.ProjectileSpeed = 10;
            rice.ProjectileLaunchVelocity = new BattleVector(0, 8); rice.ProjectileGravity = 24;
            rice.ProjectileLifetimeFrames = 60; rice.ParalysisFrames = 30; c.Specials[0] = rice;
            var pan = Special("フライパン突撃", 6, 10, 26, Rect(1.3f, 0, 2.6f, 2.2f), new BattleVector(22, 0));
            pan.ProjectileEraseCount = 1; c.Specials[1] = pan;
            c.Specials[2] = Special("おたま旋風", 3, 20, 24, Rect(0, 0.7f, 2.4f, 2.6f), new BattleVector(1, 27));
            var meal = Special("ひとくち休憩", 24, 1, 35, Rect(0, 0, 1, 1));
            meal.NonDamaging = true; meal.SelfHealNormalHits = 1; meal.CooldownFrames = 240; c.Specials[3] = meal;
            c.Ultimate = Ultimate("満腹カレー！", 16, 6, 72, Rect(1.5f, 0, 10, 5));
            c.Counter = new CounterEffectDefinition { SelfHealNormalHits = 1 };
        }
    }
}
