using System;
using System.Linq;
using AniBato.Battle;
using AniBato.Presentation;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class ProceduralAudioTests
    {
        [TestCase(MatchEndReason.RingOut, "KO!")]
        [TestCase(MatchEndReason.Hearts, "KO!")]
        [TestCase(MatchEndReason.TimeUp, "TIME UP!")]
        public void FinishUsesEndReasonRatherThanWinner(MatchEndReason reason, string text)
        {
            var feedback = new BattleFeedback(); feedback.Apply(new BattleEvent(BattleEventKind.MatchEnded, 1, value: (int)MatchResult.Player2Wins, endReason: reason), .2f, .2f, .8f);
            Assert.That(feedback.FinishText, Is.EqualTo(text)); feedback.Advance(1); Assert.That(feedback.FinishRemaining, Is.Zero);
        }
        [Test]
        public void WaveformsAreFiniteBoundedAndReproducible()
        {
            foreach (SoundCue cue in Enum.GetValues(typeof(SoundCue)))
            {
                var wave = ProceduralAudio.Effect(cue); Assert.That(wave.All(x => !float.IsNaN(x) && Math.Abs(x) < 1), Is.True);
                Assert.That(wave, Is.EqualTo(ProceduralAudio.Effect(cue))); Assert.That(wave.Any(x => x != 0), Is.True);
            }
            for (int i = 0; i < 4; i++)
            { var music = ProceduralAudio.Music(i); Assert.That(music.Length, Is.EqualTo((int)(16000 * 32 * 60d / 180))); Assert.That(music.All(x => Math.Abs(x) < 1), Is.True); }
        }
        [Test]
        public void AudioEventConsumptionLeavesSimulationUnchanged()
        {
            var a = BattleTestFactory.Create(definition: PhaseThreeDefaults.Character(0)); var b = BattleTestFactory.Create(definition: PhaseThreeDefaults.Character(0));
            var aa = new CpuBrain(CpuDefinition.ForDifficulty(1), 23); var ab = new CpuBrain(CpuDefinition.ForDifficulty(1), 45);
            var ba = new CpuBrain(CpuDefinition.ForDifficulty(1), 23); var bb = new CpuBrain(CpuDefinition.ForDifficulty(1), 45);
            var feedback = new BattleFeedback();
            while (!a.Match.IsFinished)
            {
                a.Tick(aa.NextInput(a, 0), ab.NextInput(a, 1)); b.Tick(ba.NextInput(b, 0), bb.NextInput(b, 1));
                foreach (var e in a.Events) { ProceduralAudio.ForEvent(e); feedback.Apply(e, .2f, .2f, .8f); }
                feedback.Advance(1f / 60);
                Assert.That(a.P1.Position.X, Is.EqualTo(b.P1.Position.X)); Assert.That(a.P1.Gauge, Is.EqualTo(b.P1.Gauge));
                Assert.That(a.P2.Hearts, Is.EqualTo(b.P2.Hearts)); Assert.That(a.Match.RemainingTicks, Is.EqualTo(b.Match.RemainingTicks));
            }
            Assert.That(a.Match.Result, Is.EqualTo(b.Match.Result));
        }
    }
}
