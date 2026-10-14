using System;
using AniBato.Presentation;
using NUnit.Framework;
namespace AniBato.Tests
{
    public sealed class PaperTravelLeanTests
    {
        [TestCase(15, false)] [TestCase(15, true)] [TestCase(45, false)] [TestCase(45, true)]
        public void TravelAlwaysLeansIntoDirectionAndKeepsSoleAboveFloor(float degrees, bool flip)
        {
            var result = PaperTravelLean.Evaluate(degrees, flip, -.4f, .7f);
            Assert.That(result.Degrees, Is.EqualTo(flip ? degrees : -degrees));
            double sin = Math.Sin(result.Degrees * Math.PI / 180);
            float left = flip ? -.7f : -.4f, right = flip ? .4f : .7f;
            Assert.That(result.Lift + Math.Min(left * sin, right * sin), Is.EqualTo(0).Within(.000001));
            var mirrored = PaperTravelLean.Evaluate(degrees, !flip, -.4f, .7f);
            Assert.That(mirrored.Lift, Is.EqualTo(result.Lift).Within(.000001));
        }
        [Test]
        public void InvalidTravelIsRejected()
        {
            Assert.Throws<ArgumentException>(() => PaperTravelLean.Evaluate(float.NaN, false, 0, 1));
            Assert.Throws<ArgumentException>(() => PaperTravelLean.Evaluate(90, false, 0, 1));
            Assert.Throws<ArgumentException>(() => PaperTravelLean.Evaluate(15, false, 1, 0));
        }
    }
}
