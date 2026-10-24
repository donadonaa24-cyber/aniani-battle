using System.Collections;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class GameFlowSceneTests
    {
        [UnityTest] public IEnumerator LayoutEditorDragsWithoutGameplaySavesCancelsAndResets()
        {
            string old = PlayerPrefs.HasKey(UiLayoutProfile.Storage) ? PlayerPrefs.GetString(UiLayoutProfile.Storage) : null;
            try
            {
                yield return ChooseMain(3); yield return SettingsItem(12); yield return Tap(keyboard.enterKey);
                Assert.That(Flow.Screen, Is.EqualTo(GameScreen.LayoutEditor));
                var editor = Flow.LayoutEditor; Assert.That(editor.IsEditing, Is.True); Assert.That(Flow.Runner.Paused, Is.True);
                Assert.That(editor.Targets.Count, Is.EqualTo(13));
                var target = editor.Targets.Single(t => t.Id == "control.attack"); editor.Select(target);
                int tick = Flow.Runner.Simulation.Match.TickCount; var pos = Flow.Runner.Simulation.P1.Position;
                var start = RectTransformUtility.WorldToScreenPoint(null, target.transform.position);
                var pointer = new PointerEventData(EventSystem.current) { pointerId = 7, position = start };
                Flow.TouchControls.Buttons["attack"].OnPointerDown(pointer); target.OnPointerDown(pointer);
                pointer.position += new Vector2(-90, 80); target.OnDrag(pointer);
                editor.SetSelectedValues(1.25f, .42f); yield return null;
                Assert.That(Flow.TouchControls.Buttons["attack"].Held, Is.False);
                Assert.That(Flow.Runner.Simulation.Match.TickCount, Is.EqualTo(tick)); Assert.That(Flow.Runner.Simulation.P1.Position, Is.EqualTo(pos));
                Assert.That(Vector2.Distance(start, RectTransformUtility.WorldToScreenPoint(null, target.transform.position)), Is.GreaterThan(20));
                CaptureScreen("layout_editor", 720, "ui_motion_camera");
                editor.Save(); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Settings));
                var saved = UiLayoutProfile.Load().Find(target.Id); Assert.That(saved.Scale, Is.EqualTo(1.25f)); Assert.That(saved.Opacity, Is.EqualTo(.42f));
                yield return Tap(keyboard.enterKey); Assert.That(editor.IsEditing, Is.True);
                editor.Select(target); editor.SetSelectedValues(.6f, .2f); editor.Cancel();
                Assert.That(target.Current().Scale, Is.EqualTo(1.25f));
                yield return Tap(keyboard.enterKey); editor.ResetLayout();
                Assert.That(target.Current().Scale, Is.EqualTo(1));
                var hud = editor.Targets.Single(t => t.Id == "hud.p1"); Assert.That(hud.Rect.localScale.x, Is.EqualTo(2f/3).Within(.0001));
                editor.Cancel(); Assert.That(target.Current().Scale, Is.EqualTo(1.25f));
            }
            finally { if (old == null) PlayerPrefs.DeleteKey(UiLayoutProfile.Storage); else PlayerPrefs.SetString(UiLayoutProfile.Storage, old); PlayerPrefs.Save(); }
        }
        [UnityTest] public IEnumerator StickDeadZoneDragAndForeignFingerDoNotStick()
        {
            Flow.TouchControls.SetMovementMode(false, TouchMovementMode.Stick);
            Flow.TouchControls.SetEnabled(true); yield return null;
            var stick = Flow.TouchControls.Stick;
            var p = new PointerEventData(EventSystem.current) {pointerId=3,position=StickPoint(stick,Vector2.zero)};
            stick.OnPointerDown(p); Assert.That(stick.Value, Is.EqualTo(Vector2.zero));
            foreach (var direction in new[] {Vector2.left,Vector2.up,Vector2.right,Vector2.down,new Vector2(1,1).normalized})
            {
                p.position=StickPoint(stick,direction);stick.OnDrag(p);yield return null;
                Assert.That(Vector2.Distance(stick.Value,direction),Is.LessThan(.01f));
                Assert.That(Vector2.Distance(Flow.TouchControls.VirtualPad.leftStick.ReadValue(),direction),Is.LessThan(.01f));
            }
            stick.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=4});Assert.That(stick.Held,Is.True);
            Flow.TouchControls.ReleaseAll();yield return null;
            Assert.That(stick.Held,Is.False);Assert.That(Flow.TouchControls.VirtualPad.leftStick.ReadValue(),Is.EqualTo(Vector2.zero));
        }
    }
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest] public IEnumerator TeleportChargeWidensCameraBeforeMovingAndArrivalDoesNotJump()
        {
            foreach (string name in new[] {"湊","シロ"})
            {
                runner.BeginMatch(runner.Setup.Characters.Single(c=>c.Movement.Name==name),runner.Setup.Character(0),runner.CurrentStage,false);
                for(int i=0;i<3;i++)runner.Simulation.Tick(default,default);
                runner.RenderState.Reset(runner.Simulation);
                var camera=Camera.main;var cameraView=camera.GetComponent<CameraController>();
                cameraView.enabled=false;cameraView.enabled=true;yield return null;
                float before=camera.orthographicSize;var start=runner.Simulation.P1.Position;
                runner.Simulation.Tick(new FrameInput(1,1,InputButtons.Special,InputButtons.Special),default);
                Assert.That(runner.Simulation.P1.Attack.Definition.Recovery.ChargedTeleport,Is.True);
                float previous=before;
                for(int i=0;i<30;i++)
                {
                    runner.Simulation.Tick(new FrameInput(1,1,InputButtons.Special),default);runner.RenderState.Reset(runner.Simulation);
                    yield return new WaitForSecondsRealtime(1f/60);
                    Assert.That(camera.orthographicSize-previous,Is.LessThan(1.5f),"発動直後にも急拡大しない");previous=camera.orthographicSize;
                }
                Assert.That(camera.orthographicSize,Is.GreaterThan(before+.5f),"瞬間移動する前から先行して広げる");
                float prior=camera.orthographicSize;
                for(int i=0;i<8;i++){runner.Simulation.Tick(default,default);runner.RenderState.Reset(runner.Simulation);yield return null;}
                Assert.That(runner.Simulation.P1.Position.X,Is.GreaterThan(start.X+2));
                Assert.That(Mathf.Abs(camera.orthographicSize-prior),Is.LessThan(.8f),"到着座標による一瞬の拡大を起こさない");
            }
        }
    }
}
