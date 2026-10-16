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
        private AudioSource music, effects;
        private AudioClip[] cues, tracks;
        private int track = -1;
        private float duckRemaining;
        public float BgmVolume { get; private set; }
        public float SeVolume { get; private set; }
        public void Configure(BattleRunner value, AudioSettings data) { runner = value; settings = data; }
        private void Awake()
        {
            BgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("AniBato.BgmVolume", settings.BgmVolume));
            SeVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("AniBato.SeVolume", settings.SeVolume));
            music = gameObject.AddComponent<AudioSource>(); music.loop = true; music.playOnAwake = false; music.volume = BgmVolume;
            effects = gameObject.AddComponent<AudioSource>(); effects.playOnAwake = false; effects.volume = SeVolume;
            cues = new AudioClip[Enum.GetValues(typeof(SoundCue)).Length];
            for (int i = 0; i < cues.Length; i++) cues[i] = Clip(((SoundCue)i).ToString(), ArcadeAudio.Effect((SoundCue)i, settings.Style));
            tracks = new AudioClip[4]; for (int i = 0; i < 4; i++) tracks[i] = Clip("Original Arcade Loop " + i, ArcadeAudio.Music(i, settings.Style));
        }
        private static AudioClip Clip(string name, float[] samples)
        { var clip = AudioClip.Create(name, samples.Length, 1, ProceduralAudio.SampleRate, false); clip.SetData(samples, 0); return clip; }
        private void OnEnable() { if (runner) runner.EventProduced += OnEvent; }
        private void OnDisable() { if (runner) runner.EventProduced -= OnEvent; }
        private void OnEvent(BattleEvent value) { var cue = ProceduralAudio.ForEvent(value); if (cue.HasValue) Play(cue.Value); }
        public void Play(SoundCue cue)
        {
            if (effects && cues != null) effects.PlayOneShot(cues[(int)cue]);
            if (cue == SoundCue.NormalHit || cue == SoundCue.SpecialHit || cue == SoundCue.Ultimate || cue == SoundCue.KO || cue == SoundCue.Fight || cue == SoundCue.TimeUp)
                duckRemaining = settings.DuckSeconds;
        }
        private void LateUpdate()
        {
            duckRemaining = Mathf.Max(0, duckRemaining - Time.unscaledDeltaTime);
            if (music) music.volume = BgmVolume * (duckRemaining > 0 ? settings.DuckRatio : 1);
        }
        public void SetMusic(int theme)
        { if (!music || theme == track) return; track = theme; music.clip = tracks[theme]; music.Play(); }
        private void Start() { if (!runner.ManagedByFlow) SetMusic((int)runner.CurrentStage.Theme + 1); }
        public void AdjustVolume(bool bgm, float delta)
        {
            if (bgm) { BgmVolume = Mathf.Clamp01(BgmVolume + delta); music.volume = BgmVolume; PlayerPrefs.SetFloat("AniBato.BgmVolume", BgmVolume); }
            else { SeVolume = Mathf.Clamp01(SeVolume + delta); effects.volume = SeVolume; PlayerPrefs.SetFloat("AniBato.SeVolume", SeVolume); }
            PlayerPrefs.Save();
        }
        private void OnDestroy()
        { if (cues != null) foreach (var clip in cues) Destroy(clip); if (tracks != null) foreach (var clip in tracks) Destroy(clip); }
    }
}
