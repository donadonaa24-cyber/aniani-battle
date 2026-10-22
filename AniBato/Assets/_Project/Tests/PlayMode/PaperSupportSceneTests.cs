using System.Collections;
using System.Linq;
using System.Reflection;
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
        public IEnumerator EverySupportIsCollectedWithMovementKeysAndShowsItsGeneratedActorAndEffect()
        {
            var asset=(BattleRules)typeof(BattleRunner).GetField("rules",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(runner);
            var rules=asset.ToDefinition();rules.Support.FirstMinimumFrames=rules.Support.FirstMaximumFrames=1;
            foreach(SupportKind kind in System.Enum.GetValues(typeof(SupportKind)))
            {
                uint seed=1;
                for(;seed<500;seed++)
                {
                    var candidate=new BattleSimulation(runner.Setup.Character(2).ToDefinition(),runner.Setup.Character(1).ToDefinition(),rules,runner.Setup.Stages[0].ToGeometry(),seed);
                    candidate.Tick(default,default);if(candidate.Supports.Pickup.Kind==kind)break;
                }
                Assert.That(seed,Is.LessThan(500));
                runner.BeginMatch(runner.Setup.Character(2),runner.Setup.Character(1),runner.Setup.Stages[0],false);
                var sim=new BattleSimulation(runner.Character(0).ToDefinition(),runner.Character(1).ToDefinition(),rules,runner.CurrentStage.ToGeometry(),seed);
                typeof(BattleRunner).GetProperty("Simulation").SetValue(runner,sim);
                runner.RenderState.Reset(sim);Tick();var pickup=sim.Supports.Pickup;Assert.That(pickup,Is.Not.Null);
                bool reverse=kind==SupportKind.Mai;
                var f=sim.P1;Set(f,"Position",new BattleVector(pickup.Position.X+(reverse?1.1f:-1.1f),sim.Stage[0].Bounds.Top+f.Definition.HurtboxSize.Y*.5f));
                Set(f,"StaminaUnits",20*60);Set(f,"Hearts",2);Set(f,"Grounded",true);Set(f,"GroundPlatform",0);
                var approach=reverse?controller.Bindings.P1.Left:controller.Bindings.P1.Right;
                yield return Key(approach,true);
                for(int i=0;i<20&&sim.Supports.Pickup!=null;i++)Tick();
                yield return Key(approach,false);
                Assert.That(sim.Supports.Pickup,Is.Null,kind.ToString());Assert.That(sim.Supports.Actors.Single().Kind,Is.EqualTo(kind));
                for(int i=0;i<rules.Support.PortalFrames+8;i++)Tick();
                var support=Object.FindAnyObjectByType<SupportView>();support.SendMessage("LateUpdate");yield return null;
                var actor=support.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.enabled&&s.name=="Support Character").ToArray();
                if(kind!=SupportKind.ShionCar)
                {
                    Assert.That(actor.Length,Is.EqualTo(1));
                    var expected=runner.Setup.SupportArt.Find(kind==SupportKind.Hota?"pickup_pawn":PaperSupportArt.ActorKey(kind));
                    Assert.That(actor[0].sprite,Is.SameAs(expected.Sprite));
                    if(reverse)
                    {
                        Assert.That(f.Facing,Is.EqualTo(-1));
                        Assert.That(actor[0].flipX,Is.EqualTo(sim.P2.Position.X<sim.Supports.Actors.Single().Position.X));
                        Assert.That(sim.Projectiles.Single().Facing,Is.EqualTo(sim.Supports.Actors.Single().AttackFacing));
                    }
                }
                else Assert.That(sim.Projectiles.Single().Attack.Definition.Ability.Look,Is.EqualTo(AbilityLook.SupportCar));
                var hud=Object.FindAnyObjectByType<HUDController>();hud.SendMessage("LateUpdate");
                Assert.That(hud.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.text.Contains("サポート")&&t.text.Contains("秒")),Is.True);
                var notice=Object.FindAnyObjectByType<SupportNoticeHud>();
                var description=notice.GetComponentsInChildren<UnityEngine.UI.Text>().Single(t=>t.name=="Support Effect Text");
                string[] effectClues={"60％","2秒","5秒","1個","1個","財布","超必殺","15"};
                Assert.That(description.text,Does.StartWith("P1 千鶴｜").And.Contain(effectClues[(int)kind]),"取得直後に誰へのどの効果か表示");
                Assert.That(description.preferredHeight,Is.LessThanOrEqualTo(description.rectTransform.rect.height));
                if(kind==SupportKind.Alma)Assert.That(sim.P2.SupportSlowRemaining,Is.GreaterThan(0));
                if(kind==SupportKind.SakiHana)Assert.That(f.SupportInfiniteRemaining,Is.GreaterThan(0));
                if(kind==SupportKind.Sara)Assert.That(f.Hearts,Is.EqualTo(3));
                if(kind==SupportKind.Mio)Assert.That(f.SupportBarrierUnits,Is.EqualTo(3));
                if(kind==SupportKind.Tsuyoshi)Assert.That(sim.Supports.Wallet,Is.Not.Null);
                var camera=Camera.main;var old=camera.transform.position;float size=camera.orthographicSize;
                float center=kind==SupportKind.ShionCar?sim.Projectiles.Single().Position.X:(sim.Supports.Actors.Single().Position.X+f.Position.X)*.5f;
                camera.transform.position=new Vector3(center,sim.Stage[0].Bounds.Top+(kind==SupportKind.Hota?4:1.8f),-10);camera.orthographicSize=kind==SupportKind.Hota?6.4f:3.6f;
                Capture(kind.ToString(),false,"paper_support");camera.transform.position=old;camera.orthographicSize=size;
                if(kind==SupportKind.Tsuyoshi)
                {
                    var wallet=sim.Supports.Wallet;Set(f,"Position",new BattleVector(wallet.Position.X-.9f,f.Position.Y));
                    Set(f,"StaminaUnits",10*60);
                    yield return Key(controller.Bindings.P1.Right,true);
                    for(int j=0;j<20&&sim.Supports.Wallet!=null;j++)Tick();
                    yield return Key(controller.Bindings.P1.Right,false);
                    Assert.That(sim.Supports.Wallet,Is.Null);
                    var walletText=notice.GetComponentsInChildren<UnityEngine.UI.Text>().Single(t=>t.name=="Support Effect Text");
                    Assert.That(walletText.text,Does.Contain("財布").And.Contain("スタミナを全回復"));
                    Assert.That(f.Stamina,Is.EqualTo(f.Definition.Stamina.Maximum));
                }
            }
        }
        [UnityTest]
        public IEnumerator NewMatchClearsEverySupportStatusAndPooledIllustration()
        {
            // Runtime restart must clear every status and the pooled view's old drawings.
            Set(runner.Simulation.P1,"SupportInfiniteRemaining",300);Set(runner.Simulation.P2,"SupportSlowRemaining",300);
            Set(runner.Simulation.P1,"SupportBarrierRemaining",300);Set(runner.Simulation.P1,"SupportBarrierUnits",3);
            runner.RestartMatch();var support=Object.FindAnyObjectByType<SupportView>();support.SendMessage("LateUpdate");yield return null;
            Assert.That(runner.Simulation.P1.SupportInfiniteRemaining,Is.Zero);Assert.That(runner.Simulation.P2.SupportSlowRemaining,Is.Zero);
            Assert.That(support.GetComponentsInChildren<SpriteRenderer>().All(s=>!s.enabled),Is.True);
            Assert.That(Object.FindAnyObjectByType<SupportNoticeHud>().GetComponentsInChildren<UnityEngine.UI.Text>().Count(t=>t.name=="Support Effect Text"),Is.Zero,"前ラウンドの説明は残さない");
            Assert.That(Object.FindAnyObjectByType<SupportFieldFilter>(),Is.Not.Null);
        }
    }
}
