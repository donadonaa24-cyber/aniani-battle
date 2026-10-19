using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator MinatoUltimatePullsAnOpponentWalkingAwayIntoTheFieldAndLosesTwoHeartsOnce()
        {
            var keys = controller.Bindings.P1; var opponent = controller.Bindings.P2;
            foreach (int facing in new[] { 1, -1 })
            {
                runner.BeginMatch(runner.Setup.Character(3), runner.Setup.Character(2), runner.CurrentStage, false);
                var sim = runner.Simulation; Set(sim.P1, "Facing", facing); Set(sim.P1, "Gauge", sim.Rules.Gauge.Maximum);
                Set(sim.P1, "Position", new BattleVector(-14 * facing, sim.P1.Position.Y));
                Set(sim.P2, "Position", new BattleVector(14 * facing, sim.P2.Position.Y));
                yield return Key(keys.Ultimate, true); Tick(); yield return Key(keys.Ultimate, false);
                int wait = 0; while (sim.Projectiles.Count == 0 && wait++ < 120) Tick();
                Assert.That(wait, Is.LessThan(120)); var field = sim.Projectiles.Single(); var center = field.Position;
                Set(sim.P2, "Position", new BattleVector(8 * facing, sim.P2.Position.Y)); Set(sim.P2, "NormalHitCount", 2);
                var away = facing > 0 ? opponent.Right : opponent.Left; yield return Key(away, true);
                var projectileView = Object.FindAnyObjectByType<ProjectileView>(); projectileView.SendMessage("LateUpdate");
                yield return null;
                Capture("pull_before_" + facing, false, "minato_attraction");
                float previous = Mathf.Abs(sim.P2.Position.X); int elapsed = 0, hits = 0;
                while (sim.P2.Hearts == 3 && elapsed++ < 90)
                {
                    Tick(); projectileView.SendMessage("LateUpdate");
                    hits += sim.Events.Count(e => e.Kind == BattleEventKind.Hit && e.SourcePlayer == 0);
                    Assert.That(field.Position, Is.EqualTo(center)); Assert.That(sim.Match.IsFinished, Is.False);
                    if (sim.P2.Hearts == 3) { Assert.That(Mathf.Abs(sim.P2.Position.X), Is.LessThan(previous)); previous = Mathf.Abs(sim.P2.Position.X); }
                    if (elapsed == 6) { yield return null; Capture("pull_moving_" + facing, false, "minato_attraction"); }
                }
                Assert.That(elapsed, Is.InRange(2, 60)); Assert.That(hits, Is.EqualTo(1));
                Assert.That(sim.P2.Hearts, Is.EqualTo(1)); Assert.That(sim.P2.NormalHitCount, Is.EqualTo(2)); Assert.That(sim.P1.Hearts, Is.EqualTo(3));
                Assert.That(ProjectileAttractionSystem.Velocity(sim.P2, sim.Projectiles), Is.EqualTo(default(BattleVector)));
                yield return null; Capture("pull_contact_two_hearts_" + facing, false, "minato_attraction"); yield return Key(away, false);
                for (int i = 0; i < 90; i++) Tick(); Assert.That(sim.P2.Hearts, Is.EqualTo(1));
            }
        }
    }
}
