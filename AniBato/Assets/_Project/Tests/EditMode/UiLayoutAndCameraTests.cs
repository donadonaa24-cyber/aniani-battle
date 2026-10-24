using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace AniBato.Tests
{
    public sealed class UiLayoutAndCameraTests
    {
        [Test] public void LayoutSanitizesMalformedDataAndCloneDoesNotShareEdits()
        {
            Assert.That(UiLayoutProfile.Decode("invalid").Entries, Is.Empty);
            var profile = new UiLayoutProfile();
            profile.Set("control.attack", -2, 3, 9, -1);
            var e = profile.Find("control.attack");
            Assert.That(e.X, Is.Zero); Assert.That(e.Y, Is.EqualTo(1));
            Assert.That(e.Scale, Is.EqualTo(2)); Assert.That(e.Opacity, Is.EqualTo(.15f));
            var copy = profile.Clone(); copy.Set(e.Id, .3f, .4f, 1, .8f);
            Assert.That(profile.Find(e.Id).X, Is.Zero);
            profile.Set("hud.p1", float.NaN, float.PositiveInfinity, float.NaN, float.NaN);
            Assert.That(profile.Find("hud.p1").Scale, Is.EqualTo(1));
            Assert.That(profile.Find("hud.p1").X, Is.EqualTo(.5f));
        }
        [Test] public void LayoutKeepsWholeElementInsideParentEvenAfterAspectChange()
        {
            string old = PlayerPrefs.HasKey(UiLayoutProfile.Storage) ? PlayerPrefs.GetString(UiLayoutProfile.Storage) : null;
            var root = new GameObject("Layout test", typeof(RectTransform));
            try
            {
                var profile = new UiLayoutProfile(); profile.Set("test", 1, 0, 2, .4f); profile.Save();
                var parent = (RectTransform)root.transform; parent.sizeDelta = new Vector2(960, 540);
                var child = new GameObject("Movable", typeof(RectTransform)); child.transform.SetParent(parent, false);
                var rect = (RectTransform)child.transform; rect.sizeDelta = new Vector2(200, 120);
                var target = child.AddComponent<UiLayoutTarget>(); target.Initialize("test", "Test", 1);
                foreach (var size in new[] {new Vector2(960, 540), new Vector2(270, 480)})
                {
                    parent.sizeDelta = size; target.Apply(); var corners = new Vector3[4]; rect.GetWorldCorners(corners);
                    foreach (var corner in corners)
                    {
                        var p = parent.InverseTransformPoint(corner);
                        Assert.That(p.x, Is.InRange(parent.rect.xMin - .001f, parent.rect.xMax + .001f));
                        Assert.That(p.y, Is.InRange(parent.rect.yMin - .001f, parent.rect.yMax + .001f));
                    }
                    Assert.That(child.GetComponent<CanvasGroup>().alpha, Is.EqualTo(.4f));
                }
            }
            finally { Object.DestroyImmediate(root); if (old == null) PlayerPrefs.DeleteKey(UiLayoutProfile.Storage); else PlayerPrefs.SetString(UiLayoutProfile.Storage, old); }
        }
        [Test] public void TeleportCameraAnticipatesGraduallyHoldsAndFreezesWithoutDestinationSnap()
        {
            var cue = new TeleportCameraCue(); var start = new CameraFrame(new BattleVector(0, 2), 6, default);
            var full = new CameraFrame(new BattleVector(0, 4), 14, default);
            var frame = cue.Advance(true, 0, start, full, .4f, .5f);
            Assert.That(frame.Size, Is.EqualTo(6));
            float previous = frame.Size;
            for (int i = 0; i < 24; i++)
            {
                frame = cue.Advance(true, 1f/60, frame, full, .4f, .5f);
                Assert.That(frame.Size - previous, Is.InRange(0, .51f)); previous = frame.Size;
            }
            Assert.That(frame.Size, Is.EqualTo(14).Within(.001));
            frame = cue.Advance(false, 0, frame, full, .4f, .5f); Assert.That(frame.Size, Is.EqualTo(14));
            cue.Advance(false, .3f, frame, full, .4f, .5f); Assert.That(cue.Active, Is.True);
            cue.Advance(true, 1f/60, frame, full, .4f, .5f); Assert.That(cue.Active, Is.True, "連続発動は広角を保持");
            cue.Advance(false, .6f, frame, full, .4f, .5f); Assert.That(cue.Active, Is.False);
            cue.Reset(); Assert.That(cue.Active, Is.False);
        }
    }
}
