using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class PaperCharacterSceneTests : InputTestFixture
    {
        private Keyboard keyboard;
        private BattleRunner runner;
        private InputController controller;
        private FighterView view;
        private SpriteRenderer body;
        private bool isolated, previousRun;
        private InputSettings.UpdateMode previousMode;
        private InputSettings.BackgroundBehavior previousBackground;
        private readonly Dictionary<string, string> savedPrefs = new Dictionary<string, string>();
        [SetUp] public override void Setup() { }
        [TearDown] public override void TearDown() { }
        [UnitySetUp]
        public IEnumerator Open()
        {
            previousRun = Application.runInBackground;
            base.Setup(); isolated = true; InputFixtureFocus.Configure(this);
            previousMode = InputSystem.settings.updateMode; previousBackground = InputSystem.settings.backgroundBehavior;
            Application.runInBackground = true; InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; InputSystem.Update();
            for (int player = 0; player < 2; player++) foreach (string device in new[] { "keyboard", "pad" })
            {
                string key = "AniBato.Controls.v1." + player + "." + device;
                savedPrefs[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null; PlayerPrefs.DeleteKey(key);
            }
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("Battle"); yield return null;
            keyboard.MakeCurrent(); controller = Object.FindAnyObjectByType<InputController>(); controller.AssignKeyboard(keyboard);
            runner = Object.FindAnyObjectByType<BattleRunner>(); runner.enabled = false; runner.SetPaused(false);
            runner.BeginMatch(runner.Setup.Character(2), runner.Setup.Character(0), runner.CurrentStage, false);
            view = Object.FindObjectsByType<FighterView>().Single(v => v.PlayerIndex == 0);
            body = view.transform.Find("Body").GetComponent<SpriteRenderer>(); view.RenderFrame();
        }
        [UnityTearDown]
        public IEnumerator Close()
        {
            if (runner) runner.enabled = false;
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            yield return null;
            foreach (var pair in savedPrefs) { if (pair.Value == null) PlayerPrefs.DeleteKey(pair.Key); else PlayerPrefs.SetString(pair.Key, pair.Value); }
            savedPrefs.Clear(); PlayerPrefs.Save();
            if (isolated)
            {
                InputSystem.settings.updateMode = previousMode; InputSystem.settings.backgroundBehavior = previousBackground;
                Application.runInBackground = previousRun; base.TearDown(); isolated = false;
            }
        }
        private IEnumerator Key(Key key, bool pressed)
        {
            if (pressed) Press(keyboard[key], queueEventOnly: true); else Release(keyboard[key], queueEventOnly: true);
            float deadline = Time.realtimeSinceStartup + 2;
            while (keyboard[key].isPressed != pressed && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(keyboard[key].isPressed, Is.EqualTo(pressed)); yield return null; controller.Capture();
        }
        private void Tick() { runner.AdvanceRenderFrame(1d / 60); view.RenderFrame(); }
        private static float Angle(SpriteRenderer sprite) => Mathf.DeltaAngle(0, sprite.transform.localEulerAngles.z);
        private void AssertPaper()
        {
            Assert.That(view.UsesPaperSprite, Is.True); Assert.That(body.sprite, Is.SameAs(runner.Character(0).PaperMotion.Sprite));
            Assert.That(body.transform.localScale, Is.EqualTo(Vector3.one)); Assert.That(view.transform.localRotation, Is.EqualTo(Quaternion.identity));
        }
        [UnityTest]
        public IEnumerator IdleAndBothWalkDirectionsUseFixedFifteenDegreeLeanAndCaptureUnityFrames()
        {
            AssertPaper(); Assert.That(view.RenderedSample.State, Is.EqualTo(AnimationState.Idle)); Assert.That(Angle(body), Is.EqualTo(0).Within(.001));
            Capture("idle", true); Capture("scene_idle", false);
            var data = runner.Character(0); var phases = new HashSet<int>();
            foreach (int direction in new[] { 1, -1 })
            {
                runner.RestartMatch(); view.RenderFrame();
                Key key = direction > 0 ? controller.Bindings.P1.Right : controller.Bindings.P1.Left;
                yield return Key(key, true); float start = runner.Simulation.P1.Position.X;
                phases.Clear();
                for (int i = 0; i < 60; i++)
                {
                    Tick(); AssertPaper(); Assert.That(view.RenderedSample.State, Is.EqualTo(AnimationState.Move));
                    Assert.That(body.flipX, Is.EqualTo(direction < 0));
                    var sample = PaperTravelLean.Evaluate(15, direction < 0, data.PaperMotion.Motion.FootMinX, data.PaperMotion.Motion.FootMaxX);
                    Assert.That(Angle(body), Is.EqualTo(sample.Degrees).Within(.001));
                    Assert.That(body.transform.localPosition.y, Is.EqualTo(-runner.Simulation.P1.Definition.HurtboxSize.Y * .5f + sample.Lift).Within(.00001));
                    if (phases.Add(sample.Phase)) Capture("walk_" + (direction > 0 ? "right_" : "left_") + sample.Phase.ToString("D2"), true);
                    Vector3 position = body.transform.localPosition; Quaternion rotation = body.transform.localRotation;
                    for (int frame = 0; frame < 3; frame++) { view.RenderFrame(); Assert.That(body.transform.localRotation, Is.EqualTo(rotation)); Assert.That(body.transform.localPosition, Is.EqualTo(position)); }
                }
                Assert.That(phases.Count, Is.EqualTo(1), "Walking must stay fixed, with no periodic sway.");
                Assert.That((runner.Simulation.P1.Position.X - start) * direction, Is.EqualTo(data.Movement.MoveSpeed).Within(.0001));
                yield return Key(key, false); Tick();
                Assert.That(view.RenderedSample.State, Is.EqualTo(AnimationState.Idle)); Assert.That(Angle(body), Is.EqualTo(0).Within(.001));
            }
        }
        [UnityTest]
        public IEnumerator PauseAttackAndRestartDoNotKeepWalkingTransform()
        {
            yield return Key(controller.Bindings.P1.Right, true); for (int i = 0; i < 35; i++) Tick();
            AssertPaper(); Assert.That(Mathf.Abs(Angle(body)), Is.GreaterThan(1));
            Quaternion pausedRotation = body.transform.localRotation; Vector3 pausedPosition = body.transform.localPosition;
            runner.SetPaused(true);
            for (int i = 0; i < 144; i++) { Tick(); Assert.That(body.transform.localRotation, Is.EqualTo(pausedRotation)); Assert.That(body.transform.localPosition, Is.EqualTo(pausedPosition)); }
            runner.SetPaused(false); yield return Key(controller.Bindings.P1.Right, false);
            yield return Key(controller.Bindings.P1.Attack, true); Tick();
            Assert.That(view.UsesPaperSprite, Is.True); Assert.That(body.sprite, Is.SameAs(runner.Character(0).PaperMotion.Find(AnimationState.Attack1).Frames[0]));
            Assert.That(Angle(body), Is.EqualTo(8).Within(.001));
            Assert.That(body.transform.localPosition.x, Is.EqualTo(-.06f).Within(.0001));
            var attackPose = runner.Character(0).PaperMotion.Find(AnimationState.Attack1);
            float attackLift = Mathf.Max(0, -Mathf.Min(attackPose.FootMinX, attackPose.FootMaxX) * Mathf.Sin(8 * Mathf.Deg2Rad));
            Assert.That(body.transform.localPosition.y, Is.EqualTo(-runner.Simulation.P1.Definition.HurtboxSize.Y * .5f + attackLift).Within(.0001));
            yield return Key(controller.Bindings.P1.Attack, false);
            for (int i = 0; i < 30; i++) Tick(); AssertPaper();
            runner.RestartMatch(); view.RenderFrame(); AssertPaper();
            Assert.That(view.RenderedSample.Age, Is.Zero); Assert.That(Angle(body), Is.EqualTo(0).Within(.001));
        }
        [UnityTest]
        public IEnumerator BothPlayerSlotsUsePaperAndOtherCharactersKeepTheirExistingSprites()
        {
            var chizuru = runner.Setup.Character(2);
            runner.BeginMatch(chizuru, chizuru, runner.CurrentStage, false);
            var views = Object.FindObjectsByType<FighterView>().OrderBy(v => v.PlayerIndex).ToArray();
            foreach (var item in views) { item.RenderFrame(); Assert.That(item.UsesPaperSprite, Is.True); Assert.That(item.transform.Find("Body").GetComponent<SpriteRenderer>().sprite, Is.SameAs(chizuru.PaperMotion.Sprite)); }
            for (int c = 0; c < 2; c++)
            {
                var other = runner.Setup.Character(c); runner.BeginMatch(other, chizuru, runner.CurrentStage, false); view.RenderFrame();
                Assert.That(view.UsesPaperSprite, Is.False); Assert.That(body.sprite, Is.SameAs(other.Animations.Find("idle").Frames[0]));
                Assert.That(body.transform.localRotation, Is.EqualTo(Quaternion.identity));
            }
            yield return null;
        }
        [UnityTest]
        public IEnumerator DisabledPaperProfileReturnsToExistingPixelSprites()
        {
            var original = runner.Setup.Character(2); var character = Object.Instantiate(original); var paper = Object.Instantiate(original.PaperMotion);
            try
            {
                paper.Enabled = false; character.PaperMotion = paper;
                runner.BeginMatch(character, runner.Setup.Character(0), runner.CurrentStage, false); view.RenderFrame();
                Assert.That(view.UsesPaperSprite, Is.False); Assert.That(body.sprite, Is.SameAs(original.Animations.Find("idle").Frames[0]));
                yield return Key(controller.Bindings.P1.Right, true); Tick();
                Assert.That(body.sprite, Is.SameAs(original.Animations.Find("walk").Frames[0])); Assert.That(body.transform.localRotation, Is.EqualTo(Quaternion.identity));
                yield return Key(controller.Bindings.P1.Right, false);
            }
            finally { runner.BeginMatch(original, runner.Setup.Character(0), runner.CurrentStage, false); Object.Destroy(character); Object.Destroy(paper); }
        }
        [UnityTest]
        public IEnumerator DashAndDefensiveActionsUseTheirPaperPosesWithoutTravelWobble()
        {
            var data = runner.Character(0); var keys = controller.Bindings.P1;
            foreach (int direction in new[] { 1, -1 })
            {
                runner.RestartMatch(); Key travel = direction > 0 ? keys.Right : keys.Left;
                yield return Key(travel, true); yield return Key(keys.Dash, true);
                for (int tick = 0; tick < 20; tick++)
                {
                    Tick(); Assert.That(view.RenderedSample.State, Is.EqualTo(AnimationState.Dash));
                    Assert.That(view.UsesPaperSprite, Is.True); Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Find(AnimationState.Dash).Frames[0]));
                    Assert.That(Angle(body), Is.EqualTo(direction > 0 ? -45 : 45).Within(.001));
                    if (tick == 0) Capture(direction > 0 ? "dash_right" : "dash_left", true);
                }
                yield return Key(keys.Dash, false); yield return Key(travel, false);
            }
            Key[] actions = { keys.Down, keys.Guard, keys.Dodge, keys.Counter, keys.Attack, keys.Special };
            AnimationState[] expected = { AnimationState.Crouch, AnimationState.Guard, AnimationState.Dodge, AnimationState.Counter, AnimationState.Attack1, AnimationState.SpecialNeutral };
            for (int i = 0; i < actions.Length; i++)
            {
                runner.RestartMatch(); yield return Key(actions[i], true); Tick();
                Assert.That(view.RenderedSample.State, Is.EqualTo(expected[i])); Assert.That(view.UsesPaperSprite, Is.True);
                Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Find(expected[i]).Frames[0]));
                Assert.That(Angle(body), Is.EqualTo(i >= 4 ? 8 : 0).Within(.001)); Capture(expected[i].ToString(), true);
                yield return Key(actions[i], false);
            }
        }
        [UnityTest]
        public IEnumerator WholeSpriteAttackAndTumbleRenderHeldKeyPosesWithoutChangingPhysics()
        {
            var player = ((FighterAnimationPlayer[])typeof(BattleRunner).GetField("animations", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(runner))[0];
            var sampleProperty = typeof(FighterAnimationPlayer).GetProperty("Sample");
            var original = player.Sample; var sim = runner.Simulation; var position = sim.P1.Position; int ticks = sim.Match.TickCount;
            try
            {
                foreach (int frame in new[] { 0, 8, 9, 13, 14, 17 })
                {
                    var sample = new AnimationSample { State = AnimationState.Attack1, IsAttack = true, AttackFrame = frame, Startup = 9, Active = 5, Recovery = 10 };
                    sampleProperty.SetValue(player, sample); view.RenderFrame();
                    float angle = frame < 9 ? 8 : frame < 14 ? -15 : frame < 17 ? 3.2f : 0;
                    Assert.That(Angle(body), Is.EqualTo(angle).Within(.001));
                    Assert.That(body.transform.localScale, Is.EqualTo(Vector3.one));
                    if (frame == 0 || frame == 9) Capture("attack_key_" + frame, true);
                }
                var pose = runner.Character(0).PaperMotion.Find(AnimationState.Knockback);
                for (int age = 0; age < 12; age++)
                {
                    sampleProperty.SetValue(player, new AnimationSample { State = AnimationState.Knockback, Age = age }); view.RenderFrame();
                    float angle = age / 3 * 45;
                    Assert.That(Angle(body), Is.EqualTo(angle).Within(.001));
                    Vector3 center = body.transform.TransformPoint(new Vector3(0, pose.HeadHeight * .5f, 0));
                    Assert.That(center.x, Is.EqualTo(view.transform.position.x).Within(.0001));
                    Assert.That(center.y, Is.EqualTo(view.transform.position.y - sim.P1.Definition.HurtboxSize.Y * .5f + pose.HeadHeight * .5f).Within(.0001));
                    if (age % 3 == 0) Capture("tumble_" + age, true);
                }
                Assert.That(sim.Match.TickCount, Is.EqualTo(ticks)); Assert.That(sim.P1.Position, Is.EqualTo(position));
            }
            finally { sampleProperty.SetValue(player, original); view.RenderFrame(); }
            yield return null;
        }
        [UnityTest]
        public IEnumerator EveryDirectionalAirAttackKeepsItsPaperPoseFromStartupThroughRecovery()
        {
            var keys = controller.Bindings.P1; var data = runner.Character(0);
            Key[][] directions = { new Key[0], new[] { keys.Right }, new[] { keys.Left }, new[] { keys.Up }, new[] { keys.Down }, new[] { keys.Right, keys.Up }, new[] { keys.Right, keys.Down } };
            AnimationState[] expected = { AnimationState.AirNeutral, AnimationState.AirForward, AnimationState.AirBack, AnimationState.AirUp, AnimationState.AirDown, AnimationState.AirForwardUp, AnimationState.AirForwardDown };
            for (int action = 0; action < directions.Length; action++)
            {
                runner.RestartMatch(); yield return Key(keys.Jump, true); Tick();
                yield return Key(keys.Jump, false); for (int tick = 0; tick < 10; tick++) Tick();
                Assert.That(runner.Simulation.P1.Grounded, Is.False);
                foreach (var key in directions[action]) yield return Key(key, true);
                yield return Key(keys.Attack, true); Tick();
                Assert.That(view.RenderedSample.State, Is.EqualTo(expected[action]));
                yield return Key(keys.Attack, false);
                foreach (var key in directions[action]) yield return Key(key, false);
                int visible = 0, active = 0, recovery = 0;
                while (runner.Simulation.P1.Attack != null && visible < 120)
                {
                    var attack = runner.Simulation.P1.Attack;
                    Assert.That(view.RenderedSample.State, Is.EqualTo(expected[action])); Assert.That(view.UsesPaperSprite, Is.True);
                    Assert.That(body.sprite, Is.SameAs(data.PaperMotion.Find(expected[action]).Frames[0]));
                    if (attack.IsActive) active++; if (attack.Frame >= attack.Definition.StartupFrames + attack.Definition.ActiveFrames) recovery++;
                    if (visible++ == 0) Capture(expected[action].ToString(), true);
                    Tick();
                }
                Assert.That(visible, Is.GreaterThanOrEqualTo(12)); Assert.That(visible, Is.LessThan(120));
                Assert.That(active, Is.GreaterThan(0)); Assert.That(recovery, Is.GreaterThan(0));
            }
        }
        private void Capture(string name, bool isolateBody)
        {
            var camera = Camera.main; Assert.That(camera, Is.Not.Null);
            Vector3 position = camera.transform.position; float size = camera.orthographicSize;
            Color clear = camera.backgroundColor; var flags = camera.clearFlags; var target = camera.targetTexture;
            int mask = camera.cullingMask, layer = body.gameObject.layer;
            int width = isolateBody ? 512 : 1280, height = isolateBody ? 512 : 720;
            var texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var previousActive = RenderTexture.active; var image = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                if (isolateBody)
                {
                    body.gameObject.layer = 31; camera.cullingMask = 1 << 31; camera.orthographicSize = 1.8f;
                    camera.transform.position = new Vector3(view.transform.position.x, view.transform.position.y - runner.Simulation.P1.Definition.HurtboxSize.Y * .5f + 1.12f, position.z);
                    camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.125f, .15f, .19f, 1);
                }
                camera.targetTexture = texture; camera.Render(); RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../../docs/screenshots/paper_theatre_unity")); Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, name + ".png"), image.EncodeToPNG());
            }
            finally
            {
                body.gameObject.layer = layer; camera.cullingMask = mask; camera.transform.position = position; camera.orthographicSize = size;
                camera.backgroundColor = clear; camera.clearFlags = flags; camera.targetTexture = target;
                RenderTexture.active = previousActive; RenderTexture.ReleaseTemporary(texture); Object.Destroy(image);
            }
        }
    }
}
