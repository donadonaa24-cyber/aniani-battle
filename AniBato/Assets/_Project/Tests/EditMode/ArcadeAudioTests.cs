using System;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AniBato.Tests
{
    public sealed class ArcadeAudioTests
    {
        [Test]
        public void FourOriginalLoopsHaveRhythmHeadroomSeamSilenceAndReproducibleSamples()
        {
            var settings = AssetDatabase.LoadAssetAtPath<AniBato.Game.AudioSettings>("Assets/_Project/Data/AudioSettings.asset");
            settings.Style.Validate(); Assert.That(settings.StyleVersion, Is.EqualTo(1));
            Assert.That(settings.Style.LoopBeats, Is.EqualTo(64));
            float[] previous = null;
            for (int theme = 0; theme < 4; theme++)
            {
                var wave = ArcadeAudio.Music(theme, settings.Style);
                Assert.That(wave, Is.EqualTo(ArcadeAudio.Music(theme, settings.Style)));
                Assert.That(wave.All(x => !float.IsNaN(x) && !float.IsInfinity(x) && Math.Abs(x) < .9f), Is.True);
                Assert.That(Math.Sqrt(wave.Average(x => (double)x * x)), Is.InRange(.03, .3));
                Assert.That(wave[0], Is.Zero); Assert.That(wave[wave.Length - 1], Is.Zero);
                Assert.That(Math.Abs(wave[1]) + Math.Abs(wave[wave.Length - 2]), Is.LessThan(.03));
                // No dropped/silent bars, and the answering phrase must differ from the first half.
                int barSamples = (int)(4 * ProceduralAudio.SampleRate * 60d / ArcadeAudio.Tempo(theme, settings.Style));
                for (int bar = 0; bar < 16; bar++)
                    Assert.That(Math.Sqrt(wave.Skip(bar * barSamples).Take(barSamples).Average(x => (double)x * x)), Is.GreaterThan(.03), "silent bar " + bar);
                Assert.That(wave.Take(wave.Length / 2).SequenceEqual(wave.Skip(wave.Length / 2).Take(wave.Length / 2)), Is.False);
                if (previous != null) Assert.That(wave.SequenceEqual(previous), Is.False);
                previous = wave; WriteWave("BGM_" + new[] { "メニュー", "星の終わり", "推し駒", "料理の舞台" }[theme], wave);
            }
            var cues = Enum.GetValues(typeof(SoundCue)).Cast<SoundCue>();
            var demo = cues.SelectMany(c => ArcadeAudio.Effect(c, settings.Style).Concat(new float[ProceduralAudio.SampleRate / 5])).ToArray();
            WriteWave("SE_通常_特殊_防御_ジャスト_反撃_ブレイク_コンボ_超必殺_KO_場外_カウント_移動_決定_取消_縮小_振り_FIGHT_時間切れ_結果", demo);
            foreach (var cue in cues) WriteWave("SE_" + cue, ArcadeAudio.Effect(cue, settings.Style));
        }
        [Test]
        public void EveryCueHasHeadroomGentleEdgesAndItsOwnWaveform()
        {
            var waves = Enum.GetValues(typeof(SoundCue)).Cast<SoundCue>().Select(ProceduralAudio.Effect).ToArray();
            for (int i = 0; i < waves.Length; i++)
            {
                var wave = waves[i];
                Assert.That(wave.All(x => !float.IsNaN(x) && !float.IsInfinity(x) && Math.Abs(x) <= .821), Is.True);
                Assert.That(Math.Sqrt(wave.Average(x => (double)x * x)), Is.InRange(.008, .4));
                Assert.That(wave[0], Is.Zero); Assert.That(wave[wave.Length - 1], Is.Zero);
                for (int j = 0; j < i; j++) Assert.That(wave.SequenceEqual(waves[j]), Is.False);
            }
            Assert.That(waves[(int)SoundCue.ComboFinish].Max(x => Math.Abs(x)), Is.GreaterThan(waves[(int)SoundCue.Move].Max(x => Math.Abs(x))));
        }
        [Test]
        public void AudioMigrationRunsOnceAndKeepsVolumesAndLaterTuning()
        {
            var data = ScriptableObject.CreateInstance<AniBato.Game.AudioSettings>();
            try
            {
                data.BgmVolume = .22f; data.SeVolume = .71f; data.UpgradePaperStyle();
                Assert.That(data.StyleVersion, Is.EqualTo(1)); Assert.That(data.Style.LoopBeats, Is.EqualTo(64));
                Assert.That(data.BgmVolume, Is.EqualTo(.22f)); Assert.That(data.SeVolume, Is.EqualTo(.71f));
                data.Style.Bpm = 144; data.Style.MelodyGain = .27f; data.DuckRatio = .31f; data.UpgradePaperStyle();
                Assert.That(data.Style.Bpm, Is.EqualTo(144)); Assert.That(data.Style.MelodyGain, Is.EqualTo(.27f)); Assert.That(data.DuckRatio, Is.EqualTo(.31f));
            }
            finally { UnityEngine.Object.DestroyImmediate(data); }
        }
        [TestCase(BattleEventKind.AttackStarted, SoundCue.Swing)]
        [TestCase(BattleEventKind.TimeUp, SoundCue.TimeUp)]
        [TestCase(BattleEventKind.Hit, SoundCue.NormalHit)]
        public void ImportantActionsHaveTheirOwnSound(BattleEventKind kind, SoundCue cue)
        { Assert.That(ProceduralAudio.ForEvent(new BattleEvent(kind, 0)), Is.EqualTo(cue)); }
        [Test]
        public void TwoAttackPicturesFollowHitboxPhasesAndKeepTheStrikeThroughRecovery()
        {
            var texture = new Texture2D(2, 2);
            var first = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero);
            var second = Sprite.Create(texture, new Rect(1, 0, 1, 1), Vector2.zero);
            try
            {
                var pose = new PaperPose { Frames = new[] { first, second }, HoldTicks = 8 };
                for (int frame = 0; frame < 30; frame++)
                {
                    var sample = new AnimationSample { IsAttack = true, AttackFrame = frame, Age = frame, Startup = 12, Active = 5, Recovery = 13 };
                    Assert.That(pose.Frame(sample), Is.SameAs(frame < 12 ? first : second));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(first); UnityEngine.Object.DestroyImmediate(second); UnityEngine.Object.DestroyImmediate(texture); }
        }
        private static void WriteWave(string name, float[] samples)
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../TestResults/arcade_audio")); Directory.CreateDirectory(folder);
            using (var writer = new BinaryWriter(File.Create(Path.Combine(folder, name + ".wav"))))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + samples.Length * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
                writer.Write(ProceduralAudio.SampleRate); writer.Write(ProceduralAudio.SampleRate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(samples.Length * 2);
                foreach (float sample in samples) writer.Write((short)Math.Round(sample * 32767));
            }
        }
    }
}
