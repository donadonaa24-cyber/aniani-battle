using System.Collections;
using System.Linq;
using UnityEngine.InputSystem.Controls;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AniBato.Tests
{
    public sealed class BattleSceneTests : InputTestFixture
    {
        private Keyboard keyboard;
        private readonly System.Collections.Generic.Dictionary<string, string> controlPrefs = new System.Collections.Generic.Dictionary<string, string>();
        private InputSettings.BackgroundBehavior previousBackgroundBehavior;
        private InputSettings.UpdateMode previousUpdateMode;
        private bool previousRunInBackground;
        // The Unity Test Framework runs [UnitySetUp] BEFORE [SetUp] (and [UnityTearDown] after [TearDown]),
        // so the fixture's automatic Setup/TearDown would isolate input too late. Suppress them and run the
        // isolation explicitly at the start of UnitySetUp / end of UnityTearDown instead.
        [SetUp] public override void Setup() { }
        [TearDown] public override void TearDown() { }
        private bool inputIsolated;
        private void IsolateInput()
        {
            previousRunInBackground = Application.runInBackground;
            base.Setup(); // Isolate native devices/events and install the test runtime player-loop hook.
            inputIsolated = true;
            // Application.runInBackground does not configure InputTestFixture's replacement runtime.
            InputFixtureFocus.Configure(this);
            previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            previousUpdateMode = InputSystem.settings.updateMode;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            InputSystem.Update(); // Apply focus before adding the isolated keyboard.
        }
        private void RestoreInput()
        {
            if (!inputIsolated) return;
            InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
            InputSystem.settings.updateMode = previousUpdateMode;
            Application.runInBackground = previousRunInBackground;
            base.TearDown(); // Restores the original settings, devices, editor routing, and native runtime.
            inputIsolated = false;
        }
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            IsolateInput();
            for (int p = 0; p < 2; p++) foreach (string device in new[] { "pad", "keyboard" })
            { string key = "AniBato.Controls.v1." + p + "." + device; controlPrefs[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null; PlayerPrefs.DeleteKey(key); }
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("Battle");
            yield return null;
            keyboard.MakeCurrent();
            Object.FindAnyObjectByType<InputController>().AssignKeyboard(keyboard);
            Assert.That(keyboard.enabled, Is.True, "テストキーボードがフォーカス処理で無効化されています。");
        }
        [UnityTearDown]
        public IEnumerator CloseScene()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            if (runner) runner.enabled = false;
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            yield return null;
            foreach (var entry in controlPrefs) { if (entry.Value == null) PlayerPrefs.DeleteKey(entry.Key); else PlayerPrefs.SetString(entry.Key, entry.Value); }
            controlPrefs.Clear(); PlayerPrefs.Save();
            RestoreInput();
        }
        private static IEnumerator WaitFor(System.Func<bool> condition, string message)
        {
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, message);
        }
        private IEnumerator ChangeKeys(bool pressed, params ButtonControl[] buttons)
        {
            // UnityTest Press/Release queue delta events. Bit controls can share the byte copied
            // from current device state, so a second pending event can overwrite the earlier key.
            // Let the real dynamic PlayerLoop deliver each event before constructing the next.
            foreach (var button in buttons)
            {
                if (pressed) Press(button, queueEventOnly: true); else Release(button, queueEventOnly: true);
                yield return WaitFor(() => button.isPressed == pressed, button.path + " の入力イベントが反映されませんでした。");
            }
            // Callers also wait for Capture -> tick -> LateUpdate, not an assumed frame count.
            yield return null;
        }
        private IEnumerator PressKeys(params ButtonControl[] buttons) => ChangeKeys(true, buttons);
        private IEnumerator ReleaseKeys(params ButtonControl[] buttons) => ChangeKeys(false, buttons);
        private static bool IsIdleSprite(BattleRunner runner, FighterView view, SpriteRenderer body)
        {
            var data = runner.Character(view.PlayerIndex);
            if (data.PaperMotion) return view.UsesAnimationSprites && view.UsesPaperSprite &&
                view.RenderedSample.State == AniBato.Presentation.AnimationState.Idle && body.sprite == data.PaperMotion.Sprite;
            return view.UsesAnimationSprites && data.Animations.Find("idle").Frames.Contains(body.sprite);
        }
        private static bool IsWalkSprite(BattleRunner runner, FighterView view, SpriteRenderer body)
        {
            var data = runner.Character(view.PlayerIndex);
            if (data.PaperMotion) return view.UsesPaperSprite && view.RenderedSample.State == AniBato.Presentation.AnimationState.Move && body.sprite == data.PaperMotion.Sprite;
            return body.sprite.name.StartsWith("walk_");
        }
        [UnityTest]
        public IEnumerator ActualFighterViewMovesUniformlyWith144HzFrameTimeSequence()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>(); var input = Object.FindAnyObjectByType<InputController>();
            var view = Object.FindObjectsByType<FighterView>().Single(candidate => candidate.PlayerIndex == 0);
            yield return PressKeys(keyboard.dKey); runner.enabled = false; runner.RestartMatch(); input.Capture();
            float last = runner.Simulation.P1.Position.X, min = float.MaxValue, max = 0; int rawStationary = 0; float rawLast = last;
            var log = new System.Text.StringBuilder("frame,seconds,renderX,deltaX,simulationX\n");
            for (int i = 0; i < 72; i++)
            {
                runner.AdvanceRenderFrame(1d / 144); view.RenderFrame(); float x = view.transform.position.x, delta = x - last;
                log.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0},{1:R},{2:R},{3:R},{4:R}\n", i, (i + 1d) / 144, x, delta, runner.Simulation.P1.Position.X);
                if (i > 4) { Assert.That(delta, Is.GreaterThan(0)); min = Mathf.Min(min, delta); max = Mathf.Max(max, delta); if (rawLast == runner.Simulation.P1.Position.X) rawStationary++; }
                last = x; rawLast = runner.Simulation.P1.Position.X;
            }
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../TestResults/phase5b-render-144hz-unity.csv"));
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)); System.IO.File.WriteAllText(path, log.ToString());
            Assert.That(max - min, Is.LessThan(.00002f)); Assert.That(rawStationary, Is.GreaterThan(30));
            runner.SetPaused(true); view.RenderFrame(); Assert.That(view.transform.position.x, Is.EqualTo(runner.Simulation.P1.Position.X));
            runner.RestartMatch(); view.RenderFrame(); Assert.That(view.transform.position.x, Is.EqualTo(runner.Simulation.P1.Position.X));
            yield return ReleaseKeys(keyboard.dKey);
        }
        [UnityTest]
        public IEnumerator GeneratedSceneRunsAndCreatesBothFightersAndHeadCounters()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            Assert.That(runner, Is.Not.Null); Assert.That(runner.Simulation, Is.Not.Null);
            int previous = runner.Simulation.Match.TickCount;
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(runner.Simulation.Match.TickCount, Is.GreaterThan(previous));
            Assert.That(Object.FindObjectsByType<FighterView>().Length, Is.EqualTo(2));
            Assert.That(Object.FindObjectsByType<SpriteRenderer>().Length, Is.GreaterThanOrEqualTo(12));
            var counters = Object.FindObjectsByType<Text>().Where(t => t.name.Contains("Head Counter")).ToArray();
            Assert.That(counters.Length, Is.EqualTo(2)); Assert.That(counters.All(t => t.text == "0/3"), Is.True);
        }
        [UnityTest]
        public IEnumerator NewActionsRenderForAllThreeFightersAndJumpShowsTwoPlantedTicks()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>(); var controller = Object.FindAnyObjectByType<InputController>();
            var view = Object.FindObjectsByType<FighterView>().Single(v => v.PlayerIndex == 0);
            var body = view.transform.Find("Body").GetComponent<SpriteRenderer>();
            runner.enabled = false; // Drive exactly one real runner tick per assertion, independent of render frame rate.
            runner.SetPaused(false);
            for (int c = 0; c < 3; c++)
            {
                var data = runner.Setup.Character(c);
                runner.BeginMatch(data, runner.Setup.Character((c + 1) % 3), runner.CurrentStage, false);
                yield return PressKeys(keyboard[controller.Bindings.P1.Jump]); controller.Capture();
                for (int tick = 0; tick < 2; tick++)
                {
                    runner.AdvanceRenderFrame(1d / 60); view.RenderFrame();
                    Assert.That(runner.Simulation.P1.Grounded, Is.True); Assert.That(runner.Simulation.P1.JumpsUsed, Is.Zero);
                    Assert.That(view.RenderedSample.State, Is.EqualTo(AniBato.Presentation.AnimationState.JumpStartup));
                    Assert.That(body.sprite, Is.SameAs(data.Animations.Find("land").Frames[0])); Assert.That(view.UsesAnimationSprites, Is.True);
                    if (tick == 0) { yield return ReleaseKeys(keyboard[controller.Bindings.P1.Jump]); controller.Capture(); }
                }
                runner.AdvanceRenderFrame(1d / 60); view.RenderFrame();
                Assert.That(runner.Simulation.P1.Grounded, Is.False); Assert.That(runner.Simulation.P1.JumpsUsed, Is.EqualTo(1));
                Assert.That(body.sprite, Is.SameAs(data.Animations.Find("jump").Frames[0]));
                Key[] keys = { controller.Bindings.P1.Guard, controller.Bindings.P1.Dodge, controller.Bindings.P1.Dash };
                string[] clips = { "guard", "dodge", "dash" };
                for (int action = 0; action < keys.Length; action++)
                {
                    runner.RestartMatch(); yield return PressKeys(keyboard[keys[action]]); controller.Capture();
                    runner.AdvanceRenderFrame(1d / 60); view.RenderFrame();
                    Assert.That(body.sprite, Is.SameAs(data.Animations.Find(clips[action]).Frames[0]), data.name + "/" + clips[action]);
                    Assert.That(body.transform.localScale, Is.EqualTo(Vector3.one)); Assert.That(view.UsesAnimationSprites, Is.True);
                    yield return ReleaseKeys(keyboard[keys[action]]); controller.Capture();
                }
            }
            runner.enabled = true;
            yield return null;
        }
        [UnityTest]
        public IEnumerator WalkAndThreeGroundNormalsRenderForEveryFighterAtActualInputPhases()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>(); var controller = Object.FindAnyObjectByType<InputController>();
            var view = Object.FindObjectsByType<FighterView>().Single(v => v.PlayerIndex == 0);
            var body = view.transform.Find("Body").GetComponent<SpriteRenderer>();
            runner.enabled = false; runner.SetPaused(false);
            for (int c = 0; c < 3; c++)
            {
                var data = runner.Setup.Character(c);
                runner.BeginMatch(data, runner.Setup.Character((c + 1) % 3), runner.CurrentStage, false);
                yield return PressKeys(keyboard.dKey); controller.Capture();
                float start = runner.Simulation.P1.Position.X;
                for (int tick = 0; tick < 32; tick++)
                {
                    runner.AdvanceRenderFrame(1d / 60); view.RenderFrame();
                    Assert.That(view.RenderedSample.State, Is.EqualTo(AniBato.Presentation.AnimationState.Move));
                    Assert.That(body.sprite, Is.SameAs(data.PaperMotion ? data.PaperMotion.Sprite : data.Animations.Find("walk").Frames[tick / 4]));
                    Assert.That(view.UsesPaperSprite, Is.EqualTo(c == 2));
                    Assert.That(body.transform.localScale, Is.EqualTo(Vector3.one)); Assert.That(view.UsesAnimationSprites, Is.True);
                }
                Assert.That(runner.Simulation.P1.Position.X, Is.GreaterThan(start));
                yield return ReleaseKeys(keyboard.dKey); controller.Capture(); runner.RestartMatch();
                  yield return PressKeys(keyboard.dKey, keyboard[controller.Bindings.P1.Dash]); controller.Capture();
                  for (int tick = 0; tick < 33; tick++)
                  {
                      runner.AdvanceRenderFrame(1d / 60); view.RenderFrame();
                      Assert.That(view.RenderedSample.State, Is.EqualTo(AniBato.Presentation.AnimationState.Dash));
                      Assert.That(body.sprite, Is.SameAs(data.Animations.Find("dash").Frames[(tick / 4) % 8]));
                      Assert.That(runner.Simulation.P1.Grounded, Is.True, "Drawn flight must not change ground dash physics.");
                      Assert.That(body.transform.localScale, Is.EqualTo(Vector3.one));
                  }
                  yield return ReleaseKeys(keyboard.dKey, keyboard[controller.Bindings.P1.Dash]); controller.Capture(); runner.RestartMatch();
                yield return PressKeys(keyboard[controller.Bindings.P1.Attack]); controller.Capture();
                runner.AdvanceRenderFrame(1d / 60); view.RenderFrame();
                yield return ReleaseKeys(keyboard[controller.Bindings.P1.Attack]); controller.Capture();
                int[] activeTicks = new int[3]; bool[] queued = new bool[3]; int ticks = 0;
                while (runner.Simulation.P1.Attack != null && ticks++ < 200)
                {
                    var attack = runner.Simulation.P1.Attack; int chain = attack.ChainIndex;
                    Assert.That(view.RenderedSample.State, Is.EqualTo((AniBato.Presentation.AnimationState)((int)AniBato.Presentation.AnimationState.Attack1 + chain)));
                    var clip = data.Animations.Find("attack" + (chain + 1));
                    if (attack.IsActive) { activeTicks[chain]++; Assert.That(body.sprite, Is.SameAs(clip.Frames[2])); }
                    else if (attack.Frame < attack.Definition.StartupFrames) Assert.That(body.sprite, Is.SameAs(clip.Frames[0]).Or.SameAs(clip.Frames[1]));
                    else Assert.That(System.Array.IndexOf(clip.Frames, body.sprite), Is.InRange(3, 5));
                    Assert.That(body.transform.localScale, Is.EqualTo(Vector3.one));
                    bool press = chain < 2 && !queued[chain];
                    if (press) { queued[chain] = true; yield return PressKeys(keyboard[controller.Bindings.P1.Attack]); controller.Capture(); }
                    runner.AdvanceRenderFrame(1d / 60); view.RenderFrame();
                    if (press) { yield return ReleaseKeys(keyboard[controller.Bindings.P1.Attack]); controller.Capture(); }
                }
                Assert.That(ticks, Is.LessThan(200));
                for (int chain = 0; chain < 3; chain++) Assert.That(activeTicks[chain], Is.EqualTo(data.GroundNormals[chain].ToDefinition().ActiveFrames));
                Assert.That(body.sprite.name, Does.StartWith("idle_"), "Attack-only items must disappear on return to idle.");
            }
            runner.enabled = true;
            yield return null;
        }
        [UnityTest]
        public IEnumerator ChizuruThrowsOneActualBagSpriteInBothDirectionsAndSpecialEffectsStaySeparate()
        {
            var runner=Object.FindAnyObjectByType<BattleRunner>();var controller=Object.FindAnyObjectByType<InputController>();
            runner.enabled=false;runner.SetPaused(false);
            var view=Object.FindObjectsByType<FighterView>().Single(v=>v.PlayerIndex==0);
            var body=view.transform.Find("Body").GetComponent<SpriteRenderer>();var chizuru=runner.Setup.Character(2);
            foreach(int facing in new[]{1,-1})
            {
                runner.BeginMatch(chizuru,runner.Setup.Character(0),runner.CurrentStage,false);
                var directionKey=facing<0?keyboard.aKey:keyboard.dKey;
                yield return PressKeys(directionKey);controller.Capture();
                runner.AdvanceRenderFrame(1d/60);view.RenderFrame();
                yield return ReleaseKeys(directionKey);controller.Capture();
                Assert.That(runner.Simulation.P1.Facing,Is.EqualTo(facing));
                bool[] queued=new bool[2];bool observed=false;long projectileId=0;
                yield return PressKeys(keyboard[controller.Bindings.P1.Attack]);controller.Capture();
                runner.AdvanceRenderFrame(1d/60);view.RenderFrame();
                yield return ReleaseKeys(keyboard[controller.Bindings.P1.Attack]);controller.Capture();
                for(int tick=0;tick<100 && (runner.Simulation.P1.Attack!=null || runner.Simulation.Projectiles.Count>0);tick++)
                {
                    var attack=runner.Simulation.P1.Attack;
                    bool press=attack!=null && attack.ChainIndex<2 && !queued[attack.ChainIndex];
                    if(press){queued[attack.ChainIndex]=true;yield return PressKeys(keyboard[controller.Bindings.P1.Attack]);controller.Capture();}
                    runner.AdvanceRenderFrame(1d/60);view.RenderFrame();
                    if(press){yield return ReleaseKeys(keyboard[controller.Bindings.P1.Attack]);controller.Capture();}
                    yield return null;
                    if(runner.Simulation.Projectiles.Count==0)continue;
                    Assert.That(runner.Simulation.Projectiles.Count,Is.EqualTo(1));var shot=runner.Simulation.Projectiles[0];
                    Assert.That(shot.Attack.Definition.IsNormal,Is.True);Assert.That(shot.Facing,Is.EqualTo(facing));
                    if(!observed){projectileId=shot.Attack.Id;observed=true;}
                    Assert.That(shot.Attack.Id,Is.EqualTo(projectileId));
                    var sprite=Object.FindObjectsByType<SpriteRenderer>().Single(s=>s.name=="Projectile" && s.enabled);
                    int age=shot.Attack.Definition.ProjectileLifetimeFrames-shot.RemainingFrames;
                    Assert.That(sprite.sprite,Is.SameAs(chizuru.Animations.Effect("bag_throw",age)));
                    Assert.That(sprite.flipX,Is.EqualTo(facing<0));Assert.That(sprite.transform.localScale,Is.EqualTo(Vector3.one));
                    if(runner.Simulation.P1.Attack!=null && runner.Simulation.P1.Attack.IsActive)
                        Assert.That(body.sprite,Is.SameAs(chizuru.Animations.Find("attack3").Frames[2]));
                }
                Assert.That(observed,Is.True);Assert.That(runner.Simulation.Projectiles.Count,Is.Zero);
                Assert.That(chizuru.Animations.Effect("rice_ball",0),Is.Not.SameAs(chizuru.Animations.Effect("bag_throw",0)));
                Assert.That(IsIdleSprite(runner,view,body),Is.True);
            }
            runner.enabled=true;yield return null;
        }
        [UnityTest]
        public IEnumerator TwoKeyboardPlayersMoveIndependentlyAndRRestartsFinishedMatch()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            float first = runner.Simulation.P1.Position.X, second = runner.Simulation.P2.Position.X;
            yield return PressKeys(keyboard.dKey, keyboard.leftArrowKey);
            yield return WaitFor(() => runner.Simulation.P1.Position.X > first && runner.Simulation.P2.Position.X < second, "両プレイヤーの移動がtickに反映されませんでした。");
            Assert.That(keyboard[Key.D].isPressed, Is.True, "投入したP1入力がInput Systemへ届いていません。");
            Assert.That(keyboard[Key.LeftArrow].isPressed, Is.True, "投入したP2入力がInput Systemへ届いていません。");
            Assert.That(runner.Simulation.P1.Position.X, Is.GreaterThan(first));
            Assert.That(runner.Simulation.P2.Position.X, Is.LessThan(second));
            yield return ReleaseKeys(keyboard.dKey, keyboard.leftArrowKey);
            while (!runner.Simulation.Match.IsFinished) runner.Simulation.Tick(default, default);
            var finished = runner.Simulation;
            yield return PressKeys(keyboard.rKey);
            yield return WaitFor(() => runner.Simulation != finished, "Rキーで再戦しませんでした。");
            Assert.That(runner.Simulation, Is.Not.SameAs(finished));
            Assert.That(runner.Simulation.Match.IsFinished, Is.False);
            Assert.That(runner.Simulation.P1.Hearts, Is.EqualTo(3));
        }
        [UnityTest]
        public IEnumerator HeadCountersFollowMovingFighters()
        {
            var counter = Object.FindObjectsByType<Text>().Single(t => t.name == "P1 Head Counter");
            Vector2 start = counter.rectTransform.anchoredPosition;
            yield return PressKeys(keyboard.dKey, keyboard.spaceKey);
            yield return WaitFor(() => Vector2.Distance(start, counter.rectTransform.anchoredPosition) > 1, "頭上カウンターが移動に追従しませんでした。");
            Assert.That(Vector2.Distance(start, counter.rectTransform.anchoredPosition), Is.GreaterThan(1));
            yield return ReleaseKeys(keyboard.dKey, keyboard.spaceKey);
        }
        [UnityTest]
        public IEnumerator LaptopAndNumpadJumpBindingsBothReachPlayerTwo()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            yield return PressKeys(keyboard.rightCtrlKey);
            yield return WaitFor(() => runner.Simulation.P2.JumpsUsed == 1, "P2のノートPC用ジャンプキーが届きませんでした。");
            Assert.That(runner.Simulation.P2.JumpsUsed, Is.EqualTo(1));
            Assert.That(runner.Simulation.P1.JumpsUsed, Is.Zero);
            yield return ReleaseKeys(keyboard.rightCtrlKey);
            yield return PressKeys(keyboard.numpad0Key);
            yield return WaitFor(() => runner.Simulation.P2.JumpsUsed == 2, "P2のテンキー用二段ジャンプが届きませんでした。");
            Assert.That(runner.Simulation.P2.JumpsUsed, Is.EqualTo(2));
            Assert.That(runner.Simulation.P1.JumpsUsed, Is.Zero);
        }
        [UnityTest]
        public IEnumerator PhaseTwoKeysChangeDefenseStateAndHudShowsLinkGauge()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            yield return PressKeys(keyboard.lKey, keyboard.pageDownKey);
            yield return WaitFor(() => runner.Simulation.P1.Mode == AniBato.Battle.FighterMode.Guard && runner.Simulation.P2.Mode == AniBato.Battle.FighterMode.Guard, "両プレイヤーのガードが維持されませんでした。");
            Assert.That(runner.Simulation.P1.Mode, Is.EqualTo(AniBato.Battle.FighterMode.Guard));
            Assert.That(runner.Simulation.P2.Mode, Is.EqualTo(AniBato.Battle.FighterMode.Guard));
            Assert.That(Object.FindObjectsByType<Text>().Count(t => t.text.Contains("LINK 0/100")), Is.EqualTo(2));
            Assert.That(Object.FindObjectsByType<Text>().Count(t => t.name.Contains("Battle Popup")), Is.EqualTo(2));
        }
        [UnityTest]
        public IEnumerator CurrentKeyboardMovesBothPlayersWithoutExplicitAssignment()
        {
            var input = Object.FindAnyObjectByType<InputController>();
            // Start with a DIFFERENT assigned device, so ignoring AssignKeyboard(null) fails
            // gameplay assertions even though Keyboard.current is the intended keyboard.
            var otherKeyboard = InputSystem.AddDevice<Keyboard>();
            input.AssignKeyboard(otherKeyboard); input.AssignKeyboard(null);
            keyboard.MakeCurrent();
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            float first = runner.Simulation.P1.Position.X, second = runner.Simulation.P2.Position.X;
            yield return PressKeys(keyboard.dKey, keyboard.leftArrowKey);
            yield return WaitFor(() => runner.Simulation.P1.Position.X > first && runner.Simulation.P2.Position.X < second, "Keyboard.currentから両プレイヤーを動かせませんでした。");
            Assert.That(Keyboard.current, Is.SameAs(keyboard));
            Assert.That(keyboard[Key.D].isPressed, Is.True);
            Assert.That(runner.Simulation.P1.Position.X, Is.GreaterThan(first));
            Assert.That(runner.Simulation.P2.Position.X, Is.LessThan(second));
            yield return ReleaseKeys(keyboard.dKey, keyboard.leftArrowKey);
            InputSystem.RemoveDevice(otherKeyboard);
        }
        [UnityTest]
        public IEnumerator Spec19_AllThreeCharactersMoveAndUseNormalsAndSpecialsOnBothPlayerSlots()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            string[] names = { "透真", "蓮", "千鶴" };
            var views = Object.FindObjectsByType<FighterView>().OrderBy(view => view.PlayerIndex).ToArray();
            var bodies = views.Select(view => view.transform.Find("Body").GetComponent<SpriteRenderer>()).ToArray();
            for (int index = 0; index < 3; index++)
            {
                while (!runner.Simulation.Match.IsFinished) runner.Simulation.Tick(default, default);
                runner.SelectCharacter(0, index); runner.SelectCharacter(1, index); runner.RestartMatch(); yield return null;
                Assert.That(runner.Simulation.P1.Definition.Name, Is.EqualTo(names[index]));
                Assert.That(runner.Simulation.P2.Definition.Name, Is.EqualTo(names[index]));
                yield return WaitFor(() => views.All(view => IsIdleSprite(runner,view,bodies[view.PlayerIndex])), names[index] + " の実スプライトが両スロットに反映されませんでした。");
                float first = runner.Simulation.P1.Position.X, second = runner.Simulation.P2.Position.X;
                yield return PressKeys(keyboard.dKey, keyboard.leftArrowKey);
                yield return WaitFor(() => runner.Simulation.P1.Position.X > first && runner.Simulation.P2.Position.X < second, names[index] + " が両スロットで移動しませんでした。");
                Assert.That(runner.Simulation.P1.Position.X, Is.GreaterThan(first));
                Assert.That(runner.Simulation.P2.Position.X, Is.LessThan(second));
                yield return WaitFor(() => views.All(view => IsWalkSprite(runner,view,bodies[view.PlayerIndex])), "歩行のSpriteが反映されませんでした。");
                Assert.That(bodies[0].flipX, Is.False); Assert.That(bodies[1].flipX, Is.True);
                yield return ReleaseKeys(keyboard.dKey, keyboard.leftArrowKey);
                yield return PressKeys(keyboard.aKey, keyboard.rightArrowKey);
                yield return WaitFor(() => bodies[0].flipX && !bodies[1].flipX && views.All(view => IsWalkSprite(runner,view,bodies[view.PlayerIndex])), names[index] + " の向き反転が両スロットで描画されませんでした。");
                Assert.That(bodies[0].flipX, Is.True); Assert.That(bodies[1].flipX, Is.False);
                yield return ReleaseKeys(keyboard.aKey, keyboard.rightArrowKey);
                yield return PressKeys(keyboard.jKey, keyboard.deleteKey);
                yield return WaitFor(() => runner.Simulation.P1.Attack != null && runner.Simulation.P2.Attack != null, names[index] + " の通常攻撃が両スロットで開始しませんでした。");
                Assert.That(runner.Simulation.P1.Attack, Is.Not.Null); Assert.That(runner.Simulation.P1.Attack.Definition.IsNormal, Is.True);
                Assert.That(runner.Simulation.P2.Attack, Is.Not.Null); Assert.That(runner.Simulation.P2.Attack.Definition.IsNormal, Is.True);
                yield return ReleaseKeys(keyboard.jKey, keyboard.deleteKey);
                yield return WaitFor(() => runner.Simulation.P1.Attack == null && runner.Simulation.P2.Attack == null && runner.Simulation.P1.Mode == AniBato.Battle.FighterMode.Idle && runner.Simulation.P2.Mode == AniBato.Battle.FighterMode.Idle, "通常攻撃の硬直が完了しませんでした。");
                yield return PressKeys(keyboard.kKey, keyboard.endKey);
                yield return WaitFor(() => runner.Simulation.P1.Attack != null && runner.Simulation.P2.Attack != null, names[index] + " の特殊攻撃が両スロットで開始しませんでした。");
                Assert.That(runner.Simulation.P1.Attack, Is.Not.Null); Assert.That(runner.Simulation.P1.Attack.Definition.IsSpecial, Is.True);
                Assert.That(runner.Simulation.P2.Attack, Is.Not.Null); Assert.That(runner.Simulation.P2.Attack.Definition.IsSpecial, Is.True);
                Assert.That(runner.Simulation.P1.CurrentSpecial, Is.EqualTo(AniBato.Battle.SpecialSlot.Neutral));
                Assert.That(runner.Simulation.P2.CurrentSpecial, Is.EqualTo(AniBato.Battle.SpecialSlot.Neutral));
                yield return ReleaseKeys(keyboard.kKey, keyboard.endKey);
            }
        }
        [UnityTest]
        public IEnumerator ChizuruSpritesFollowMovementFacingAndActiveJab()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            while (!runner.Simulation.Match.IsFinished) runner.Simulation.Tick(default, default);
            runner.SelectCharacter(0, 2); runner.SelectCharacter(1, 0); runner.RestartMatch(); yield return null; yield return null;
            var view = Object.FindObjectsByType<FighterView>().Single(item => item.PlayerIndex == 0);
            var body = view.transform.Find("Body").GetComponent<SpriteRenderer>();
            yield return WaitFor(() => IsIdleSprite(runner,view,body), "再戦後の千鶴の待機Spriteが反映されませんでした。");
            Assert.That(view.UsesAnimationSprites, Is.True); Assert.That(body.sprite.name, Does.StartWith("idle_"));
            yield return PressKeys(keyboard.aKey);
            yield return WaitFor(() => IsWalkSprite(runner,view,body) && body.flipX, "千鶴の歩行と左向きが描画されませんでした。");
            Assert.That(body.sprite, Is.SameAs(runner.Character(0).PaperMotion.Sprite)); Assert.That(body.flipX, Is.True);
            yield return ReleaseKeys(keyboard.aKey);
            yield return PressKeys(keyboard.jKey);
            // Coroutine assertions can run before LateUpdate. Inspect the fixed-tick sample that
            // FighterView actually rendered, rather than comparing last render to a newer tick.
            bool sawActive = false;
            float activeDeadline = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < activeDeadline)
            {
                yield return null;
                var rendered = view.RenderedSample;
                if (!rendered.IsAttack || rendered.State != AniBato.Presentation.AnimationState.Attack1 || rendered.AttackFrame < rendered.Startup || rendered.AttackFrame >= rendered.Startup + rendered.Active) continue;
                // Source key 1 of 3 is mapped to frame 2 of the new six-frame strip.
                // Keep an exact active-pose assertion after adding inbetweens.
                Assert.That(runner.Character(0).Animations.Bindings.Single(binding => binding.State == AniBato.Presentation.AnimationState.Attack1).ActiveFrame, Is.EqualTo(2));
                Assert.That(body.sprite.name, Is.EqualTo("attack1_02")); Assert.That(body.flipX, Is.True); sawActive = true; break;
            }
            Assert.That(sawActive, Is.True, "有効持続の絵を観測できませんでした。");
            yield return ReleaseKeys(keyboard.jKey);
        }
        [UnityTest]
        public IEnumerator F9ShowsActualHurtboxesWithoutChangingSimulation()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            Assert.That(runner.DebugBoxes, Is.False);
            Assert.That(Object.FindAnyObjectByType<HitboxDebugView>(), Is.Not.Null);
            yield return PressKeys(keyboard.f9Key);
            yield return WaitFor(() => runner.DebugBoxes && Object.FindObjectsByType<LineRenderer>().Count(line => line.enabled) >= 2, "F9の判定表示が描画されませんでした。");
            Assert.That(runner.DebugBoxes, Is.True);
            Assert.That(Object.FindObjectsByType<LineRenderer>().Count(line => line.enabled), Is.GreaterThanOrEqualTo(2));
            yield return ReleaseKeys(keyboard.f9Key); yield return PressKeys(keyboard.f9Key);
            yield return WaitFor(() => !runner.DebugBoxes && Object.FindObjectsByType<LineRenderer>().All(line => !line.enabled), "F9の判定表示を閉じられませんでした。");
            Assert.That(runner.DebugBoxes, Is.False);
            Assert.That(Object.FindObjectsByType<LineRenderer>().All(line => !line.enabled), Is.True);
        }
        [UnityTest]
        public IEnumerator FinishedMatchFunctionKeysSelectCharactersAndRUpdatesViews()
        {
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            while (!runner.Simulation.Match.IsFinished) runner.Simulation.Tick(default, default);
            yield return PressKeys(keyboard.f2Key, keyboard.f4Key);
            yield return WaitFor(() => runner.Character(0).Movement.Name == "蓮" && runner.Character(1).Movement.Name == "千鶴", "F2/F4でキャラ選択が反映されませんでした。");
            Assert.That(runner.Character(0).Movement.Name, Is.EqualTo("蓮"));
            Assert.That(runner.Character(1).Movement.Name, Is.EqualTo("千鶴"));
            yield return ReleaseKeys(keyboard.f2Key, keyboard.f4Key);
            var finished = runner.Simulation;
            yield return PressKeys(keyboard.rKey);
            yield return WaitFor(() => runner.Simulation != finished && Object.FindObjectsByType<FighterView>().All(p => IsIdleSprite(runner,p,p.transform.Find("Body").GetComponent<SpriteRenderer>())) && Object.FindObjectsByType<Text>().Any(t => t.name == "Player 2 HUD" && t.text.Contains("千鶴")), "Rキーで再戦した実スプライトとHUDが反映されませんでした。");
            Assert.That(runner.Simulation.P1.Definition.Name, Is.EqualTo("蓮"));
            Assert.That(runner.Simulation.P2.Definition.Name, Is.EqualTo("千鶴"));
            var players = Object.FindObjectsByType<FighterView>();
            var ren = players.Single(p => p.PlayerIndex == 0);
            Assert.That(ren.UsesAnimationSprites, Is.True);
            Assert.That(ren.transform.Find("Body").GetComponent<SpriteRenderer>().sprite.name, Does.StartWith("idle_"));
            Assert.That(ren.transform.Find("Left Ear").GetComponent<SpriteRenderer>().enabled, Is.False, "実スプライトに仮図形の耳が重なっています。");
            var chizuru = players.Single(p => p.PlayerIndex == 1);
            Assert.That(chizuru.transform.Find("Chef Hat").GetComponent<SpriteRenderer>().enabled, Is.False);
            Assert.That(chizuru.UsesAnimationSprites, Is.True);
            Assert.That(chizuru.transform.Find("Body").GetComponent<SpriteRenderer>().sprite.name, Does.StartWith("idle_"));
            Assert.That(Object.FindObjectsByType<Text>().Any(t => t.name == "Player 2 HUD" && t.text.Contains("千鶴")), Is.True);
        }
    }
}




