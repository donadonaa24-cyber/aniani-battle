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
        public IEnumerator CurryResidueAppearsAfterSplashWithNativeArtworkAndExpiresFiveSecondsLater()
        {
            var keys=controller.Bindings.P1;var projectiles=Object.FindAnyObjectByType<ProjectileView>();
            foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(runner.Setup.Character(2),runner.Setup.Character(8),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Facing",facing);Set(sim.P1,"Position",new BattleVector(0,sim.P1.Position.Y));
                Set(sim.P2,"Position",new BattleVector(-facing*14,sim.P2.Position.Y));Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);Set(sim.P1,"NormalHitCount",2);
                yield return Key(keys.Ultimate,true);Tick();yield return Key(keys.Ultimate,false);
                int wait=0;Projectile residue=null;
                while(residue==null&&wait++<160){Tick();residue=sim.Projectiles.FirstOrDefault(p=>p.Attack.Definition.Ability.Look==AbilityLook.CurryResidue);}
                Assert.That(residue,Is.Not.Null);Assert.That(residue.Visible,Is.False);
                projectiles.SendMessage("LateUpdate");Assert.That(projectiles.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.enabled),Is.False);
                var source=sim.P1.Attack;wait=0;while(!residue.Visible&&wait++<150)Tick();Assert.That(residue.Visible,Is.True);
                Assert.That(source.IsActive,Is.False);Assert.That(residue.RemainingFrames,Is.EqualTo(300));Assert.That(sim.P1.NormalHitCount,Is.EqualTo(1));
                projectiles.SendMessage("LateUpdate");var renderer=projectiles.GetComponentsInChildren<SpriteRenderer>().Single(s=>s.enabled);
                var art=runner.Setup.Character(2).PaperMotion.Projectile(AbilityLook.CurryResidue);
                Assert.That(renderer.sprite,Is.SameAs(art.Sprite));Assert.That(renderer.sprite.texture.width,Is.EqualTo(2172));Assert.That(renderer.sprite.texture.height,Is.EqualTo(724));
                Assert.That(renderer.transform.localScale.x,Is.EqualTo(art.ScaleFor(residue.Hitbox.Size)).Within(.0001f));Assert.That(renderer.sortingOrder,Is.EqualTo(3));
                Capture("curry_residue_"+facing,false,"special_refinement");
                Set(sim.P2,"Position",new BattleVector(residue.Position.X,sim.P2.Definition.HurtboxSize.Y*.5f));Set(sim.P2,"Velocity",default(BattleVector));Tick();
                Assert.That(sim.P2.NormalHitCount,Is.EqualTo(1));Assert.That(sim.P2.Hearts,Is.EqualTo(3));
                Set(sim.P2,"Position",new BattleVector(-facing*14,sim.P2.Definition.HurtboxSize.Y*.5f));Set(sim.P2,"Velocity",default(BattleVector));
                for(int tick=0;tick<299;tick++){Tick();if(tick==32)Capture("curry_settled_"+facing,false,"special_refinement");}Assert.That(sim.Projectiles.Contains(residue),Is.False);
                projectiles.SendMessage("LateUpdate");Assert.That(projectiles.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.enabled),Is.False);
            }
        }
        [UnityTest]
        public IEnumerator AkatsukiRealInputCapsTwoCardsAndOpponentHitRemovesOnlyTheTouchedCard()
        {
            var keys=controller.Bindings.P1;
            foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(runner.Setup.Character(7),runner.Setup.Character(7),runner.CurrentStage,false);
                var sim=runner.Simulation;Set(sim.P1,"Facing",facing);Set(sim.P2,"Position",new BattleVector(-facing*14,sim.P2.Position.Y));
                for(int index=0;index<2;index++)
                {
                    Set(sim.P1,"Position",new BattleVector(facing*(index==0?-4:4),sim.P1.Definition.HurtboxSize.Y*.5f));
                    yield return Key(keys.Down,true);yield return Key(keys.Special,true);Tick();yield return Key(keys.Special,false);yield return Key(keys.Down,false);
                    for(int tick=0;tick<125;tick++)Tick();
                }
                Assert.That(sim.P1.PlacedCount,Is.EqualTo(2));var traps=sim.Projectiles.ToArray();
                yield return Key(keys.Down,true);yield return Key(keys.Special,true);Tick();yield return Key(keys.Special,false);yield return Key(keys.Down,false);
                Assert.That(sim.P1.Attack,Is.Null);Assert.That(sim.Projectiles.Count,Is.EqualTo(2));
                Set(sim.P1,"Position",new BattleVector(traps[0].Position.X,sim.P1.Definition.HurtboxSize.Y*.5f));Tick();
                Assert.That(sim.P1.NormalHitCount,Is.Zero);Assert.That(sim.Projectiles.Count,Is.EqualTo(2));
                Set(sim.P2,"Position",new BattleVector(traps[0].Position.X,sim.P2.Definition.HurtboxSize.Y*.5f));Tick();
                Assert.That(sim.P2.NormalHitCount,Is.EqualTo(1));Assert.That(sim.P2.Hearts,Is.EqualTo(3));
                Assert.That(sim.Projectiles.Contains(traps[0]),Is.False);Assert.That(sim.Projectiles.Single(),Is.SameAs(traps[1]));Assert.That(sim.P1.PlacedCount,Is.EqualTo(1));
                Capture("akatsuki_card_consumed_"+facing,false,"special_refinement");
            }
        }
        [UnityTest]
        public IEnumerator RyutaRealChantFinishesBeforeTwentySecondBoostAndRunningUsesTheMultiplier()
        {
            var keys=controller.Bindings.P1;
            runner.BeginMatch(runner.Setup.Character(8),runner.Setup.Character(2),runner.CurrentStage,false);var sim=runner.Simulation;
            Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);Set(sim.P2,"Position",new BattleVector(14,sim.P2.Position.Y));
            yield return Key(keys.Ultimate,true);Tick();yield return Key(keys.Ultimate,false);var attack=sim.P1.Attack;int wait=0;
            while(sim.P1.Attack==attack&&wait++<240){Assert.That(sim.P1.MoveMultiplier,Is.EqualTo(1));Tick();}
            Assert.That(sim.P1.Attack,Is.Null);Assert.That(sim.P1.BuffRemaining,Is.EqualTo(1200));Assert.That(sim.P1.MoveMultiplier,Is.EqualTo(1.5f));
            float start=sim.P1.Position.X;yield return Key(keys.Right,true);for(int tick=0;tick<30;tick++)Tick();yield return Key(keys.Right,false);
            Assert.That(sim.P1.Position.X-start,Is.EqualTo(sim.P1.Definition.MoveSpeed*1.5f*.5f).Within(.001f));
            Assert.That(sim.P1.BuffRemaining,Is.EqualTo(1170));Capture("ryuta_spell_speed",false,"special_refinement");
        }
    }
}
