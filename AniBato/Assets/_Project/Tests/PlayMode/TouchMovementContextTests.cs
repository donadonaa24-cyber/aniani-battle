using System.Collections;
using System.Linq;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class GameFlowSceneTests
    {
        [UnityTest] public IEnumerator TouchDefaultsChangeByScreenAndKeepSharedCustomLayout()
        {
            var view = Flow.TouchControls; view.SetEnabled(true); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.MainMenu));
            Assert.That(view.ActiveMovementMode, Is.EqualTo(TouchMovementMode.DirectionalPad));
            Assert.That(view.Stick.gameObject.activeInHierarchy, Is.False);
            Assert.That(view.Buttons["right"].gameObject.activeInHierarchy, Is.True);
            CaptureScreen("menu_dpad", 720, "touch_pad_context");
            var layout = view.MovementLayout;
            Flow.LayoutEditor.Profile.Set("control.stick", .22f, .24f, 1.2f, .4f); layout.Apply();
            yield return CpuToCountdown(); yield return null;
            Assert.That(view.ActiveMovementMode, Is.EqualTo(TouchMovementMode.Stick), "開幕カウントから戦闘用");
            yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "戦闘開始");
            Assert.That(layout.Current().Scale, Is.EqualTo(1.2f));
            Assert.That(layout.Current().Opacity, Is.EqualTo(.4f));
            Assert.That(view.Buttons["right"].gameObject.activeInHierarchy, Is.False);
            CaptureScreen("battle_stick", 720, "touch_pad_context");
            Flow.Runner.enabled = false;
            var pointer = new PointerEventData(EventSystem.current) { pointerId = 31, position = StickPoint(view.Stick, Vector2.right) };
            view.Stick.OnPointerDown(pointer); yield return null;
            Assert.That(view.Stick.Held, Is.True);
            yield return Tap(keyboard.escapeKey);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause));
            Assert.That(view.ActiveMovementMode, Is.EqualTo(TouchMovementMode.DirectionalPad));
            Assert.That(view.Stick.Held, Is.False);
            Assert.That(view.VirtualPad.leftStick.ReadValue(), Is.EqualTo(Vector2.zero));
            Assert.That(view.VirtualPad.dpad.ReadValue(), Is.EqualTo(Vector2.zero));
            Assert.That(layout.Current().Scale, Is.EqualTo(1.2f));
        }

        [UnityTest] public IEnumerator TouchContextSettingsAreIndependentPersistAndReleaseHeldInput()
        {
            var view = Flow.TouchControls; view.SetEnabled(true);
            yield return ChooseMain(3); yield return SettingsItem(13); yield return Tap(keyboard.enterKey);
            Assert.That(view.MovementSettings.Combat, Is.EqualTo(TouchMovementMode.DirectionalPad));
            Assert.That(view.MovementSettings.Menu, Is.EqualTo(TouchMovementMode.DirectionalPad));
            yield return SettingsItem(14);
            var right = view.Buttons["right"];
            right.OnPointerDown(new PointerEventData(EventSystem.current) { pointerId = 20 });
            // The held right arrow itself activates this settings row, then must clear.
            yield return WaitFor(() => view.ActiveMovementMode == TouchMovementMode.Stick, "設定画面の十字キーで方式を切替");
            yield return null;
            Assert.That(view.ActiveMovementMode, Is.EqualTo(TouchMovementMode.Stick));
            Assert.That(right.Held, Is.False);
            Assert.That(view.VirtualPad.dpad.ReadValue(), Is.EqualTo(Vector2.zero));
            var reloaded = TouchMovementSettings.Load();
            Assert.That(reloaded.Combat, Is.EqualTo(TouchMovementMode.DirectionalPad));
            Assert.That(reloaded.Menu, Is.EqualTo(TouchMovementMode.Stick));
            CaptureScreen("separate_settings", 720, "touch_pad_context");
            yield return SettingsItem(15); yield return Tap(keyboard.enterKey);
            yield return CpuToCountdown(); yield return null;
            Assert.That(view.ActiveMovementMode, Is.EqualTo(TouchMovementMode.DirectionalPad));
            Assert.That(view.Buttons["right"].gameObject.activeInHierarchy, Is.True);
            Assert.That(view.VirtualPad.leftStick.ReadValue(), Is.EqualTo(Vector2.zero));
        }
    }
}
