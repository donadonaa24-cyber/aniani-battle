using System.Collections;
using System.IO;
using System.Linq;
using AniBato.Battle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using InputKey = UnityEngine.InputSystem.Key;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest] public IEnumerator NineFightersRenderBoundedDashTrailsAndOffIsTheExactOriginalBody()
        {
            runner.SetMotionTrails(true, false);
            for (int index = 0; index < runner.Setup.Characters.Length; index++)
            {
                runner.SetPaused(false);
                runner.BeginMatch(runner.Setup.Character(index), runner.Setup.Character((index + 1) % 9), runner.CurrentStage, false);
                view.RenderFrame(); Assert.That(view.AfterimageCount, Is.Zero, "Restart clears the previous character");
                yield return Key(InputKey.F, true);
                for (int i = 0; i < 6; i++) Tick();
                Assert.That(view.AfterimageCount, Is.InRange(1, 2), "Dash trail for " + runner.Character(0).name);
                runner.SetPaused(true); view.RenderFrame();
                var position = body.transform.position; var rotation = body.transform.rotation;
                var sprite = body.sprite; var color = body.color;
                var fighter = runner.Simulation.P1; var simPosition = fighter.Position; var velocity = fighter.Velocity;
                int tick = runner.Simulation.Match.TickCount, stamina = fighter.StaminaUnits, hearts = fighter.Hearts;
                if (index == 2) CaptureTrailComparison("dash_on");
                runner.SetMotionTrails(false, false); view.RenderFrame();
                Assert.That(view.AfterimageCount, Is.Zero);
                Assert.That(body.sprite, Is.SameAs(sprite)); Assert.That(body.color, Is.EqualTo(color));
                Assert.That(body.transform.position, Is.EqualTo(position)); Assert.That(body.transform.rotation, Is.EqualTo(rotation));
                Assert.That(fighter.Position, Is.EqualTo(simPosition)); Assert.That(fighter.Velocity, Is.EqualTo(velocity));
                Assert.That(fighter.StaminaUnits, Is.EqualTo(stamina)); Assert.That(fighter.Hearts, Is.EqualTo(hearts));
                Assert.That(runner.Simulation.Match.TickCount, Is.EqualTo(tick));
                if (index == 2) CaptureTrailComparison("dash_off");
                runner.SetMotionTrails(true, false); yield return Key(InputKey.F, false);
            }
        }
        [UnityTest] public IEnumerator NormalStrikeLeavesAShortTrailAndPauseRestartAndDisableAreSafe()
        {
            runner.SetMotionTrails(true, false); runner.SetPaused(false);
            runner.BeginMatch(runner.Setup.Character(2), runner.Setup.Character(0), runner.CurrentStage, false); view.RenderFrame();
            yield return Key(InputKey.J, true); Tick(); yield return Key(InputKey.J, false);
            bool observed = false;
            for (int i = 0; i < 55 && !observed; i++)
            {
                Tick(); if (view.AfterimageCount == 0) continue;
                observed = true; CaptureTrailComparison("attack_on");
                var images = view.GetComponentsInChildren<SpriteRenderer>().Where(r => r.name.StartsWith("Afterimage ") && r.enabled).ToArray();
                var colors = images.Select(r => r.color).ToArray();
                runner.SetPaused(true); for (int render = 0; render < 144; render++) view.RenderFrame();
                CollectionAssert.AreEqual(colors, images.Select(r => r.color).ToArray(), "Pause does not age afterimages");
                runner.SetMotionTrails(false, false); view.RenderFrame(); CaptureTrailComparison("attack_off");
                Assert.That(view.AfterimageCount, Is.Zero);
            }
            Assert.That(observed, Is.True, "The actual normal attack must emit a trail");
            runner.RestartMatch(); view.RenderFrame(); Assert.That(view.AfterimageCount, Is.Zero);
            view.enabled = false;
            Assert.That(view.GetComponentsInChildren<SpriteRenderer>().Where(r => r.name.StartsWith("Afterimage ")).All(r => !r.enabled), Is.True);
        }
        private void CaptureTrailComparison(string name)
        {
            var camera = Camera.main; var target = camera.targetTexture; var active = RenderTexture.active;
            var position = camera.transform.position; float size = camera.orthographicSize;
            int mask = camera.cullingMask; var clear = camera.backgroundColor; var flags = camera.clearFlags;
            var renderers = view.GetComponentsInChildren<SpriteRenderer>().Where(r => r == body || r.name.StartsWith("Afterimage ")).ToArray();
            var layers = renderers.Select(r => r.gameObject.layer).ToArray();
            var texture = RenderTexture.GetTemporary(768, 768, 24); var image = new Texture2D(768, 768, TextureFormat.RGBA32, false);
            try
            {
                foreach (var renderer in renderers) renderer.gameObject.layer = 31;
                camera.cullingMask = 1 << 31; camera.orthographicSize = 2.15f;
                camera.transform.position = body.bounds.center + new Vector3(0, 0, -10);
                camera.backgroundColor = new Color(.055f, .08f, .13f); camera.clearFlags = CameraClearFlags.SolidColor;
                camera.targetTexture = texture; camera.Render(); RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, 768, 768), 0, 0); image.Apply();
                string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../docs/screenshots/motion_trails"));
                Directory.CreateDirectory(folder); CaptureFile.Write(Path.Combine(folder, name + ".png"), image.EncodeToPNG());
            }
            finally
            {
                for (int i = 0; i < renderers.Length; i++) renderers[i].gameObject.layer = layers[i];
                camera.cullingMask = mask; camera.orthographicSize = size; camera.transform.position = position;
                camera.backgroundColor = clear; camera.clearFlags = flags; camera.targetTexture = target;
                RenderTexture.active = active; RenderTexture.ReleaseTemporary(texture); Object.Destroy(image);
            }
        }
    }
}
