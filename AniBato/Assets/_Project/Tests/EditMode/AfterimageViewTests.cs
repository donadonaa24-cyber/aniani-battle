using System.Linq;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;

namespace AniBato.Tests
{
    public sealed class AfterimageViewTests
    {
        private GameObject fighter;
        private SpriteRenderer body;
        private Sprite sprite;
        private FighterAfterimageView trails;
        private AfterimageSettings settings;
        [SetUp] public void Open()
        {
            fighter = new GameObject("Trail test fighter");
            var obj = new GameObject("Body"); obj.transform.SetParent(fighter.transform, false);
            body = obj.AddComponent<SpriteRenderer>();
            sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
            body.sprite = sprite; body.sortingOrder = 5;
            settings = AfterimageSettings.Default; trails = new FighterAfterimageView(fighter.transform);
        }
        [TearDown] public void Close() { trails.Dispose(); Object.DestroyImmediate(fighter); Object.DestroyImmediate(sprite); }
        private void Render(double time, bool emitting = true, bool frozen = false, bool enabled = true, bool reset = false) =>
            trails.Render(body, time, settings, enabled, frozen, emitting, reset);
        private SpriteRenderer[] Visible() => fighter.GetComponentsInChildren<SpriteRenderer>().Where(r => r != body && r.enabled).ToArray();
        [Test] public void PoolReusesTwoSharedSpritesAndOldPositionsThenFadesOut()
        {
            Render(0);
            for (int i = 1; i <= 12; i++) { fighter.transform.position = Vector3.right * i * .18f; Render(i / 60d); }
            Assert.That(trails.VisibleCount, Is.EqualTo(2));
            Assert.That(fighter.GetComponentsInChildren<SpriteRenderer>().Length, Is.EqualTo(3));
            foreach (var image in Visible())
            {
                Assert.That(image.sprite, Is.SameAs(body.sprite)); Assert.That(image.sharedMaterial, Is.SameAs(body.sharedMaterial));
                Assert.That(image.sortingOrder, Is.LessThan(body.sortingOrder));
                Assert.That(image.color.a, Is.InRange(0.001f, .18f));
                Assert.That(image.transform.position.x, Is.LessThan(body.transform.position.x));
            }
            var poses = Visible().Select(r => r.transform.position).ToArray();
            fighter.transform.position += Vector3.right * .3f; Render(12 / 60d, false);
            CollectionAssert.AreEqual(poses, Visible().Select(r => r.transform.position).ToArray(), "Parent motion must not drag old images");
            Render(.5, false); Assert.That(trails.VisibleCount, Is.Zero);
        }
        [Test] public void FreezeHoldsFadeAndDisableImmediatelyRestoresOnlyTheBody()
        {
            Render(0); fighter.transform.position = Vector3.right; Render(.05);
            var image = Visible().Single(); float alpha = image.color.a; var position = image.transform.position;
            for (int i = 1; i < 20; i++) Render(.05 + i / 60d, frozen: true);
            Assert.That(image.color.a, Is.EqualTo(alpha)); Assert.That(image.transform.position, Is.EqualTo(position));
            var bodyPosition = body.transform.position; var color = body.color;
            Render(1, frozen: true, enabled: false);
            Assert.That(trails.VisibleCount, Is.Zero); Assert.That(body.sprite, Is.SameAs(sprite));
            Assert.That(body.transform.position, Is.EqualTo(bodyPosition)); Assert.That(body.color, Is.EqualTo(color));
            Assert.That(fighter.GetComponentsInChildren<Collider2D>().Length, Is.Zero);
        }
        [Test] public void SubframeRenderingStillEmitsAtHighRefreshRatesAndNeverWhileIdle()
        {
            Render(0);
            for (int i = 1; i <= 18; i++) { fighter.transform.position = Vector3.right * i * .02f; Render(i / 240d); }
            Assert.That(trails.VisibleCount, Is.GreaterThan(0)); Assert.That(trails.VisibleCount, Is.LessThanOrEqualTo(2));
            trails.Clear(); Render(0, false);
            for (int i = 1; i <= 20; i++) { fighter.transform.position += Vector3.right; Render(i / 60d, false); }
            Assert.That(trails.VisibleCount, Is.Zero);
        }
        [Test] public void DiscontinuityClockResetAndExplicitResetNeverLeaveAConnectingTrail()
        {
            Render(0); fighter.transform.position = Vector3.right; Render(.05); Assert.That(trails.VisibleCount, Is.EqualTo(1));
            fighter.transform.position += Vector3.right * 10; Render(.1); Assert.That(trails.VisibleCount, Is.Zero);
            fighter.transform.position += Vector3.right; Render(.15); Assert.That(trails.VisibleCount, Is.EqualTo(1));
            Render(0); Assert.That(trails.VisibleCount, Is.Zero);
            fighter.transform.position += Vector3.right; Render(.05); Assert.That(trails.VisibleCount, Is.EqualTo(1));
            Render(.1, reset: true); Assert.That(trails.VisibleCount, Is.Zero);
        }
    }
}
