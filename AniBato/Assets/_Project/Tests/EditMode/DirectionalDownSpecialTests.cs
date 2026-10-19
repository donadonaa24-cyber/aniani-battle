using System.Linq;
using AniBato.Battle;
using AniBato.EditorTools;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class DirectionalDownSpecialTests
    {
        private static BattleSetup Setup => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static BattleSimulation Create(int index, int? cooldown = null)
        {
            var c = Setup.Characters[index].ToDefinition();
            if (cooldown.HasValue) { var a = c.Specials[3]; a.CooldownFrames = cooldown.Value; c.Specials[3] = a; }
            var r = PhaseOneDefaults.Rules(); r.CombatInput = CombatInputDefinition.Default; r.CombatInput.DownSpecialEnabled = true;
            return BattleTestFactory.Create(false, r, c);
        }
        private static void StartDown(BattleSimulation sim) => sim.Tick(new FrameInput(y: -1, pressed: InputButtons.Special), default);
        private static Projectile Place(BattleSimulation sim)
        {
            StartDown(sim); for (int tick = 0; sim.Projectiles.Count == 0 && tick < 40; tick++) sim.Tick(default, default);
            Assert.That(sim.Projectiles.Count, Is.GreaterThan(0)); return sim.Projectiles.Last();
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void AllNineHaveAUniqueDownMoveAndIndependentCooldown(int index)
        {
            var sim = Create(index); var expected = sim.P1.Definition.Special(SpecialSlot.Down); expected.Validate();
            StartDown(sim);
            Assert.That(sim.P1.CurrentSpecial, Is.EqualTo(SpecialSlot.Down));
            Assert.That(sim.P1.Attack.Definition.Name, Is.EqualTo(expected.Name));
            Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Down), Is.EqualTo(expected.CooldownFrames));
            Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Neutral), Is.Zero); Assert.That(sim.P1.UpSpecialUsed, Is.False);
            Assert.That(sim.P1.Attack.Definition.DamageMode, Is.EqualTo(AttackDamageMode.NormalAccumulation));
            Assert.That(SpecialCooldownDisplay.Text(sim.P1, sim.Rules.CombatInput), Does.Contain("↓"));
        }
        [TestCase(0,1)] [TestCase(0,-1)] [TestCase(4,1)] [TestCase(4,-1)]
        public void TeleportUsesTheOpponentsFacingAndThenFacesThem(int index, int facing)
        {
            var sim = Create(index); sim.P2.Facing = facing; var target = sim.P2.Position;
            StartDown(sim); BattleTestFactory.Idle(sim, 15);
            Assert.That(sim.P1.Position.X, Is.EqualTo(target.X - facing * 1.6f).Within(.001));
            Assert.That(sim.P1.Facing, Is.EqualTo(facing)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Assert.That(sim.P2.NormalHitCount, Is.Zero);
        }
        [Test]
        public void MealLocksFor150TicksHealsOnlyOnCompletionAndStartsATwentySecondCooldown()
        {
            var sim = Create(2); sim.P1.NormalHitCount = 2; StartDown(sim);
            for (int tick = 1; tick < 150; tick++)
            {
                sim.Tick(new FrameInput(1), default);
                Assert.That(sim.P1.NormalHitCount, Is.EqualTo(2)); Assert.That(sim.P1.Attack, Is.Not.Null);
                Assert.That(sim.P1.Position.X, Is.EqualTo(sim.Stage.SpawnP1.X));
            }
            sim.Tick(default, default); Assert.That(sim.P1.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P1.Attack, Is.Null);
            Assert.That(sim.P1.Hearts, Is.EqualTo(3)); Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Down), Is.EqualTo(1050));
            StartDown(sim); Assert.That(sim.P1.Attack, Is.Null);
            BattleTestFactory.Idle(sim, 1050); StartDown(sim); Assert.That(sim.P1.Attack, Is.Not.Null);
        }
        [Test]
        public void InterruptedMealDoesNotGiveAFreeHeal()
        {
            var sim = Create(2); sim.P1.NormalHitCount = 2; StartDown(sim); BattleTestFactory.Idle(sim, 40);
            sim.P1.Attack = null; sim.P1.Mode = FighterMode.Hitstun; sim.P1.HitstunRemaining = 10;
            BattleTestFactory.Idle(sim, 180); Assert.That(sim.P1.NormalHitCount, Is.EqualTo(2)); Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Down), Is.GreaterThan(0));
        }
        [Test]
        public void RyutaGetsFiveSecondSpeedBoostAfterReadingEnds()
        {
            var sim = Create(8); StartDown(sim); BattleTestFactory.Idle(sim, 44);
            Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1)); sim.Tick(default, default);
            Assert.That(sim.P1.BuffRemaining, Is.EqualTo(300)); Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1.5f)); Assert.That(sim.P1.DashMultiplier, Is.EqualTo(1.5f));
            BattleTestFactory.Idle(sim, 299); Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1.5f));
            sim.Tick(default, default); Assert.That(sim.P1.MoveMultiplier, Is.EqualTo(1));
        }
        [TestCase(3)] [TestCase(7)]
        public void PlacedTrapOnlyDamagesOpponentForOneThirdAndDisappearsOnHit(int index)
        {
            var sim = Create(index); var trap = Place(sim); Assert.That(trap.RemainingFrames, Is.EqualTo(1200));
            Assert.That(trap.Position.Y - trap.Hitbox.Size.Y * .5f, Is.EqualTo(sim.Stage[0].Bounds.Top).Within(.001));
            var contacts = new System.Collections.Generic.List<HitContact>(); new HitResolver().Gather(trap, sim.P1, contacts); Assert.That(contacts, Is.Empty);
            sim.P2.Position = new BattleVector(trap.Position.X, sim.P2.Position.Y);
            sim.Tick(default, default);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Assert.That(sim.P1.NormalHitCount, Is.Zero); Assert.That(sim.Projectiles, Is.Empty); Assert.That(sim.P1.PlacedCount, Is.Zero);
            BattleTestFactory.Idle(sim, 60); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1));
        }
        [Test]
        public void AkatsukiHasTwoTrapsAndCannotStartAThirdUntilOneIsConsumed()
        {
            var sim = Create(7, 0); Place(sim); BattleTestFactory.Idle(sim, 40); StartDown(sim); BattleTestFactory.Idle(sim, 40);
            Assert.That(sim.P1.PlacedCount, Is.EqualTo(2)); Assert.That(sim.Projectiles.Count, Is.EqualTo(2));
            StartDown(sim); Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.Projectiles.Count, Is.EqualTo(2));
            Assert.That(SpecialCooldownDisplay.Text(sim.P1, sim.Rules.CombatInput), Does.Contain("設置 2/2"));
            sim.P2.Position = new BattleVector(sim.Projectiles[0].Position.X, sim.P2.Position.Y); sim.Tick(default, default);
            Assert.That(sim.P1.PlacedCount, Is.LessThan(2)); BattleTestFactory.Idle(sim, 40); StartDown(sim); Assert.That(sim.P1.Attack, Is.Not.Null);
        }
        [Test]
        public void TrapGuardDoesNotConsumeItOrPermanentlyDisarmIt()
        {
            var sim = Create(7); var trap = Place(sim); sim.P2.Position = new BattleVector(trap.Position.X, sim.P2.Position.Y);
            sim.P2.Mode = FighterMode.Guard; sim.P2.DefenseAge = 20;
            sim.Tick(default, new FrameInput(held: InputButtons.Guard)); Assert.That(sim.Projectiles.Count, Is.EqualTo(1)); Assert.That(sim.P2.NormalHitCount, Is.Zero);
            BattleTestFactory.Idle(sim, 40); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.Projectiles, Is.Empty);
        }
        [Test]
        public void TwoAkatsukisEachHaveTwoIndependentSlotsAndCanCrossTheirOwnCardsSafely()
        {
            var sim = Create(7, 0);
            for (int count = 0; count < 2; count++)
            {
                var down = new FrameInput(y: -1, pressed: InputButtons.Special);
                sim.Tick(down, down); BattleTestFactory.Idle(sim, 40);
            }
            Assert.That(sim.Projectiles.Count, Is.EqualTo(4));
            Assert.That(sim.P1.PlacedCount, Is.EqualTo(2)); Assert.That(sim.P2.PlacedCount, Is.EqualTo(2));
            sim.Tick(new FrameInput(y: -1, pressed: InputButtons.Special), new FrameInput(y: -1, pressed: InputButtons.Special));
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P2.Attack, Is.Null);
            var trap = sim.Projectiles.First(p => p.Owner == sim.P1);
            sim.P1.Position = new BattleVector(trap.Position.X, sim.P1.Position.Y); sim.Tick(default, default);
            Assert.That(sim.P1.NormalHitCount, Is.Zero); Assert.That(sim.Projectiles.Count, Is.EqualTo(4));
            sim.P2.Position = sim.P1.Position; sim.Tick(default, default);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(2)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Assert.That(sim.P1.PlacedCount, Is.Zero); Assert.That(sim.P2.PlacedCount, Is.EqualTo(2));
        }
        [Test]
        public void CardDamageUsesTheSharedRecoilAndTheGeneratedMineHasNativeArtwork()
        {
            var setup = Setup;
            Assert.That(setup.Character(7).Specials[3].Definition.DamageRecoilDistance, Is.EqualTo(.18f));
            var art = setup.Character(3).PaperMotion.Projectile(AbilityLook.Mine);
            Assert.That(art, Is.Not.Null); Assert.That(art.Sprite.texture.width, Is.EqualTo(1774));
            Assert.That(art.Sprite.texture.height, Is.EqualTo(887)); Assert.That(art.FitToHitbox, Is.True);
        }
        [Test]
        public void UnhitCardExpiresAfterTwentySecondsAndDoesNotOccupyNeutralProjectileLimit()
        {
            var sim = Create(7); var trap = Place(sim); BattleTestFactory.Idle(sim, 40);
            sim.Tick(new FrameInput(pressed: InputButtons.Special), default); BattleTestFactory.Idle(sim, 11);
            Assert.That(sim.Projectiles.Any(p => p.Attack.Definition.Ability.ProjectileChannel == ProjectileChannel.Shared), Is.True);
            BattleTestFactory.Idle(sim, 1200); Assert.That(sim.Projectiles.Contains(trap), Is.False); Assert.That(sim.P1.PlacedCount, Is.Zero);
        }
        [TestCase(1)] [TestCase(5)] [TestCase(6)]
        public void SuperGuardBlocksNormalAndHasARepellingOneThirdAttack(int index)
        {
            var sim = Create(index); StartDown(sim); BattleTestFactory.Idle(sim, 6);
            Assert.That(sim.P1.Attack.IsActive && sim.P1.Attack.Definition.Ability.SuperGuard, Is.True);
            var normal = new AttackInstance(900, PhaseOneDefaults.Normal(), 0, false);
            Assert.That(new DefenseSystem().Evaluate(new HitContact(sim.P2, sim.P1, normal), sim.Rules), Is.EqualTo(DefenseOutcome.Guard));
            sim.P2.Position = sim.P1.Position + new BattleVector(1.5f, 0); sim.Tick(default, default);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1)); Assert.That(sim.P2.Hearts, Is.EqualTo(3)); Assert.That(sim.P2.Velocity.X, Is.GreaterThan(10));
        }
        [Test]
        public void CooldownDisplayRoundsUpAndShowsUnavailableRecovery()
        {
            var sim = Create(2); sim.P1.SpecialCooldowns[3] = 1; var policy = sim.Rules.CombatInput; policy.UpRecoveryEnabled = true; sim.P1.UpSpecialUsed = true;
            Assert.That(SpecialCooldownDisplay.Text(sim.P1, policy), Does.Contain("↓ 0.1秒")); Assert.That(SpecialCooldownDisplay.Text(sim.P1, policy), Does.Contain("↑ 着地待ち"));
        }
    }
}
