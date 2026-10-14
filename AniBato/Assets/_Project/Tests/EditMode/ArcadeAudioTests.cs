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
            settings.Style.Validate(); Assert.That(settings.Style.Bpm, Is.EqualTo(180));
            float[] previous = null;
            for (int theme = 0; theme < 4; theme++)
            {
                var wave = ArcadeAudio.Music(theme, settings.Style);
                Assert.That(wave, Is.EqualTo(ArcadeAudio.Music(theme, settings.Style)));
                Assert.That(wave.All(x => !float.IsNaN(x) && !float.IsInfinity(x) && Math.Abs(x) < .9f), Is.True);
                Assert.That(Math.Sqrt(wave.Average(x => (double)x * x)), Is.InRange(.03, .3));
                Assert.That(wave[0], Is.Zero); Assert.That(wave[wave.Length - 1], Is.Zero);
                if (previous != null) Assert.That(wave.SequenceEqual(previous), Is.False);
                previous = wave; WriteWave("BGM_" + new[] { "メニュー", "星の終わり", "推し駒", "料理の舞台" }[theme], wave);
            }
            var cues = Enum.GetValues(typeof(SoundCue)).Cast<SoundCue>();
            var demo = cues.SelectMany(c => ArcadeAudio.Effect(c, settings.Style).Concat(new float[1600])).ToArray();
            WriteWave("SE_通常_特殊_防御_ジャスト_反撃_ブレイク_コンボ_超必殺_KO_場外_カウント_移動_決定_取消_縮小_振り_FIGHT_時間切れ_結果", demo);
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
