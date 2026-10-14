using AniBato.Presentation;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class PaperPerformanceTests
    {
        [TestCase(false, 8, -.06f, -15, .10f)]
        [TestCase(true, -8, .06f, 15, -.10f)]
        public void AttackPullsBackThenSnapsForwardAndHoldsTheStrike(bool flip, float pullAngle, float pullX, float hitAngle, float hitX)
        {
            var settings = PaperPerformanceSettings.Default;
            var sample = new AnimationSample { IsAttack = true, State = AnimationState.Attack1, FlipX = flip, Startup = 9, Active = 5, Recovery = 10 };
            for (int frame = 0; frame < 14; frame++)
            {
                sample.AttackFrame = frame;
                var pose = PaperPerformance.Evaluate(settings, sample);
                Assert.That(pose.Degrees, Is.EqualTo(frame < 9 ? pullAngle : hitAngle));
                Assert.That(pose.X, Is.EqualTo(frame < 9 ? pullX : hitX));
                Assert.That(PaperPerformance.Evaluate(settings, sample).Degrees, Is.EqualTo(pose.Degrees));
            }
            sample.AttackFrame = 14;
            Assert.That(PaperPerformance.Evaluate(settings, sample).Degrees, Is.EqualTo(pullAngle * .4f));
            sample.AttackFrame = 17;
            Assert.That(PaperPerformance.Evaluate(settings, sample).Degrees, Is.Zero);
        }
        [TestCase(false, 45)] [TestCase(true, -45)]
        public void TumbleIsSteppedAroundTheCenter(bool flip, float angle)
        {
            var settings = PaperPerformanceSettings.Default;
            for (int age = 0; age < 24; age++)
            {
                var pose = PaperPerformance.Evaluate(settings, new AnimationSample { State = AnimationState.Knockback, Age = age, FlipX = flip });
                Assert.That(pose.Degrees, Is.EqualTo(age / 3 * angle)); Assert.That(pose.RotateAboutCenter, Is.True);
                Assert.That(pose.X, Is.Zero); Assert.That(pose.Y, Is.Zero);
            }
        }
        [TestCase(false)] [TestCase(true)]
        public void RecoilFacesAwayFromTheHitAndSettlesInSteps(bool flip)
        {
            var settings = PaperPerformanceSettings.Default;
            float direction = flip ? -1 : 1;
            var sample = new AnimationSample { State = AnimationState.Hitstun, FlipX = flip };
            var first = PaperPerformance.Evaluate(settings, sample);
            Assert.That(first.Degrees, Is.EqualTo(14 * direction)); Assert.That(first.X, Is.EqualTo(-.08f * direction));
            Assert.That(PaperPerformance.Evaluate(settings, sample, true).Degrees, Is.EqualTo(21 * direction));
            sample.Age = 12;
            Assert.That(PaperPerformance.Evaluate(settings, sample).Degrees, Is.Zero);
            Assert.That(PaperPerformance.Evaluate(settings, sample).X, Is.Zero);
        }
        [Test]
        public void IdleWalkDashDoNotAcquireAttackWobbleAndVictoryHasFourHeldPoses()
        {
            var settings = PaperPerformanceSettings.Default;
            foreach (var state in new[] { AnimationState.Idle, AnimationState.Move, AnimationState.Dash })
                for (int age = 0; age < 60; age++)
                    Assert.That(PaperPerformance.Evaluate(settings, new AnimationSample { State = state, Age = age }).Degrees, Is.Zero);
            float[] angles = { 0, 3, 0, -3 };
            for (int age = 0; age < 48; age++)
                Assert.That(PaperPerformance.Evaluate(settings, new AnimationSample { State = AnimationState.Win, Age = age }).Degrees, Is.EqualTo(angles[age / 12]));
        }
    }
}
