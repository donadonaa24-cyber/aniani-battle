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
        public IEnumerator AllNineSpecialsAndSupersUseTheirOwnDefinitionsAndCapturePendingArtClearly()
        {
            var keys = controller.Bindings.P1;
            for (int character = 0; character < runner.Setup.Characters.Length; character++)
            {
                var data = runner.Setup.Character(character);
                Assert.That(data.MoveSetSchemaVersion, Is.GreaterThanOrEqualTo(1)); Assert.That(data.PrototypeSharedMoves, Is.False);
                foreach (bool ultimate in new[] { false, true })
                {
                    runner.BeginMatch(data,runner.Setup.Character(0),runner.CurrentStage,false); view.RenderFrame();
                    var sim = runner.Simulation;
                    Set(sim.P2,"Position",new BattleVector(sim.P1.Position.X+1.5f,sim.P1.Position.Y));
                    Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);
                    var button = ultimate ? keys.Ultimate : keys.Special;
                    yield return Key(button,true); Tick();
                    Assert.That(sim.P1.Attack,Is.Not.Null);
                    var attack = sim.P1.Attack; var expected = ultimate ? data.Ultimate.Definition : data.Specials[0].Definition;
                    Assert.That(attack.Definition.Name,Is.EqualTo(expected.Name));
                    Assert.That(attack.Definition.Ability.Look,Is.EqualTo(expected.Ability.Look));
                    Capture(data.name+(ultimate?"_super_prepare":"_special_prepare"),true,"rework_r3");
                    yield return Key(button,false);
                    int limit=expected.StartupFrames+data.Movement.UltimateCutInFrames+40;
                    int waited=0;
                    while(sim.P1.Attack==attack&&!attack.IsDamageActive&&waited++<limit)Tick();
                    Assert.That(attack.IsDamageActive,Is.True,data.name);
                    var shots=Object.FindAnyObjectByType<ProjectileView>();shots.SendMessage("LateUpdate");view.RenderFrame();
                    if(expected.SpawnProjectile)Assert.That(sim.Projectiles.Any(p=>p.Owner==sim.P1&&p.Attack==attack),Is.True);
                    Capture(data.name+(ultimate?"_super_active":"_special_active"),false,"rework_r3");
                }
            }
        }
        [UnityTest]
        public IEnumerator ToumaNormalGunActuallyFiresWhileRiceBallKeepsTheSamePaperSprite()
        {
            var keys = controller.Bindings.P1;var toma = runner.Setup.Character(0);
            runner.BeginMatch(toma,runner.Setup.Character(2),runner.CurrentStage,false);
            yield return Key(keys.Attack,true);Tick();yield return Key(keys.Attack,false);Tick();
            for(int tick=0;tick<toma.GroundNormals[0].Definition.StartupFrames;tick++)Tick();
            Assert.That(runner.Simulation.Projectiles.Count,Is.EqualTo(1));
            Assert.That(runner.Simulation.Projectiles[0].Attack.Definition.Ability.Look,Is.EqualTo(AbilityLook.Bullet));
            Object.FindAnyObjectByType<ProjectileView>().SendMessage("LateUpdate");Capture("Touma_bullet_placeholder",false,"rework_r3");
            var chizuru=runner.Setup.Character(2);runner.BeginMatch(chizuru,toma,runner.CurrentStage,false);
            yield return Key(keys.Special,true);Tick();yield return Key(keys.Special,false);
            for(int tick=0;tick<chizuru.Specials[0].Definition.StartupFrames;tick++)Tick();
            Object.FindAnyObjectByType<ProjectileView>().SendMessage("LateUpdate");
            var sprite=Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Single(r=>r.name=="Projectile"&&r.enabled);
            Assert.That(sprite.sprite,Is.SameAs(chizuru.PaperMotion.RiceBall));
        }
    }
}
