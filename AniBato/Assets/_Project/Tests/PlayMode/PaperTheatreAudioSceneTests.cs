using System.Collections;
using System.Linq;
using System.Reflection;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class GameFlowSceneTests
    {
        private static T AudioField<T>(AudioManager audio, string name) =>
            (T)typeof(AudioManager).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(audio);
        [UnityTest]
        public IEnumerator PaperTheatreTracksCrossfadeAndEffectsDuckMusicEvenWhileTimeIsPaused()
        {
            var audio = Flow.Audio; var tracks = AudioField<AudioClip[]>(audio, "tracks"); var cues = AudioField<AudioClip[]>(audio, "cues");
            Assert.That(tracks.Length, Is.EqualTo(4)); Assert.That(cues.Length, Is.EqualTo(19));
            foreach (var clip in tracks.Concat(cues))
            {
                Assert.That(clip.frequency, Is.EqualTo(24000)); Assert.That(clip.channels, Is.EqualTo(1));
                var data = new float[clip.samples]; Assert.That(clip.GetData(data, 0), Is.True);
                Assert.That(data.Any(x => Mathf.Abs(x) > .01f), Is.True, clip.name);
            }
            for (int theme = 0; theme < 4; theme++)
            {
                var sim = Flow.Runner.Simulation; int? tick = sim?.Match.TickCount;
                audio.SetMusic(theme);
                var current = AudioField<AudioSource>(audio, "music");
                Assert.That(current.clip, Is.SameAs(tracks[theme])); Assert.That(current.loop, Is.True);
                Assert.That(audio.CurrentTrackName, Is.EqualTo(ArcadeAudio.TrackName(theme)));
                yield return new WaitForSecondsRealtime(.43f);
                Assert.That(current.isPlaying, Is.True); Assert.That(AudioField<AudioSource>(audio, "fadingMusic").isPlaying, Is.False);
                Assert.That(current.volume, Is.EqualTo(audio.BgmVolume).Within(.001f));
                int position = current.timeSamples; audio.SetMusic(theme); Assert.That(current.timeSamples, Is.GreaterThanOrEqualTo(position));
                if (sim != null) Assert.That(sim.Match.TickCount, Is.EqualTo(tick));
            }
            float previousTimeScale = Time.timeScale;
            try
            {
                Time.timeScale = 0; audio.Play(SoundCue.ComboFinish);
                yield return new WaitForSecondsRealtime(.07f);
                var current = AudioField<AudioSource>(audio, "music"); var effects = AudioField<AudioSource>(audio, "effects");
                Assert.That(effects.isPlaying, Is.True);
                Assert.That(current.volume, Is.LessThanOrEqualTo(audio.BgmVolume * .5f + .0001f));
                yield return new WaitForSecondsRealtime(.5f);
                Assert.That(current.volume, Is.EqualTo(audio.BgmVolume).Within(.001f));
                foreach (SoundCue cue in System.Enum.GetValues(typeof(SoundCue))) audio.Play(cue);
                Assert.That(effects.isPlaying, Is.True);
            }
            finally { Time.timeScale = previousTimeScale; }
        }
    }
}
