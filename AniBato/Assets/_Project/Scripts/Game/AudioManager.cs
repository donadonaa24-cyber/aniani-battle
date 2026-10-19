using System;
using AniBato.Battle;
using AniBato.Presentation;
using UnityEngine;

namespace AniBato.Game
{
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private AudioSettings settings;
        private AudioSource music, fadingMusic, effects;
        private AudioClip[] cues, tracks;
        private int track = -1;
        private float duckRemaining, duckGain = 1, fade = 1, previousGain;
        public const float CrossfadeSeconds = .35f;
        public int CurrentTrack => track;
        public string CurrentTrackName => track < 0 ? string.Empty : ArcadeAudio.TrackName(track);
        public float BgmVolume { get; private set; }
        public float SeVolume { get; private set; }
        public void Configure(BattleRunner value, AudioSettings data) { runner = value; settings = data; }
        private void Awake()
        {
            BgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("AniBato.BgmVolume", settings.BgmVolume));
            SeVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("AniBato.SeVolume", settings.SeVolume));
            music = MusicSource(); fadingMusic = MusicSource();
            effects = gameObject.AddComponent<AudioSource>(); effects.playOnAwake = false; effects.volume = SeVolume;
            cues = new AudioClip[Enum.GetValues(typeof(SoundCue)).Length];
            for (int i = 0; i < cues.Length; i++) cues[i] = Clip(((SoundCue)i).ToString(), ArcadeAudio.Effect((SoundCue)i, settings.Style));
            tracks = new AudioClip[ArcadeAudio.TrackCount];
            for (int i = 0; i < tracks.Length; i++) tracks[i] = Clip(ArcadeAudio.TrackName(i), ArcadeAudio.Music(i, settings.Style));
        }
        private AudioSource MusicSource()
        { var source = gameObject.AddComponent<AudioSource>(); source.loop = true; source.playOnAwake = false; source.volume = 0; return source; }
        private static AudioClip Clip(string name, float[] samples)
        { var clip = AudioClip.Create(name, samples.Length, 1, ProceduralAudio.SampleRate, false); clip.SetData(samples, 0); return clip; }
        private void OnEnable() { if (runner) runner.EventProduced += OnEvent; }
        private void OnDisable() { if (runner) runner.EventProduced -= OnEvent; }
        private void OnEvent(BattleEvent value) { var cue = ProceduralAudio.ForEvent(value); if (cue.HasValue) Play(cue.Value); }
        public void Play(SoundCue cue)
        {
            if (effects && cues != null) effects.PlayOneShot(cues[(int)cue]);
            if (cue == SoundCue.NormalHit || cue == SoundCue.SpecialHit || cue == SoundCue.ComboFinish || cue == SoundCue.Break ||
                cue == SoundCue.Ultimate || cue == SoundCue.KO || cue == SoundCue.Fight || cue == SoundCue.TimeUp || cue == SoundCue.Result)
                duckRemaining = settings.DuckSeconds;
        }
        private void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;
            duckRemaining = Mathf.Max(0, duckRemaining - dt);
            duckGain = Mathf.MoveTowards(duckGain, duckRemaining > 0 ? settings.DuckRatio : 1, dt * (duckRemaining > 0 ? 45 : 4));
            fade = Mathf.MoveTowards(fade, 1, dt / CrossfadeSeconds);
            if (music) music.volume = BgmVolume * duckGain * fade;
            if (fadingMusic)
            {
                fadingMusic.volume = BgmVolume * previousGain * (1 - fade);
                if (fade >= 1 && fadingMusic.isPlaying) fadingMusic.Stop();
            }
        }
        public void SetMusic(int theme)
        {
            if (theme < 0 || theme >= ArcadeAudio.TrackCount) throw new ArgumentOutOfRangeException(nameof(theme));
            if (!music || theme == track) return;
            previousGain = BgmVolume > 0 ? music.volume / BgmVolume : 0;
            var old = music; music = fadingMusic; fadingMusic = old;
            music.Stop(); music.clip = tracks[theme]; music.volume = 0; fade = 0; track = theme; music.Play();
        }
        private void Start() { if (!runner.ManagedByFlow) SetMusic((int)runner.CurrentStage.Theme + 1); }
        public void AdjustVolume(bool bgm, float delta)
        {
            if (bgm) { BgmVolume = Mathf.Clamp01(BgmVolume + delta); music.volume = BgmVolume * duckGain * fade; PlayerPrefs.SetFloat("AniBato.BgmVolume", BgmVolume); }
            else { SeVolume = Mathf.Clamp01(SeVolume + delta); effects.volume = SeVolume; PlayerPrefs.SetFloat("AniBato.SeVolume", SeVolume); }
            PlayerPrefs.Save();
        }
        private void OnDestroy()
        { if (cues != null) foreach (var clip in cues) Destroy(clip); if (tracks != null) foreach (var clip in tracks) Destroy(clip); }
    }
}
