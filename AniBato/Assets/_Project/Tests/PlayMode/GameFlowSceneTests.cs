using System.Collections;
using System.Linq;
using UnityEngine.InputSystem.Controls;
using AniBato.Game;
using AniBato.Battle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using AniBato.Presentation;
using UnityEngine.InputSystem.DualShock;

namespace AniBato.Tests
{
    public sealed class GameFlowSceneTests : InputTestFixture
    {
        private Keyboard keyboard;
        private readonly System.Collections.Generic.Dictionary<string, string> controlPrefs = new System.Collections.Generic.Dictionary<string, string>();
        private InputSettings.BackgroundBehavior previousBackgroundBehavior;
        private InputSettings.UpdateMode previousUpdateMode;
        private bool previousRunInBackground;
        private int? doubleTapPref;
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
            var oldFlow = Object.FindAnyObjectByType<GameFlowController>(); if (oldFlow) oldFlow.enabled = false;
            var oldRunner = Object.FindAnyObjectByType<BattleRunner>(); if (oldRunner) oldRunner.enabled = false;
            var oldInput = Object.FindAnyObjectByType<InputController>(); if (oldInput) oldInput.enabled = false;
            IsolateInput();
            doubleTapPref = PlayerPrefs.HasKey(ControlConfiguration.DoubleTapStorage) ? PlayerPrefs.GetInt(ControlConfiguration.DoubleTapStorage) : (int?)null;
            PlayerPrefs.DeleteKey(ControlConfiguration.DoubleTapStorage);
            for (int p = 0; p < 2; p++) foreach (string device in new[] { "pad", "keyboard" })
            { string key = "AniBato.Controls.v1." + p + "." + device; controlPrefs[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null; PlayerPrefs.DeleteKey(key); }
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("Title");
            yield return null;
            keyboard.MakeCurrent();
            Object.FindAnyObjectByType<InputController>().AssignKeyboard(keyboard);
            Assert.That(keyboard.enabled, Is.True, "テストキーボードがフォーカス処理で無効化されています。");
        }
        [UnityTearDown]
        public IEnumerator CloseScene()
        {
            try
            {
            // Stop the old scene before restoring InputTestFixture's global state buffers.
            // Otherwise its Update can read controls from the previous isolated runtime.
            var flow = Object.FindAnyObjectByType<GameFlowController>();
            if (flow) flow.enabled = false;
            var runner = Object.FindAnyObjectByType<BattleRunner>();
            if (runner) runner.enabled = false;
            var input = Object.FindAnyObjectByType<InputController>();
            if (input) input.enabled = false;
            foreach (var device in InputSystem.devices.ToArray()) InputSystem.RemoveDevice(device);
            dualSenseButtons.Clear();
            yield return null;
            foreach (var entry in controlPrefs) { if (entry.Value == null) PlayerPrefs.DeleteKey(entry.Key); else PlayerPrefs.SetString(entry.Key, entry.Value); }
            controlPrefs.Clear(); PlayerPrefs.Save();
            }
            finally
            {
                if (doubleTapPref.HasValue) PlayerPrefs.SetInt(ControlConfiguration.DoubleTapStorage, doubleTapPref.Value);
                else PlayerPrefs.DeleteKey(ControlConfiguration.DoubleTapStorage);
                PlayerPrefs.Save(); RestoreInput();
            }
        }
        private static IEnumerator WaitFor(System.Func<bool> condition, string message)
        {
            float deadline = Time.realtimeSinceStartup + 6f;
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
                if (button.device is DualSenseGamepadHID)
                {
                    // DualSense rejects DeltaStateEvent (the fixture's Press/Release format).
                    // Deliver an actual USB HID report, with centered sticks and neutral hat.
                    if (pressed) dualSenseButtons.Add(button); else dualSenseButtons.Remove(button);
                    InputSystem.QueueStateEvent(button.device, DualSenseUsbReport.From((Gamepad)button.device, dualSenseButtons));
                }
                else if (pressed) Press(button, queueEventOnly: true); else Release(button, queueEventOnly: true);
                yield return WaitFor(() => button.isPressed == pressed, button.path + " の入力イベントが反映されませんでした。");
            }
            // Callers also wait for Capture -> tick -> LateUpdate, not an assumed frame count.
            yield return null;
        }
        private IEnumerator PressKeys(params ButtonControl[] buttons) => ChangeKeys(true, buttons);
        private IEnumerator ReleaseKeys(params ButtonControl[] buttons) => ChangeKeys(false, buttons);
        private GameFlowController Flow => Object.FindAnyObjectByType<GameFlowController>();
        private IEnumerator Tap(ButtonControl key) { yield return PressKeys(key); yield return ReleaseKeys(key); }
        private IEnumerator SelectLocalBattle()
        {
            yield return Tap(keyboard.enterKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.MainMenu));
            yield return Tap(keyboard.sKey); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.CharacterSelect)); Assert.That(Flow.CpuMode, Is.False);
            yield return Tap(keyboard.dKey); yield return Tap(keyboard.leftArrowKey);
            yield return Tap(keyboard.jKey); Assert.That(Flow.Ready(0), Is.True);
            yield return Tap(keyboard.deleteKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.StageSelect));
            yield return Tap(keyboard.dKey); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Countdown));
            Assert.That(Flow.Runner.Simulation.Match.TickCount, Is.Zero);
            Assert.That(Flow.Runner.Simulation.Match.RemainingTicks, Is.EqualTo(4200));
            yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "カウントダウン後に対戦が始まりませんでした。");
        }
        [UnityTest]
        public IEnumerator ActualDualSenseAndXboxDevicesSwitchAllActionLabels()
        {
            var controller = Object.FindAnyObjectByType<InputController>();
            var dualSense = (Gamepad)InputSystem.AddDevice("DualSenseGamepadHID");
            Gamepad xbox = null;
            try
            {
            InputSystem.QueueStateEvent(dualSense, DualSenseUsbReport.From(dualSense, dualSenseButtons));
            yield return null;
            controller.AssignGamepad(0, dualSense);
            yield return Tap(dualSense.buttonSouth);
            Assert.That(controller.Label(0, ControlAction.Jump), Is.EqualTo("×")); Assert.That(controller.Label(0, ControlAction.Attack), Is.EqualTo("□"));
            Assert.That(controller.Label(0, ControlAction.Pause), Is.EqualTo("OPTIONS"));
            xbox = (Gamepad)InputSystem.AddDevice("XInputController"); controller.AssignGamepad(0, xbox);
            yield return Tap(xbox.buttonSouth);
            Assert.That(controller.Label(0, ControlAction.Jump), Is.EqualTo("A")); Assert.That(controller.Label(0, ControlAction.Attack), Is.EqualTo("X"));
            yield return Tap(keyboard.dKey); Assert.That(controller.Label(0, ControlAction.Jump), Is.EqualTo("Space"));
            }
            finally
            {
                controller.AssignGamepad(0, null);
                if (dualSense.added) InputSystem.RemoveDevice(dualSense);
                if (xbox != null && xbox.added) InputSystem.RemoveDevice(xbox);
                dualSenseButtons.Clear();
            }
        }
        private readonly System.Collections.Generic.HashSet<ButtonControl> dualSenseButtons = new System.Collections.Generic.HashSet<ButtonControl>();
        [UnityTest]
        public IEnumerator DualSenseUsbReportsDeliverEveryBattleButtonAndRepeatedMenuPolling()
        {
            Flow.enabled = false; Flow.Runner.enabled = false;
            var controller = Flow.Input;
            var pad = (Gamepad)InputSystem.AddDevice("DualSenseGamepadHID");
            try
            {
                controller.AssignGamepad(0, pad);
                InputSystem.QueueStateEvent(pad, DualSenseUsbReport.From(pad, dualSenseButtons)); yield return null;
                for (int action = 4; action <= 11; action++)
                {
                    var button = ControlConfiguration.PadButton(pad, controller.Configuration.Profile(0, true)[(ControlAction)action]);
                    yield return PressKeys(button);
                    var first = controller.ReadMenu(0); var second = controller.ReadMenu(0);
                    Assert.That(first.Horizontal, Is.EqualTo(second.Horizontal)); Assert.That(first.Vertical, Is.EqualTo(second.Vertical));
                    controller.Capture(); var frame = controller.Consume(0); var flag = (InputButtons)(1 << (action - 4));
                    Assert.That(frame.IsHeld(flag), Is.True); Assert.That(frame.IsPressed(flag), Is.True);
                    Assert.That(frame.X, Is.Zero); Assert.That(frame.Y, Is.Zero);
                    yield return ReleaseKeys(button); controller.Capture(); Assert.That(controller.Consume(0).IsHeld(flag), Is.False);
                }
            }
            finally { controller.AssignGamepad(0, null); if (pad.added) InputSystem.RemoveDevice(pad); dualSenseButtons.Clear(); }
        }
        [UnityTest]
        public IEnumerator DoubleTapSettingUiPersistsAndDedicatedDashStillReachesBothPlayers()
        {
            yield return Tap(keyboard.enterKey);
            for (int i = 0; i < 7; i++) yield return Tap(keyboard.sKey);
            Assert.That(Flow.Input.Configuration.DoubleTapDashEnabled, Is.True);
            yield return Tap(keyboard.enterKey); Assert.That(Flow.Input.Configuration.DoubleTapDashEnabled, Is.False);
            var reload = new ControlConfiguration(); reload.Initialize(Flow.Input.Bindings); Assert.That(reload.DoubleTapDashEnabled, Is.False);
            Assert.That(LearningText.DashHelp(Flow.Input), Does.Contain("OFF"));
            yield return Tap(keyboard.wKey); yield return Tap(keyboard.enterKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.KeyConfig));
            for (int i = 0; i < 16; i++) yield return Tap(keyboard.sKey);
            yield return Tap(keyboard.enterKey); Assert.That(Flow.Input.Configuration.DoubleTapDashEnabled, Is.True);
            Assert.That(Flow.CapturingBinding, Is.False);
            yield return Tap(keyboard.enterKey); Assert.That(Flow.Input.Configuration.DoubleTapDashEnabled, Is.False);
            Flow.enabled = false; Flow.Runner.enabled = false;
            var input = Flow.Input; var pad = InputSystem.AddDevice<Gamepad>(); input.AssignGamepad(1, pad);
            var dashKey = keyboard[input.EffectiveKeys(0).Dash];
            yield return PressKeys(dashKey, pad.leftStickButton); input.Capture();
            for (int p = 0; p < 2; p++)
            { var frame = input.Consume(p); Assert.That(frame.DisableDoubleTapDash, Is.True); Assert.That(frame.IsPressed(InputButtons.Dash), Is.True); }
            yield return ReleaseKeys(dashKey, pad.leftStickButton);
        }
        [UnityTest]
        public IEnumerator CameraAndBoardBackdropFitThreeAspectRatiosWithoutChangingBattle()
        {
            yield return SelectLocalBattle(); Flow.enabled = false; Flow.Runner.SetPaused(true);
            var sim = Flow.Runner.Simulation; int tick = sim.Match.TickCount; var position = sim.P1.Position;
            var camera = Camera.main; var controller = camera.GetComponent<CameraController>();
            controller.enabled = false; controller.enabled = true;
            foreach (float aspect in new[] { 16f / 9, 16f / 10, 4f / 3 })
            {
                camera.aspect = aspect; yield return null; yield return null;
                for (int i = 1; i < sim.Stage.PlatformCount; i++)
                {
                    var bounds = sim.Stage[i].Bounds;
                    foreach (float x in new[] { bounds.Left, bounds.Right })
                    {
                        var v = camera.WorldToViewportPoint(new Vector3(x, bounds.Top, 0));
                        Assert.That(v.x, Is.GreaterThan(0).And.LessThan(1)); Assert.That(v.y, Is.GreaterThan(0).And.LessThan(1));
                    }
                }
            }
            Assert.That(Object.FindObjectsByType<SpriteRenderer>().Count(s => s.name.StartsWith("Board Backdrop Cell ")), Is.EqualTo(36));
            Assert.That(Object.FindObjectsByType<SpriteRenderer>().Single(s => s.name == "Far Painted Scenery").sprite, Is.Not.Null);
            Assert.That(sim.Match.TickCount, Is.EqualTo(tick)); Assert.That(sim.P1.Position, Is.EqualTo(position));
        }
        [UnityTest]
        public IEnumerator KeyConfigUiCapturesPersistsResetsAndSwapsPadDuplicates()
        {
            yield return Tap(keyboard.enterKey);
            for (int i = 0; i < 6; i++) yield return Tap(keyboard.sKey);
            yield return Tap(keyboard.enterKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.KeyConfig));
            for (int i = 0; i < 6; i++) yield return Tap(keyboard.sKey);
            yield return Tap(keyboard.enterKey); Assert.That(Flow.CapturingBinding, Is.True);
            yield return Tap(keyboard.hKey); Assert.That(Flow.CapturingBinding, Is.False);
            var controller = Flow.Input; Assert.That(controller.EffectiveKeys(0).Jump, Is.EqualTo(Key.H));
            var reloaded = new ControlConfiguration(); reloaded.Initialize(controller.Bindings); Assert.That(reloaded.Keyboard(0, controller.Bindings.P1).Jump, Is.EqualTo(Key.H));
            Assert.That(controller.EffectiveKeys(1).Jump, Is.EqualTo(Key.RightCtrl)); controller.Configuration.Reset(0, false);
            Assert.That(controller.EffectiveKeys(0).Jump, Is.EqualTo(Key.Space));
            var pad = InputSystem.AddDevice<Gamepad>(); controller.AssignGamepad(0, pad);
            controller.Configuration.Profile(0, true).Rebind(ControlAction.Jump, 6); controller.Configuration.Save(0, true);
            Assert.That(controller.Configuration.Profile(0, true)[ControlAction.Attack], Is.EqualTo(5));
            yield return PressKeys(pad.buttonWest); controller.Capture(); Assert.That(controller.Consume(0).IsHeld(InputButtons.Jump), Is.True);
            yield return ReleaseKeys(pad.buttonWest); controller.Configuration.Reset(0, true);
        }
        [UnityTest]
        public IEnumerator TrainingAndCommandsKeepMatchStoppedInsidePanels()
        {
            yield return Tap(keyboard.enterKey);
            for (int i = 0; i < 4; i++) yield return Tap(keyboard.sKey);
            yield return Tap(keyboard.enterKey); Assert.That(Flow.PracticeMode, Is.True);
            yield return Tap(keyboard.tabKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.CommandList));
            yield return Tap(keyboard.escapeKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.CharacterSelect));
            yield return Tap(keyboard.enterKey); yield return Tap(keyboard.enterKey);
            yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "Training did not start.");
            yield return new WaitForSecondsRealtime(.2f); Assert.That(Flow.Runner.Simulation.Match.RemainingTicks, Is.EqualTo(4200));
            Assert.That(Flow.Runner.Simulation.P2.Hearts, Is.EqualTo(3));
            yield return Tap(keyboard.escapeKey); int tick = Flow.Runner.Simulation.Match.TickCount;
            for (int i = 0; i < 8; i++) yield return Tap(keyboard.sKey);
            yield return Tap(keyboard.enterKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.CommandList));
            yield return new WaitForSecondsRealtime(.15f); Assert.That(Flow.Runner.Simulation.Match.TickCount, Is.EqualTo(tick));
        }
        [UnityTest]
        public IEnumerator TitleToLocalBattleUsesSelectedCharactersAndStage()
        {
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Title)); Assert.That(Flow.Runner.Simulation, Is.Null);
            yield return SelectLocalBattle();
            Assert.That(Flow.Runner.Simulation.P1.Definition.Name, Is.EqualTo("蓮"));
            Assert.That(Flow.Runner.Simulation.P2.Definition.Name, Is.EqualTo("透真"));
            Assert.That(Flow.Runner.CurrentStage.Theme, Is.EqualTo(StageTheme.BoardArena));
            yield return WaitFor(() => Flow.Runner.Simulation.Match.TickCount > 0, "対戦tickが進みませんでした。");
        }
        [UnityTest]
        public IEnumerator PauseAndItsControlsFreezeTicksAndResumeDoesNotInjectAttack()
        {
            yield return SelectLocalBattle(); yield return Tap(keyboard.escapeKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause)); int tick = Flow.Runner.Simulation.Match.TickCount;
            yield return new WaitForSecondsRealtime(0.15f); Assert.That(Flow.Runner.Simulation.Match.TickCount, Is.EqualTo(tick));
            yield return Tap(keyboard.sKey); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Controls));
            yield return new WaitForSecondsRealtime(0.15f); Assert.That(Flow.Runner.Simulation.Match.TickCount, Is.EqualTo(tick));
            yield return Tap(keyboard.escapeKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause));
            yield return Tap(keyboard.jKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Battle));
            Assert.That(Flow.Runner.Simulation.P1.Attack, Is.Null, "再開ボタンが通常攻撃へ漏れています。");
            yield return WaitFor(() => Flow.Runner.Simulation.Match.TickCount > tick, "ポーズから再開できませんでした。");
        }
        [UnityTest]
        public IEnumerator ResultRematchResetsEntireMatchAndKeepsSelection()
        {
            yield return SelectLocalBattle(); var previous = Flow.Runner.Simulation;
            while (!previous.Match.IsFinished) previous.Tick(default, default);
            yield return WaitFor(() => Flow.Screen == GameScreen.Result, "試合終了後にリザルトへ移りませんでした。");
            var pictures = Object.FindObjectsByType<Image>().Where(i => i.name.StartsWith("Selection Picture") && i.enabled).ToArray();
            Assert.That(pictures.Length, Is.EqualTo(2)); Assert.That(pictures.All(i => i.sprite.name.StartsWith("lose_")), Is.True);
            yield return Tap(keyboard.enterKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Countdown));
            var fresh = Flow.Runner.Simulation;
            Assert.That(fresh, Is.Not.SameAs(previous)); Assert.That(fresh.P1.Hearts, Is.EqualTo(3));
            Assert.That(fresh.P1.Gauge, Is.Zero); Assert.That(fresh.P1.NormalHitCount, Is.Zero);
            Assert.That(fresh.Match.TickCount, Is.Zero); Assert.That(fresh.Stage[0].Bounds.Size.X, Is.EqualTo(36));
            Assert.That(fresh.Shrink.CompletedSteps, Is.Zero); Assert.That(fresh.Stage.IsActive(1), Is.True);
            Assert.That(fresh.P1.Definition.Name, Is.EqualTo("蓮")); Assert.That(Flow.StageIndex, Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator CpuSelectionKeepsDifficultyAndIgnoresPlayerTwoBattleInput()
        {
            yield return Tap(keyboard.enterKey); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.CpuMode, Is.True); yield return Tap(keyboard.sKey); yield return Tap(keyboard.dKey);
            yield return Tap(keyboard.sKey); yield return Tap(keyboard.dKey);
            Assert.That(Flow.Player2Index, Is.EqualTo(2)); Assert.That(Flow.Difficulty, Is.EqualTo(CpuDifficulty.Hard));
            yield return Tap(keyboard.enterKey); yield return Tap(keyboard.enterKey);
            yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "CPU対戦が始まりませんでした。");
            Assert.That(Flow.Runner.CpuBrain, Is.TypeOf<CpuBrain>());
            Assert.That(((CpuBrain)Flow.Runner.CpuBrain).Definition.ReactionFrames, Is.EqualTo(Flow.Setup.CpuDifficulties[2].Definition.ReactionFrames));
            var mirror = new BattleSimulation(Flow.Runner.Character(0).ToDefinition(), Flow.Runner.Character(1).ToDefinition(),
                Flow.Runner.Simulation.Rules, Flow.Runner.CurrentStage.ToGeometry());
            var brain = new CpuBrain(Flow.Setup.CpuDifficulties[2].ToDefinition(), Flow.Setup.CpuSeed);
            yield return PressKeys(keyboard.leftArrowKey, keyboard.deleteKey); yield return new WaitForSecondsRealtime(0.15f);
            while (mirror.Match.TickCount < Flow.Runner.Simulation.Match.TickCount) mirror.Tick(default, brain.NextInput(mirror, 1));
            Assert.That(Flow.Runner.Simulation.P2.Position.X, Is.EqualTo(mirror.P2.Position.X), "P2キーボード入力がCPU戦へ混入しています。");
            Assert.That(Flow.Runner.Simulation.P2.Position.Y, Is.EqualTo(mirror.P2.Position.Y));
            Assert.That(Flow.Runner.Simulation.P2.Mode, Is.EqualTo(mirror.P2.Mode));
            Assert.That(Flow.Difficulty, Is.EqualTo(CpuDifficulty.Hard));
            yield return ReleaseKeys(keyboard.leftArrowKey, keyboard.deleteKey);
        }
        [UnityTest]
        public IEnumerator HeldConfirmAndCancelRequireAllPlayersToReleaseAcrossScreens()
        {
            yield return Tap(keyboard.enterKey); yield return Tap(keyboard.sKey); yield return Tap(keyboard.enterKey);
            // Both readiness buttons caused this transition. Hold P1's actual readiness
            // button too; unrelated buttons must now remain available (PHASE5B).
            yield return PressKeys(keyboard.enterKey);
            yield return PressKeys(keyboard.deleteKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.StageSelect));
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.StageSelect));
            yield return PressKeys(keyboard.enterKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.StageSelect));
            yield return ReleaseKeys(keyboard.deleteKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.StageSelect));
            yield return ReleaseKeys(keyboard.enterKey); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Countdown));
            yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "対戦へ進みませんでした。");
            yield return PressKeys(keyboard.escapeKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause));
            yield return new WaitForSecondsRealtime(.15f); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause));
            yield return ReleaseKeys(keyboard.escapeKey); yield return PressKeys(keyboard.jKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Battle));
            yield return new WaitForSecondsRealtime(.15f); Assert.That(Flow.Runner.Simulation.P1.Attack, Is.Null);
            yield return ReleaseKeys(keyboard.jKey);
        }
        [UnityTest]
        public IEnumerator ReconnectingPadsKeepOriginalSlotsAndNeverControlBothPlayers()
        {
            int firstId = InputFixtureFocus.ReportGamepad(this, new UnityEngine.InputSystem.Layouts.InputDeviceDescription { deviceClass = "Gamepad", interfaceName = "Test", serial = "P1-test-pad" });
            int secondId = InputFixtureFocus.ReportGamepad(this, new UnityEngine.InputSystem.Layouts.InputDeviceDescription { deviceClass = "Gamepad", interfaceName = "Test", serial = "P2-test-pad" });
            InputSystem.Update();
            var first = (Gamepad)InputSystem.GetDeviceById(firstId); var second = (Gamepad)InputSystem.GetDeviceById(secondId);
            var input = Object.FindAnyObjectByType<InputController>();
            Assert.That(input.AssignedGamepad(0), Is.SameAs(first)); Assert.That(input.AssignedGamepad(1), Is.SameAs(second));
            InputFixtureFocus.ReportRemoved(this, first); InputFixtureFocus.ReportRemoved(this, second); InputSystem.Update();
            Assert.That(input.AssignedGamepad(0), Is.Null); Assert.That(input.AssignedGamepad(1), Is.Null);
            InputFixtureFocus.ReportGamepad(this, second.description, secondId); InputFixtureFocus.ReportGamepad(this, first.description, firstId); InputSystem.Update();
            Assert.That(input.AssignedGamepad(0), Is.SameAs(first)); Assert.That(input.AssignedGamepad(1), Is.SameAs(second));
            yield return PressKeys(second.buttonWest); input.Capture();
            Assert.That(input.Consume(0).IsHeld(InputButtons.Attack), Is.False);
            Assert.That(input.Consume(1).IsHeld(InputButtons.Attack), Is.True);
            yield return ReleaseKeys(second.buttonWest);
        }
        [UnityTest]
        public IEnumerator TransitionBlocksOnlyItsButtonAndDevice()
        {
            yield return Tap(keyboard.enterKey); yield return Tap(keyboard.sKey); yield return Tap(keyboard.enterKey);
            yield return Tap(keyboard.jKey); yield return PressKeys(keyboard.deleteKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.StageSelect));
            yield return PressKeys(keyboard.enterKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Countdown), "無関係のEnterがP2の保持に塞がれています。");
            yield return PressKeys(keyboard.escapeKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause), "決定保持中でもEscは即有効です。");
            yield return ReleaseKeys(keyboard.deleteKey, keyboard.enterKey, keyboard.escapeKey);

            // A cancel transition must not consume an unrelated confirm observed in the
            // same input update. Exercise the real controls with screen processing stopped.
            Flow.enabled = false; Flow.Runner.enabled = false;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J, Key.Escape));
            yield return WaitFor(() => keyboard.jKey.isPressed && keyboard.escapeKey.isPressed, "同時押下が配送されませんでした。");
            var input = Object.FindAnyObjectByType<InputController>(); input.RefreshMenuReleaseGuard();
            Assert.That(input.ReadMenu(0).Confirm, Is.True); Assert.That(input.EscapePressed, Is.True);
            input.GuardScreenTransition(MenuAction.Cancel); input.Capture(); var battle = input.Consume(0);
            Assert.That(input.EscapePressed, Is.False); Assert.That(input.ReadMenu(0).Confirm, Is.True);
            Assert.That(battle.IsHeld(InputButtons.Attack), Is.True); Assert.That(battle.IsPressed(InputButtons.Attack), Is.True);
            yield return ReleaseKeys(keyboard.jKey, keyboard.escapeKey);
        }
        [UnityTest]
        public IEnumerator HeldPadConfirmDoesNotBlockOtherPadConfirmOrStart()
        {
            var first = InputSystem.AddDevice<Gamepad>(); var second = InputSystem.AddDevice<Gamepad>();
            var input = Object.FindAnyObjectByType<InputController>(); input.AssignGamepad(0, first); input.AssignGamepad(1, second);
            yield return Tap(keyboard.enterKey); yield return Tap(keyboard.sKey); yield return Tap(keyboard.enterKey);
            yield return PressKeys(first.buttonSouth); yield return PressKeys(second.buttonSouth);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.StageSelect));
            yield return ReleaseKeys(second.buttonSouth); yield return Tap(second.buttonSouth);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Countdown), "P1保持が別パッドの新規決定を塞いでいます。");
            yield return PressKeys(second.startButton); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause));
            yield return ReleaseKeys(first.buttonSouth, second.startButton);
        }
        [UnityTest]
        public IEnumerator TitleCpuBattleAdvancesToResultWithAudioAndEffectsEnabled()
        {
            yield return Tap(keyboard.enterKey); yield return Tap(keyboard.enterKey);
            yield return Tap(keyboard.enterKey); yield return Tap(keyboard.enterKey);
            yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "CPU対戦を開始できませんでした。");
            Assert.That(Flow.Audio, Is.Not.Null); Assert.That(Object.FindAnyObjectByType<BattleEffectsView>(), Is.Not.Null);
            float deadline = Time.realtimeSinceStartup + 35;
            while (Flow.Screen != GameScreen.Result && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Result), "CPUが待機相手を倒してリザルトへ進みませんでした。");
            Assert.That(Flow.Runner.Simulation.Match.TickCount, Is.GreaterThan(0));
            Assert.That(Flow.Runner.Simulation.Match.Result, Is.EqualTo(MatchResult.Player2Wins));
        }
        [UnityTest]
        public IEnumerator GamepadsNavigateBothCharacterSlotsAndStartPausesBattle()
        {
            var first = InputSystem.AddDevice<Gamepad>(); var second = InputSystem.AddDevice<Gamepad>();
            var input = Object.FindAnyObjectByType<InputController>(); input.AssignGamepad(0, first); input.AssignGamepad(1, second);
            yield return Tap(first.buttonSouth);
            Set(first.dpad, Vector2.down, queueEventOnly: true); yield return null; yield return null;
            Set(first.dpad, Vector2.zero, queueEventOnly: true); yield return null; yield return null;
            yield return Tap(first.buttonSouth); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.CharacterSelect));
            yield return Tap(second.buttonEast); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.MainMenu));
            yield return Tap(first.buttonSouth); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.CharacterSelect));
            Set(second.dpad, Vector2.right, queueEventOnly: true); yield return null; yield return null;
            Set(second.dpad, Vector2.zero, queueEventOnly: true); yield return null; yield return null;
            Assert.That(Flow.Player2Index, Is.EqualTo(2));
            yield return Tap(first.buttonSouth); yield return Tap(second.buttonSouth); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.StageSelect));
            yield return Tap(first.buttonSouth); yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "パッド操作で試合を開始できませんでした。");
            yield return Tap(second.startButton); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause));
            int tick = Flow.Runner.Simulation.Match.TickCount; yield return new WaitForSecondsRealtime(0.1f);
            Assert.That(Flow.Runner.Simulation.Match.TickCount, Is.EqualTo(tick));
            yield return Tap(second.startButton); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Battle));
        }
        [UnityTest]
        public IEnumerator ResultCanReturnToCharacterSelectionAndTitle()
        {
            yield return SelectLocalBattle(); while (!Flow.Runner.Simulation.Match.IsFinished) Flow.Runner.Simulation.Tick(default, default);
            yield return WaitFor(() => Flow.Screen == GameScreen.Result, "リザルトが表示されませんでした。");
            yield return Tap(keyboard.sKey); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.CharacterSelect)); Assert.That(Flow.Ready(0), Is.False);
            yield return Tap(keyboard.escapeKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.MainMenu));
            yield return Tap(keyboard.escapeKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Title));
        }
        [UnityTest]
        public IEnumerator RingOutResultShowsWinLoseAndReturnsDirectlyToTitle()
        {
            yield return SelectLocalBattle();
            while (!Flow.Runner.Simulation.Match.IsFinished) Flow.Runner.Simulation.Tick(new FrameInput(1), default);
            Assert.That(Flow.Runner.Simulation.Match.EndReason, Is.EqualTo(MatchEndReason.RingOut));
            yield return WaitFor(() => Flow.Screen == GameScreen.Result, "場外のリザルトが表示されませんでした。");
            var pictures = Object.FindObjectsByType<Image>();
            Assert.That(pictures.Single(i => i.name == "Selection Picture 0").sprite.name, Does.StartWith("lose_"));
            Assert.That(pictures.Single(i => i.name == "Selection Picture 1").sprite.name, Does.StartWith("win_"));
            yield return Tap(keyboard.sKey); yield return Tap(keyboard.sKey); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Title)); Assert.That(Flow.Runner.Paused, Is.True);
        }
    }
}

