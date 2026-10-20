using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        private static void AssertCueFitsChargedHitbox(AttackInstance attack,FighterState fighter,PaperAbilityArt art,SpriteRenderer effect)
        {
            var hitbox=attack.WorldHitbox(fighter);
            Assert.That(effect.transform.position,Is.EqualTo(new Vector3(hitbox.Center.X,hitbox.Center.Y,0)));
            Assert.That(effect.transform.localScale.x*art.VisibleSize.x,Is.EqualTo(hitbox.Size.X).Within(.0001f));
            Assert.That(effect.transform.localScale.y*art.VisibleSize.y,Is.EqualTo(hitbox.Size.Y).Within(.0001f));
        }
        [UnityTest]
        public IEnumerator RyutaThirdNormalUsesTheTwoKickKeysWithoutAttackOverlayAndShowsStaminaCost()
        {
            var keys=controller.Bindings.P1;
            foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(runner.Setup.Character(8),runner.Setup.Character(0),runner.CurrentStage,false);
                var f=runner.Simulation.P1;Set(f,"Facing",facing);int initial=f.StaminaUnits;
                yield return Key(keys.Attack,true);Tick();yield return Key(keys.Attack,false);Tick();
                int wait=0;
                while(f.Attack==null||f.Attack.ChainIndex!=2)
                {
                    Assert.That(wait++,Is.LessThan(180));
                    if(f.Attack!=null&&!f.Attack.NextQueued)
                    {yield return Key(keys.Attack,true);Tick();yield return Key(keys.Attack,false);}
                    Tick();
                }
                view.RenderFrame();var pose=runner.Character(0).PaperMotion.Find(AniBato.Presentation.AnimationState.Attack3);
                Assert.That(body.sprite,Is.SameAs(pose.Frames[0]));
                Assert.That(body.sprite.name,Does.Contain("normal3_prepare"));
                Assert.That(view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>().enabled,Is.False);
                Capture("ryuta_kick_prepare_"+facing,true,"readable_skills");
                while(!f.Attack.IsActive)Tick();view.RenderFrame();
                Assert.That(body.sprite,Is.SameAs(pose.Frames[1]));Assert.That(body.sprite.name,Does.Contain("normal3_impact"));
                Assert.That(view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>().enabled,Is.False);
                Assert.That(f.StaminaUnits,Is.EqualTo(initial-13*60));
                Object.FindAnyObjectByType<HUDController>().SendMessage("LateUpdate");
                Assert.That(Object.FindObjectsByType<Text>().Any(t=>t.text.Contains("通常 3/4/6")),Is.True);
                Capture("ryuta_kick_impact_"+facing,true,"readable_skills");
            }
        }
        [UnityTest]
        public IEnumerator BothPlacingCharactersUseTheirCrouchIllustrationAndStayStillUntilThePlacementFinishes()
        {
            var keys=controller.Bindings.P1;
            foreach(int index in new[]{3,7})foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(runner.Setup.Character(index),runner.Setup.Character(2),runner.CurrentStage,false);
                var f=runner.Simulation.P1;Set(f,"Facing",facing);var origin=f.Position;
                yield return Key(keys.Down,true);yield return Key(keys.Special,true);Tick();yield return Key(keys.Special,false);yield return Key(keys.Down,false);
                yield return Key(keys.Right,true);
                for(int i=0;i<59;i++)Tick();view.RenderFrame();
                Assert.That(f.Position,Is.EqualTo(origin));Assert.That(runner.Simulation.Projectiles,Is.Empty);
                Assert.That(body.sprite,Is.SameAs(runner.Character(0).PaperMotion.Find(AniBato.Presentation.AnimationState.Crouch).Frames[0]));
                Capture("placing_crouch_"+index+"_"+facing,false,"readable_skills");
                Tick();Assert.That(runner.Simulation.Projectiles.Count,Is.EqualTo(1));
                for(int i=0;i<18;i++){Tick();Assert.That(f.Position,Is.EqualTo(origin));}
                Tick();Assert.That(f.Position.X,Is.GreaterThan(origin.X));yield return Key(keys.Right,false);
            }
        }
        [UnityTest]
        public IEnumerator ChargedTransferShowsBlueFragmentsAndReassemblesWithWhiteVerticalFlashThroughRealKeys()
        {
            var keys=controller.Bindings.P1;
            foreach(int index in new[]{3,4})foreach(int facing in new[]{-1,1})foreach(int duration in new[]{30,60})
            {
                runner.BeginMatch(runner.Setup.Character(index),runner.Setup.Character(2),runner.CurrentStage,false);
                var f=runner.Simulation.P1; Set(f,"Position",new BattleVector(0,7));Set(f,"Grounded",false);Set(f,"GroundPlatform",-1);Set(f,"Facing",facing);
                yield return Key(keys.Up,true);yield return Key(keys.Special,true);Tick();yield return Key(keys.Up,false);
                yield return Key(facing<0?keys.Left:keys.Right,true);var start=f.Position;
                for(int tick=0;tick<25;tick++)Tick();
                Assert.That(f.Position,Is.EqualTo(start));Assert.That(body.color.b,Is.GreaterThan(.9f));
                Assert.That(view.GetComponentsInChildren<SpriteRenderer>().Count(s=>s.enabled&&s.name.StartsWith("Teleport Fragment ")),Is.EqualTo(24));
                Capture("teleport_split_"+index+"_"+facing+"_"+duration,false,"readable_skills");
                if(duration==30)yield return Key(keys.Special,false);
                for(int tick=25;tick<duration;tick++)Tick();
                Assert.That(f.Attack.RecoveryStarted,Is.True);Assert.That((f.Position.X-start.X)*facing,Is.EqualTo(duration==30?7.5f:12f).Within(.001));
                Assert.That(view.GetComponentsInChildren<SpriteRenderer>().Count(s=>s.enabled&&s.name.StartsWith("Teleport White Flash ")),Is.EqualTo(3));
                Capture("teleport_arrival_"+index+"_"+facing+"_"+duration,false,"readable_skills");
                yield return Key(keys.Special,false);yield return Key(facing<0?keys.Left:keys.Right,false);
                for(int tick=0;tick<18;tick++)Tick();
                Assert.That(body.color.a,Is.GreaterThan(.9f));
                Assert.That(view.GetComponentsInChildren<SpriteRenderer>().Any(s=>s.enabled&&s.name.StartsWith("Teleport Fragment ")),Is.False);
            }
        }
        [UnityTest]
        public IEnumerator BothPlayersHaveReadableIndependentCooldownCardsAndAlmaArtworkMatchesMaximumHitbox()
        {
            var keys=controller.Bindings.P1;
            runner.BeginMatch(runner.Setup.Character(4),runner.Setup.Character(7),runner.CurrentStage,false);
            var sim=runner.Simulation;Set(sim.P1,"Gauge",sim.Rules.Gauge.Maximum);Set(sim.P2,"Position",new BattleVector(16,1.1f));
            yield return Key(keys.Ultimate,true);Tick();var attack=sim.P1.Attack;
            int wait=0;while(!attack.AbilityChargeReleased&&wait++<200)Tick();
            Assert.That(attack.AbilityChargeFrames,Is.EqualTo(120));Assert.That(attack.IsActive,Is.True);
            Assert.That(attack.Definition.Hitbox.Size.X,Is.EqualTo(sim.Stage[0].Bounds.Size.X*2f/3).Within(.001));
            view.RenderFrame();var art=view.transform.Find("Paper Ability").GetComponent<SpriteRenderer>();
            var visible=runner.Character(0).PaperMotion.AbilityEffect(AniBato.Presentation.AnimationState.Ultimate).VisibleSize;
            Assert.That(art.enabled,Is.True);Assert.That(art.transform.localScale.x*visible.x,Is.EqualTo(attack.Definition.Hitbox.Size.X).Within(.001));
            foreach(var panel in Object.FindObjectsByType<SpecialCooldownHud>())panel.Refresh();
            var cards=Object.FindObjectsByType<Text>().Where(t=>t.name.Contains("Special Status")&&t.gameObject.activeInHierarchy).ToArray();
            Assert.That(cards.Length,Is.EqualTo(8));Assert.That(cards.Count(t=>t.text.StartsWith("↑ 復帰")),Is.EqualTo(2));
            Assert.That(cards.Count(t=>t.text.StartsWith("↓ 特殊")),Is.EqualTo(2));
            foreach(var card in cards)
            {
                var corners=new Vector3[4];card.rectTransform.GetWorldCorners(corners);
                Assert.That(corners[0].x,Is.GreaterThanOrEqualTo(0));Assert.That(corners[2].x,Is.LessThanOrEqualTo(Screen.width));
                Assert.That(corners[0].y,Is.GreaterThan(Screen.height*.6f));Assert.That(corners[2].y,Is.LessThanOrEqualTo(Screen.height));
                Assert.That(card.preferredHeight,Is.LessThanOrEqualTo(card.rectTransform.rect.height+1));
            }
            Capture("alma_full_charge_and_cooldowns",false,"readable_skills");yield return Key(keys.Ultimate,false);
        }
    }
}
