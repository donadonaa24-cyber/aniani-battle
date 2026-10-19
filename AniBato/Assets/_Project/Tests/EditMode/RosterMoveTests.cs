using System;
using System.Globalization;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class RosterMoveTests
    {
        private static BattleSetup Setup => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static BattleRuleDefinition Rules => AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
        private static BattleSimulation Create(int index, bool ultimate = false, float targetX = 1.5f)
        {
            var c = Setup.Character(index).ToDefinition(); c.UltimateCutInFrames = 0;
            if (ultimate) { var a = c.Ultimate; a.StartupFrames = 0; c.Ultimate = a; }
            else { var a = c.Specials[0]; a.StartupFrames = 0; c.Specials[0] = a; }
            var sim = BattleTestFactory.Create(true, Rules, c);
            sim.P2.Position = new BattleVector(targetX, sim.P2.Position.Y); sim.P1.Gauge = Rules.Gauge.Maximum;
            return sim;
        }
        private static void Fire(BattleSimulation sim, bool ultimate) => sim.Tick(new FrameInput(pressed: ultimate ? InputButtons.Ultimate : InputButtons.Special), default);
        [Test]
        public void AllNineHaveIndependentLiveMovesAndTheRequestedWeapons()
        {
            var roster = Setup.Characters;
            Assert.That(roster.All(c => c.MoveSetSchemaVersion >= 1 && !c.PrototypeSharedMoves), Is.True);
            Assert.That(roster.SelectMany(c => c.GroundNormals).Distinct().Count(), Is.EqualTo(27));
            Assert.That(roster.SelectMany(c => c.AirNormals).Distinct().Count(), Is.EqualTo(27));
            Assert.That(roster.SelectMany(c => new[] { c.DirectionalNormals[1], c.DirectionalNormals[2], c.DirectionalNormals[3] }).Distinct().Count(), Is.EqualTo(27));
            Assert.That(roster.Select(c => c.UpNormal).Distinct().Count(), Is.EqualTo(9));
            Assert.That(roster.Select(c => c.Specials[0]).Distinct().Count(), Is.EqualTo(9));
            Assert.That(roster.Select(c => c.Ultimate).Distinct().Count(), Is.EqualTo(9));
            Assert.That(roster[0].GroundNormals.All(a => a.Definition.SpawnProjectile && !a.Definition.IsCloseRange && a.Definition.Ability.Look == AbilityLook.Bullet), Is.True);
            for(int i=1;i<roster.Length;i++)for(int stage=0;stage<3;stage++)
            {
                var normal=roster[i].GroundNormals[stage].Definition;
                Assert.That(normal.SpawnProjectile,Is.EqualTo(i==7&&stage==2),roster[i].name+" normal "+stage);
                if(i==7&&stage==2){Assert.That(normal.Ability.Look,Is.EqualTo(AbilityLook.Card));Assert.That(normal.IsCloseRange,Is.False);}
            }
            AbilityLook[] special = { AbilityLook.Drone, AbilityLook.Rook, AbilityLook.RiceBall, AbilityLook.Teleport, AbilityLook.Emp, AbilityLook.Bishop, AbilityLook.King, AbilityLook.Card, AbilityLook.Bat };
            AbilityLook[] supers = { AbilityLook.Rocket, AbilityLook.Rook, AbilityLook.Curry, AbilityLook.BlackHole, AbilityLook.Alma, AbilityLook.Bishop, AbilityLook.King, AbilityLook.Pyramid, AbilityLook.Spell };
            for (int i = 0; i < 9; i++)
            {
                Assert.That(roster[i].Specials[0].Definition.Ability.Look, Is.EqualTo(special[i]));
                Assert.That(roster[i].Ultimate.Definition.Ability.Look, Is.EqualTo(supers[i]));
                Assert.DoesNotThrow(() => roster[i].ToDefinition());
            }
            var rice = roster[2].Specials[0].Definition;
            Assert.That(rice.Hitbox.Size, Is.EqualTo(AssetDatabase.LoadAssetAtPath<AttackData>("Assets/_Project/Data/ChizuruSpecial1.asset").Definition.Hitbox.Size));
            Assert.That(rice.ParalysisFrames, Is.GreaterThan(0)); Assert.That(roster[2].PaperMotion.RiceBall, Is.Not.Null);
        }
        [Test]
        public void AllFortyFiveOriginalHitboxAssetsKeepClaudesHalvedBounds()
        {
            string path = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../../docs/HITBOX_REACH_20261015.csv"));
            var rows = File.ReadAllLines(path).Skip(1).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Split(',')).ToArray();
            Assert.That(rows.Select(r => r[0]).Distinct().Count(), Is.EqualTo(45));
            foreach (var row in rows)
            {
                var data = AssetDatabase.LoadAssetAtPath<AttackData>("Assets/_Project/Data/" + row[0] + ".asset"); Assert.That(data, Is.Not.Null);
                var box = row[1] == "main" ? data.Definition.Hitbox : data.Definition.SecondaryHitbox;
                Assert.That(box.Left, Is.EqualTo(float.Parse(row[4], CultureInfo.InvariantCulture)).Within(.0001), row[0]);
                Assert.That(box.Right, Is.EqualTo(float.Parse(row[5], CultureInfo.InvariantCulture)).Within(.0001), row[0]);
            }
            Assert.That(Rules.Gauge.OnHit, Is.EqualTo(14)); Assert.That(Rules.Gauge.OnDamaged, Is.EqualTo(10));
        }
        [Test]
        public void TeleportUsesOneActivationAndRespectsRangeAndSolidDestinations()
        {
            var sim = Create(3, targetX: 6); sim.P1.Position = new BattleVector(-4, 1.1f);
            Fire(sim, false); Assert.That(sim.P1.Position.X, Is.EqualTo(4).Within(.0001));
            float start = sim.P1.Position.X;
            BattleTestFactory.Idle(sim, 8); Assert.That(sim.P1.Position.X, Is.EqualTo(start).Within(.0001));
            sim = Create(3, targetX: 15); sim.P1.Position = new BattleVector(-10, 1.1f);
            Fire(sim, false); Assert.That(sim.P1.Position.X, Is.EqualTo(-2).Within(.0001));
            sim = Create(3); sim.P2.Position = new BattleVector(2, -.6f); var before = sim.P1.Position;
            Fire(sim, false); Assert.That(sim.P1.Position, Is.EqualTo(before));
        }
        [TestCase(1)] [TestCase(-1)]
        public void RookRushIsFastAndCannotDriveItsOwnerPastTheMainFloor(int facing)
        {
            var sim = Create(1, targetX: 0); sim.P1.Facing = facing; sim.P1.Position = new BattleVector(17 * facing, 1.1f);
            Fire(sim, false); Assert.That(Math.Abs(sim.P1.Position.X), Is.EqualTo(17.45f).Within(.001));
            for (int i = 0; i < 12; i++) { sim.Tick(default, default); Assert.That(Math.Abs(sim.P1.Position.X), Is.LessThanOrEqualTo(17.451)); }
            Assert.That(sim.Stage.IsOutside(sim.P1.Position), Is.False);
        }
        [Test]
        public void BishopAscendsThenDescendsInOneAttack()
        {
            var sim = Create(5, targetX: 15); float initial = sim.P1.Position.Y; Fire(sim, false);
            Assert.That(sim.P1.Velocity.Y, Is.GreaterThan(0)); var id = sim.P1.Attack.Id;
            BattleTestFactory.Idle(sim, 8); Assert.That(sim.P1.Position.Y, Is.GreaterThan(initial + 1));
            sim.Tick(default, default); Assert.That(sim.P1.Velocity.Y, Is.LessThan(0)); Assert.That(sim.P1.Attack.Id, Is.EqualTo(id));
            BattleTestFactory.Idle(sim, 45); Assert.That(sim.P1.Grounded, Is.True); Assert.That(sim.P1.Position.X, Is.GreaterThan(0));
        }
        [TestCase(0, AbilityLook.Rocket)] [TestCase(7, AbilityLook.Pyramid)]
        public void RocketAndPyramidFallAboveTheTargetAndDamageOnlyOnce(int character, AbilityLook look)
        {
            var sim = Create(character, true); Fire(sim, true);
            Assert.That(sim.Projectiles.Count, Is.EqualTo(1)); var shot = sim.Projectiles[0];
            Assert.That(shot.Attack.Definition.Ability.Look, Is.EqualTo(look));
            Assert.That(shot.Position.X, Is.EqualTo(sim.P2.Position.X)); Assert.That(shot.Position.Y, Is.GreaterThan(sim.P2.Position.Y + 10));
            Assert.That(shot.Velocity.Y, Is.LessThan(0)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            int hits = 0;
            for (int i = 0; i < 110; i++) { sim.Tick(default, default); hits += sim.Events.Count(e => e.Kind == BattleEventKind.Hit && e.SourcePlayer == 0); }
            Assert.That(hits, Is.EqualTo(1)); Assert.That(sim.P2.Hearts, Is.EqualTo(1)); Assert.That(sim.Projectiles, Is.Empty);
        }
        [Test]
        public void ThreeKingSummonsShareOneDamageInstanceEvenWhenAllOverlap()
        {
            var c = Setup.Character(6).ToDefinition(); var a = c.Specials[0]; a.StartupFrames = 0; c.Specials[0] = a;
            var wide = c; wide.HurtboxSize = new BattleVector(5, wide.HurtboxSize.Y);
            var sim = new BattleSimulation(c, wide, Rules, PhaseOneDefaults.Stage()); Fire(sim, false);
            Assert.That(sim.Projectiles.Count, Is.EqualTo(3)); Assert.That(sim.Projectiles.Select(p => p.Attack).Distinct().Count(), Is.EqualTo(1));
            int hits = 0; for (int i = 0; i < 110; i++) { sim.Tick(default, default); hits += sim.Events.Count(e => e.Kind == BattleEventKind.Hit); }
            Assert.That(hits, Is.EqualTo(1)); Assert.That(sim.P2.Hearts, Is.EqualTo(2));
        }
        [Test]
        public void ProjectileLimitCountsFormationsWithoutAllowingAnotherAttackToStack()
        {
            var sim = Create(6); var shots = new ProjectileSystem();
            var definition = sim.P1.Definition.Specials[0]; definition.StartupFrames = 0;
            sim.P1.Attack = new AttackInstance(80, definition, 0, false);
            shots.Step(sim.P1,sim.P2,1,sim.Stage); Assert.That(shots.Projectiles.Count, Is.EqualTo(3));
            sim.P1.Attack = new AttackInstance(81, definition, 0, false);
            shots.Step(sim.P1,sim.P2,1,sim.Stage);
            Assert.That(shots.Projectiles.Count, Is.EqualTo(3));
            Assert.That(shots.Projectiles.All(p => p.Attack.Id == 80), Is.True);
        }
        [Test]
        public void GunFiresAllThreeComboStepsWithoutCancellingItsDrone()
        {
            var sim = Create(0, targetX: 16); sim.P1.Position = new BattleVector(-16,sim.P1.Position.Y);
            Fire(sim,false); BattleTestFactory.Idle(sim,27);
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.Projectiles.Count, Is.EqualTo(1));
            var drone = sim.Projectiles[0]; var bullets = new System.Collections.Generic.HashSet<long>();
            bool[] queued = new bool[3]; bool sawBoth = false;
            for (int tick = 0; tick < 100; tick++)
            {
                bool press = tick == 0;
                var attack = sim.P1.Attack;
                if (attack != null && attack.Definition.IsNormal && attack.ChainIndex < 2 && !queued[attack.ChainIndex])
                { queued[attack.ChainIndex] = true; press = true; }
                sim.Tick(new FrameInput(pressed: press ? InputButtons.Attack : InputButtons.None),default);
                var gun = sim.Projectiles.Where(p => p.Attack.Definition.Ability.Look == AbilityLook.Bullet).ToArray();
                Assert.That(gun.Length, Is.LessThanOrEqualTo(1));
                foreach (var shot in gun) bullets.Add(shot.Attack.Id);
                if (gun.Length > 0 && sim.Projectiles.Contains(drone)) sawBoth = true;
            }
            Assert.That(bullets.Count, Is.EqualTo(3)); Assert.That(sawBoth, Is.True);
            Assert.That(sim.P2.NormalHitCount, Is.Zero); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
        }
        [Test]
        public void AllNineCpuMoveSetsFinishAllPairingsWithCombat()
        {
            var rows = CpuBalanceStudy.Run(Setup.Characters.Select(c => c.ToDefinition()).ToArray(), Rules,
                Setup.Stages[0].ToGeometry(), new[] { Setup.CpuDifficulties[1].ToDefinition() }, seeds: 3);
            Assert.That(rows.Count, Is.EqualTo(243));
            string path = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../TestResults/rework_r3_roster_cpu.csv"));
            File.WriteAllText(path,CpuBalanceStudy.Csv(rows));
            Assert.That(rows.All(r => r.Attacks[0] > 0 && r.Attacks[1] > 0 && r.HeartsLost > 0), Is.True,
                string.Join("; ", rows.Where(r => r.Attacks[0] == 0 || r.Attacks[1] == 0 || r.HeartsLost == 0)
                    .Select(r => $"P{r.Player1}/P{r.Player2} seed{r.Seed}: attacks={r.Attacks[0]}/{r.Attacks[1]}, hearts={r.HeartsLost}, reason={r.Reason}")));
        }
        [Test]
        public void BlackHolePersistsAfterRecoveryAndDamagesOnContactOnce()
        {
            var sim = Create(3, true, 12); Fire(sim, true); Assert.That(sim.Projectiles.Count, Is.EqualTo(1));
            var shot = sim.Projectiles[0]; var position = shot.Position;
            Assert.That(shot.Attack.Definition.Ability.ProjectilePullRadius, Is.LessThan(12), "引力範囲外で持続と接触のみを検査する");
            BattleTestFactory.Idle(sim, 50); Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.Projectiles.Count, Is.EqualTo(1));
            Assert.That(shot.Position, Is.EqualTo(position)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            sim.P2.Position = new BattleVector(0, 1.1f); sim.Tick(default, default);
            Assert.That(sim.P2.Hearts, Is.EqualTo(1));
            for (int i = 0; i < 90; i++) { sim.P2.Position = new BattleVector(0, 1.1f); sim.Tick(default, default); }
            Assert.That(sim.P2.Hearts, Is.EqualTo(1)); BattleTestFactory.Idle(sim, 50); Assert.That(sim.Projectiles, Is.Empty);
        }
        [TestCase(4)] [TestCase(8)]
        public void AlmaAndBookSpellHoldTheirCueBeforeTheDamageWindow(int character)
        {
            var sim = Create(character, true); var definition = sim.P1.Definition.Ultimate;
            Fire(sim, true); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            for (int i = 1; i < definition.DamageDelayFrames; i++) { sim.Tick(default, default); Assert.That(sim.P2.Hearts, Is.EqualTo(3)); }
            sim.Tick(default, default); Assert.That(sim.P2.Hearts, Is.EqualTo(1));
        }
        [Test]
        public void NewProjectileTimingBackShotsAndReplacementAreExplicitData()
        {
            var sim = Create(0); var c = sim.P1.Definition;
            var attack = c.GroundNormals[0]; attack.StartupFrames = 0; attack.ActiveFrames = 6; attack.DamageDelayFrames = 3;
            var instance = new AttackInstance(90, attack, 0, false); sim.P1.Attack = instance;
            var shots = new ProjectileSystem(); shots.Step(sim.P1,sim.P2,3,sim.Stage); Assert.That(shots.Projectiles, Is.Empty);
            instance.Frame = 3; shots.Step(sim.P1,sim.P2,3,sim.Stage); Assert.That(shots.Projectiles.Count, Is.EqualTo(1));
            var back = c.DirectionalNormals[2]; var backShot = new Projectile(sim.P1,new AttackInstance(91,back,0,false));
            Assert.That(backShot.Facing, Is.EqualTo(-sim.P1.Facing)); Assert.That(backShot.Velocity.X, Is.LessThan(0));
            var ultimate = c.Ultimate; ultimate.StartupFrames = 0;
            sim.P1.Attack = new AttackInstance(92,ultimate,0,false); shots.Step(sim.P1,sim.P2,3,sim.Stage);
            Assert.That(shots.Projectiles.Count, Is.EqualTo(1)); Assert.That(shots.Projectiles[0].Attack.Id, Is.EqualTo(92));
        }
    }
}
