using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AniBato.Tests
{
    public sealed partial class GameFlowSceneTests
    {
        private static Vector2 TouchPoint(MobileControlButton button) => RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
        private static Vector2 StickPoint(MobileStick stick, Vector2 direction) => RectTransformUtility.WorldToScreenPoint(null,
            stick.transform.TransformPoint(direction * 60));
        private IEnumerator MobileDirection(Vector2 direction, Touchscreen screen)
        {
            var stick = Flow.TouchControls.Stick; var point = StickPoint(stick, direction);
            BeginTouch(1, point, queueEventOnly: true, screen: screen);
            yield return WaitFor(() => stick.Held && stick.Value.sqrMagnitude > .8f, "スティック方向が届かない");
            yield return null;
            EndTouch(1, point, queueEventOnly: true, screen: screen);
            yield return WaitFor(() => !stick.Held, "スティック解放"); yield return null;
        }
        private IEnumerator MobileTap(string key, Touchscreen screen, int id = 1)
        {
            var button = Flow.TouchControls.Buttons[key]; var point = TouchPoint(button);
            BeginTouch(id, point, queueEventOnly: true, screen: screen);
            yield return WaitFor(() => button.Held, "UIへの指の押下が届きません: " + key);
            EndTouch(id, point, queueEventOnly: true, screen: screen);
            yield return WaitFor(() => !button.Held, "UIへの指の解放が届きません: " + key);
            yield return null;
        }
        [UnityTest]
        public IEnumerator MobileMenuTouchConfirmsNavigatesAndReturnsWithoutBattleButtons()
        {
            var view = Flow.TouchControls;
            Assert.That(view.Visible, Is.False, "PCでは自動表示しない");
            var screen = InputSystem.AddDevice<Touchscreen>(); view.SetEnabled(true); yield return null;
            Assert.That(view.Visible, Is.True); Assert.That(view.VirtualPad, Is.SameAs(Flow.Input.AssignedGamepad(0)));
            Assert.That(view.Buttons["attack"].gameObject.activeSelf, Is.False);
            yield return MobileTap("jump", screen);
            yield return WaitFor(() => Flow.Screen == GameScreen.MainMenu, "タッチ決定からメニューへ");
            int first = Flow.MenuIndex;
            yield return MobileDirection(Vector2.right, screen);
            Assert.That(Flow.MenuIndex, Is.Not.EqualTo(first), "横並びメニューの左右操作");
            yield return MobileDirection(Vector2.left, screen); yield return MobileTap("jump", screen);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.CharacterSelect));
            // A browser may also deliver a mouse event for a touch. It must not select the card beneath a pad button.
            var mouse = InputSystem.AddDevice<Mouse>(); var up = view.Stick;
            var covered = Object.FindObjectsByType<Image>().First(i => i.name.StartsWith("Roster Card "));
            int originalSibling = up.transform.GetSiblingIndex(); up.transform.SetAsLastSibling();
            var originalPosition = up.transform.position; up.transform.position = covered.transform.position;
            var point = StickPoint(up, Vector2.zero);
            Assert.That(RectTransformUtility.RectangleContainsScreenPoint(covered.rectTransform, point), Is.True, "背後のカードと重なる位置で検査");
            Canvas.ForceUpdateCanvases();
            Set(mouse.position, point, queueEventOnly: true);
            yield return WaitFor(() => mouse.position.ReadValue() == point, "追加したマウスの位置イベント");
            yield return null;
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
            Assert.That(hits.Count, Is.GreaterThan(0)); Assert.That(hits[0].gameObject, Is.SameAs(up.gameObject), "ボタンが手前の入力対象");
            Press(mouse.leftButton, queueEventOnly: true);
            yield return WaitFor(() => up.Held, "マウス由来のタッチパッド押下"); yield return null;
            Assert.That(Flow.Selection.Picks(0).Count, Is.Zero, "タッチ方向の裏にいるキャラを勝手に選ばない");
            Release(mouse.leftButton, queueEventOnly: true); yield return WaitFor(() => !up.Held, "方向の解放"); yield return null;
            up.transform.position = originalPosition; up.transform.SetSiblingIndex(originalSibling);
            yield return MobileTap("jump", screen); Assert.That(Flow.Selection.Picks(0).Count, Is.EqualTo(1));
            yield return MobileTap("special", screen); yield return MobileTap("special", screen);
            yield return MobileTap("special", screen);
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Title), "タッチ戻る");
        }
        [UnityTest]
        public IEnumerator MobileTwoRealTouchContactsMoveAndAttackIndependentlyThenFocusReleases()
        {
            yield return CpuToCountdown();
            yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "開始待ち");
            Flow.enabled = false; Flow.Runner.enabled = false;
            var input = Flow.Input; var view = Flow.TouchControls;
            var screen = InputSystem.AddDevice<Touchscreen>(); view.SetEnabled(true); yield return null;
            var right = view.Stick; var attack = view.Buttons["attack"];
            BeginTouch(1, StickPoint(right, Vector2.right), queueEventOnly: true, screen: screen);
            BeginTouch(2, TouchPoint(attack), queueEventOnly: true, screen: screen);
            yield return WaitFor(() => right.Held && attack.Held && view.VirtualPad.buttonWest.isPressed && view.VirtualPad.leftStick.x.ReadValue() > .9f, "方向と攻撃の同時押し");
            input.Capture(); var both = input.Consume(0);
            Assert.That(both.X, Is.EqualTo(1)); Assert.That(both.IsHeld(InputButtons.Attack), Is.True);
            Assert.That(input.Consume(1).IsHeld(InputButtons.Attack), Is.False, "P2へ漏れない");
            EndTouch(1, StickPoint(right, Vector2.right), queueEventOnly: true, screen: screen);
            yield return WaitFor(() => !right.Held && view.VirtualPad.leftStick.ReadValue() == Vector2.zero, "方向だけ離す");
            input.Capture(); var held = input.Consume(0);
            Assert.That(held.X, Is.Zero); Assert.That(held.IsHeld(InputButtons.Attack), Is.True);
            view.SendMessage("OnApplicationFocus", false); yield return null;
            input.Capture(); Assert.That(input.Consume(0).IsHeld(InputButtons.Attack), Is.False);
            Assert.That(attack.Held, Is.False);
            EndTouch(2, TouchPoint(attack), queueEventOnly: true, screen: screen); yield return null;
        }
        [UnityTest]
        public IEnumerator MobileHoldChargesUntilReleaseAndSameButtonCountsBothFingers()
        {
            yield return PracticeToBattle();
            var view = Flow.TouchControls; view.SetEnabled(true); yield return null;
            var attack = view.Buttons["attack"];
            var first = new PointerEventData(EventSystem.current) { pointerId = 101 };
            var second = new PointerEventData(EventSystem.current) { pointerId = 102 };
            attack.OnPointerDown(first); attack.OnPointerDown(second);
            yield return WaitFor(() => Flow.Runner.Simulation.P1.ChargeFrames >= 65, "通常の長押し溜めが届かない");
            attack.OnPointerUp(first); yield return null;
            Assert.That(attack.Held, Is.True); Assert.That(view.VirtualPad.buttonWest.isPressed, Is.True);
            int charge = Flow.Runner.Simulation.P1.ChargeFrames;
            attack.OnPointerUp(second);
            yield return WaitFor(() => Flow.Runner.Simulation.P1.Attack != null, "最後の指の解放で発動");
            Assert.That(Flow.Runner.Simulation.P1.Attack.ChargeFrames, Is.GreaterThanOrEqualTo(charge));
            Assert.That(attack.Held, Is.False);
        }
        [UnityTest]
        public IEnumerator MobileSafeAreaRemappingAndPhysicalPadsSurviveToggle()
        {
            yield return CpuToCountdown(); Flow.enabled = false; Flow.Runner.enabled = false;
            var p1 = InputSystem.AddDevice<Gamepad>(); var p2 = InputSystem.AddDevice<Gamepad>();
            Flow.Input.AssignGamepad(0, p1); Flow.Input.AssignGamepad(1, p2);
            var view = Flow.TouchControls; view.SetEnabled(true); yield return null;
            Assert.That(Flow.Input.AssignedGamepad(1), Is.SameAs(p2));
            Flow.Input.Configuration.Profile(0, true).Rebind(ControlAction.Attack, 7); yield return null;
            var attack = view.Buttons["attack"];
            attack.OnPointerDown(new PointerEventData(EventSystem.current) { pointerId = 1 });
            yield return WaitFor(() => view.VirtualPad.buttonNorth.isPressed, "リマップ後の通常ボタン");
            Assert.That(view.VirtualPad.buttonWest.isPressed, Is.False);
            var safe = attack.transform.parent.GetComponent<RectTransform>();
            view.ApplySafeArea(new Rect(80, 20, 1800, 1000), 1920, 1080);
            Assert.That(safe.anchorMin.x, Is.EqualTo(80f / 1920).Within(.0001));
            Assert.That(safe.anchorMax.y, Is.EqualTo(1020f / 1080).Within(.0001));
            Assert.That(attack.GetComponent<Image>().color.a, Is.LessThan(.7f));
            view.SetEnabled(false); yield return null;
            Assert.That(attack.Held, Is.False); Assert.That(view.VirtualPad, Is.Null);
            Assert.That(Flow.Input.AssignedGamepad(0), Is.SameAs(p1)); Assert.That(Flow.Input.AssignedGamepad(1), Is.SameAs(p2));
            Assert.That(view.Stick.GetComponent<Image>().color.a, Is.LessThan(.3f));
        }
    }
}
