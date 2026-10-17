using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class RenderInterpolationTests
    {
        [Test]
        public void MovingAt144HzHasUniformMonotonicRenderStepsAndRawTicksDoNot()
        {
            var simulation = BattleTestFactory.Create(); var frames = new RenderSnapshots(); frames.Reset(simulation);
            var clock = new FixedTickClock(); float last = simulation.P1.Position.X, rawLast = last;
            float min = float.MaxValue, max = 0; int rawStationary = 0;
            var log = new System.Text.StringBuilder("frame,seconds,renderX,deltaX,simulationX\n");
            for (int i = 0; i < 72; i++)
            {
                clock.Advance(1d / 144, () => { frames.BeforeTick(simulation); simulation.Tick(new FrameInput(1), default); frames.AfterTick(simulation); }, 5);
                float position = frames.FighterPosition(0, clock.Fraction).X, step = position - last;
                log.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0},{1:R},{2:R},{3:R},{4:R}\n", i, (i + 1d) / 144, position, step, simulation.P1.Position.X);
                if (i > 4)
                { Assert.That(step, Is.GreaterThan(0)); min = System.Math.Min(min, step); max = System.Math.Max(max, step); if (simulation.P1.Position.X == rawLast) rawStationary++; }
                last = position; rawLast = simulation.P1.Position.X;
            }
            Assert.That(max - min, Is.LessThan(.00002f));
            Assert.That(rawStationary, Is.GreaterThan(30), "対照（補間なし）は144Hzで均一にならないこと。");
            string root = System.IO.Directory.GetCurrentDirectory();
            if (System.IO.Directory.Exists(System.IO.Path.Combine(root, "tools/Validation")))
                System.IO.File.WriteAllText(System.IO.Path.Combine(root, "TestResults/phase5b-render-144hz-dotnet.csv"), log.ToString());
        }
        [Test]
        public void PauseHitstopFlipTeleportAndResetSnapToCurrent()
        {
            var simulation = BattleTestFactory.Create(); var frames = new RenderSnapshots(); frames.Reset(simulation);
            frames.BeforeTick(simulation); simulation.P1.Position += new BattleVector(1, 0); frames.AfterTick(simulation);
            Assert.That(frames.FighterPosition(0, .25).X, Is.EqualTo(simulation.P1.Position.X - .75f));
            Assert.That(frames.FighterPosition(0, .25, true), Is.EqualTo(simulation.P1.Position));
            simulation.P1.HitstopRemaining = 3; frames.AfterTick(simulation);
            Assert.That(frames.FighterPosition(0, .25), Is.EqualTo(simulation.P1.Position));
            simulation.P1.HitstopRemaining = 0; simulation.P1.Facing = -1; frames.AfterTick(simulation);
            Assert.That(frames.FighterPosition(0, .25), Is.EqualTo(simulation.P1.Position));
            simulation.P1.Facing = 1; simulation.P1.Position += new BattleVector(10, 0); frames.AfterTick(simulation);
            Assert.That(frames.FighterPosition(0, .25), Is.EqualTo(simulation.P1.Position));
            frames.Reset(simulation); Assert.That(frames.FighterPosition(0, 0), Is.EqualTo(simulation.P1.Position));
            var rematch = BattleTestFactory.Create(); frames.Reset(rematch);
            Assert.That(frames.FighterPosition(0, .5), Is.EqualTo(rematch.P1.Position));
        }
        [Test]
        public void BoundedCatchupPreservesAllElapsedTime()
        {
            var clock = new FixedTickClock(); int ticks = 0;
            Assert.That(clock.Advance(.5, () => ticks++, 5), Is.EqualTo(5));
            Assert.That(clock.PendingSeconds, Is.EqualTo(25d / 60).Within(1e-9));
            Assert.That(clock.Fraction, Is.EqualTo(1));
            for (int i = 0; i < 5; i++) Assert.That(clock.Advance(0, () => ticks++, 5), Is.EqualTo(5));
            Assert.That(ticks, Is.EqualTo(30)); Assert.That(clock.PendingSeconds, Is.LessThan(1e-9));
        }
        [Test]
        public void ProjectileInterpolatesByProjectileIdentityAndNewShotSnaps()
        {
            var simulation = BattleTestFactory.Create(); var frames = new RenderSnapshots(); frames.Reset(simulation);
            var definition = simulation.P1.Definition.Specials[0]; definition.StartupFrames = 0; definition.ActiveFrames = 4; definition.ProjectileLifetimeFrames = 10;
            simulation.P1.Attack = new AttackInstance(20, definition, 0, false);
            frames.BeforeTick(simulation); simulation.Tick(default, default); frames.AfterTick(simulation);
            var shot = simulation.Projectiles[0]; Assert.That(frames.ProjectilePosition(shot, .25), Is.EqualTo(shot.Position));
            var previous = shot.Position; frames.BeforeTick(simulation); simulation.Tick(default, default); frames.AfterTick(simulation);
            var rendered = frames.ProjectilePosition(shot, .25);
            Assert.That(rendered.X, Is.EqualTo(previous.X + (shot.Position.X - previous.X) * .25f).Within(.00001f));
            Assert.That(frames.ProjectilePosition(shot, .25, true), Is.EqualTo(shot.Position));
        }
        [Test]
        public void ReplacingProjectileAtSameListIndexDoesNotBlendDifferentShots()
        {
            var simulation = BattleTestFactory.Create(); var frames = new RenderSnapshots(); frames.Reset(simulation);
            var definition = simulation.P1.Definition.Specials[0]; definition.StartupFrames = 0; definition.ActiveFrames = 4; definition.ProjectileLifetimeFrames = 1;
            simulation.P1.Attack = new AttackInstance(30, definition, 0, false);
            frames.BeforeTick(simulation); simulation.Tick(default, default); frames.AfterTick(simulation);
            definition.Hitbox = new BattleRect(new BattleVector(4, 0), definition.Hitbox.Size);
            simulation.P1.Attack = new AttackInstance(31, definition, 0, false);
            frames.BeforeTick(simulation); simulation.Tick(default, default); frames.AfterTick(simulation);
            Assert.That(simulation.Projectiles.Count, Is.EqualTo(1)); Assert.That(simulation.Projectiles[0].Attack.Id, Is.EqualTo(31));
            Assert.That(frames.ProjectilePosition(simulation.Projectiles[0], .1), Is.EqualTo(simulation.Projectiles[0].Position));
        }
        [TestCase(1.5f)]
        [TestCase(2.6f)]
        public void FormationMembersKeepTheirOwnPositionsAndSpacingDuringInterpolationAndReset(float spacing)
        {
            var simulation=BattleTestFactory.Create();var frames=new RenderSnapshots();frames.Reset(simulation);
            var definition=simulation.P1.Definition.Specials[0];definition.StartupFrames=0;definition.ActiveFrames=4;definition.ProjectileLifetimeFrames=20;
            definition.Ability.ProjectileCount=3;definition.Ability.ProjectileSpacing=spacing;
            simulation.P1.Attack=new AttackInstance(100,definition,0,false);
            frames.BeforeTick(simulation);simulation.Tick(default,default);frames.AfterTick(simulation);
            Assert.That(simulation.Projectiles.Count,Is.EqualTo(3));var shots=simulation.Projectiles;
            Assert.That(shots[0].Attack,Is.SameAs(shots[1].Attack));Assert.That(shots[1].Attack,Is.SameAs(shots[2].Attack));
            foreach(var shot in shots)Assert.That(frames.ProjectilePosition(shot,.5),Is.EqualTo(shot.Position),"New members snap to their individual positions");
            var old=new[]{shots[0].Position,shots[1].Position,shots[2].Position};
            frames.BeforeTick(simulation);simulation.Tick(default,default);frames.AfterTick(simulation);
            foreach(double alpha in new[]{0d,.25d,.5d,1d})for(int i=0;i<3;i++)
            {
                var drawn=frames.ProjectilePosition(shots[i],alpha);var now=shots[i].Position;
                Assert.That(drawn.X,Is.EqualTo(old[i].X+(now.X-old[i].X)*alpha).Within(.00001f));
                Assert.That(drawn.Y,Is.EqualTo(old[i].Y+(now.Y-old[i].Y)*alpha).Within(.00001f));
                if(i>0)Assert.That(drawn.X-frames.ProjectilePosition(shots[i-1],alpha).X,Is.EqualTo(spacing).Within(.00001f));
                Assert.That(frames.ProjectilePosition(shots[i],alpha,true),Is.EqualTo(now));
            }
            frames.Reset(simulation);foreach(var shot in shots)Assert.That(frames.ProjectilePosition(shot,0),Is.EqualTo(shot.Position));
            var rematch=BattleTestFactory.Create();frames.Reset(rematch);Assert.That(rematch.Projectiles.Count,Is.EqualTo(0));
        }
    }
}
