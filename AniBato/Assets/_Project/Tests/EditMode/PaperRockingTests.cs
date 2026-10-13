using System;
using AniBato.Presentation;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class PaperRockingTests
    {
        private static PaperRockingSettings Settings => new PaperRockingSettings
        { IdleDegrees = .75f, WalkDegrees = 5, WalkBob = .025f, IdleFps = 4, WalkFps = 8, FootMinX = -.45f, FootMaxX = .62f };
        [TestCase(0, -5)] [TestCase(8, -2)] [TestCase(15, 0)] [TestCase(23, 2)]
        [TestCase(30, 5)] [TestCase(38, 2)] [TestCase(45, 0)] [TestCase(53, -2)]
        public void WalkUsesAllEightSteppedAnglesAndRepeatsInOneSecond(int age, float expected)
        {
            var first = PaperRockingMotion.Evaluate(Settings, age, true, false);
            var repeat = PaperRockingMotion.Evaluate(Settings, age + 60, true, false);
            Assert.That(first.Degrees, Is.EqualTo(expected)); Assert.That(repeat.Degrees, Is.EqualTo(expected));
            Assert.That(repeat.Lift, Is.EqualTo(first.Lift)); Assert.That(repeat.Phase, Is.EqualTo(first.Phase));
        }
        [Test]
        public void PoseHoldsBetweenUpdatesAndRepeatingRenderDoesNotAdvance()
        {
            var first = PaperRockingMotion.Evaluate(Settings, 0, true, false);
            for (int i = 0; i < 8; i++) Assert.That(PaperRockingMotion.Evaluate(Settings, i, true, false).Degrees, Is.EqualTo(first.Degrees));
            for (int i = 0; i < 144; i++) Assert.That(PaperRockingMotion.Evaluate(Settings, 8, true, false).Degrees, Is.EqualTo(-2));
        }
        [TestCase(false)] [TestCase(true)]
        public void BothDirectionsKeepTheLowerSoleAtOrAboveTheGround(bool flip)
        {
            var settings = Settings;
            for (int tick = 0; tick < 60; tick++)
            {
                var sample = PaperRockingMotion.Evaluate(settings, tick, true, flip);
                double sin = Math.Sin(sample.Degrees * Math.PI / 180);
                float minX = flip ? -settings.FootMaxX : settings.FootMinX;
                float maxX = flip ? -settings.FootMinX : settings.FootMaxX;
                Assert.That(sample.Lift + Math.Min(minX * sin, maxX * sin), Is.GreaterThanOrEqualTo(-.000001));
                Assert.That(sample.Lift, Is.LessThan(.09f));
                var mirrored = PaperRockingMotion.Evaluate(settings, tick, true, !flip);
                Assert.That(mirrored.Degrees, Is.EqualTo(-sample.Degrees)); Assert.That(mirrored.Lift, Is.EqualTo(sample.Lift).Within(.000001));
            }
        }
        [Test]
        public void IdleIsGentleAndDoesNotUseTheWalkBob()
        {
            float[] angles = { 0, .75f, 0, -.75f };
            for (int i = 0; i < 4; i++) Assert.That(PaperRockingMotion.Evaluate(Settings, i * 15, false, false).Degrees, Is.EqualTo(angles[i]));
            Assert.That(PaperRockingMotion.Evaluate(Settings, 0, false, false).Lift, Is.Zero);
            Assert.That(PaperRockingMotion.Evaluate(Settings, -10, false, false).Degrees, Is.Zero);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void InvalidMotionDataIsRejected(int field)
        {
            var settings = Settings;
            if (field == 0) settings.WalkDegrees = float.NaN;
            if (field == 1) settings.WalkFps = 0;
            if (field == 2) settings.FootMinX = settings.FootMaxX + 1;
            if (field == 3) settings.IdleDegrees = 90;
            Assert.Throws<ArgumentException>(() => settings.Validate());
        }
    }
}
