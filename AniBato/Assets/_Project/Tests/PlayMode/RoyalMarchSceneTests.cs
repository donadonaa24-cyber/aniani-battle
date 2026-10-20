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
        public IEnumerator KingUsesGeneratedRiftsForHisThreeMinionsAndCommandsThemBeforeTheyWalk()
        {
            var keys=controller.Bindings.P1;
            foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(runner.Setup.Character(6),runner.Setup.Character(2),runner.CurrentStage,false);
                var f=runner.Simulation.P1;Set(f,"Facing",facing);Set(f,"Position",new BattleVector(-facing*12,1.1f));
                Set(runner.Simulation.P2,"Position",new BattleVector(facing*15,1.1f));
                yield return Key(keys.Special,true);Tick();yield return Key(keys.Special,false);Tick();
                for(int i=0;i<30;i++)Tick();
                var projectileView=Object.FindAnyObjectByType<ProjectileView>();projectileView.SendMessage("LateUpdate");
                var rifts=projectileView.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.enabled&&s.name=="Royal Summon Rift").ToArray();
                Assert.That(rifts.Length,Is.EqualTo(3));Assert.That(rifts.All(r=>r.sprite==runner.Character(0).PaperMotion.SummonRift),Is.True);
                Capture("royal_rift_emergence_"+facing,false,"readable_skills");
                while(!runner.Simulation.Projectiles.Any(p=>p.MarchCommand))Tick();view.RenderFrame();
                var speech=view.transform.Find("Royal Command Words").GetComponent<TextMesh>();
                Assert.That(speech.gameObject.activeSelf,Is.True);Assert.That(speech.text,Is.EqualTo("突撃しろ！！"));
                Assert.That(runner.Simulation.Projectiles.All(p=>!p.CanDamage),Is.True);
                Capture("royal_command_"+facing,false,"readable_skills");
                for(int i=0;i<26;i++)Tick();projectileView.SendMessage("LateUpdate");
                var minions=projectileView.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.enabled&&s.name=="Projectile").ToArray();
                Assert.That(minions.Length,Is.EqualTo(3));Assert.That(runner.Simulation.Projectiles.All(p=>p.CanDamage),Is.True);
                var arts=runner.Character(0).PaperMotion.Projectile(AbilityLook.King);
                for(int i=0;i<3;i++)
                {
                    Assert.That(minions[i].sprite,Is.SameAs(arts.SpriteFor(i)));
                    Assert.That(minions[i].transform.localScale.y*arts.VariantVisibleSizes[i].y,
                        Is.EqualTo(runner.Character(0).PaperMotion.StandingHeight).Within(.001));
                }
                Capture("royal_march_"+facing,false,"readable_skills");
                int waited=0;while(!runner.Simulation.Projectiles.Any(p=>p.MarchExitAge>0)&&waited++<220)Tick();
                Assert.That(waited,Is.LessThan(220));projectileView.SendMessage("LateUpdate");
                Assert.That(projectileView.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.enabled&&s.name=="Royal Summon Rift"),Is.True);
                Capture("royal_rift_exit_"+facing,false,"readable_skills");
            }
        }
    }
}
