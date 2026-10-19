using System;

namespace AniBato.Presentation
{
    [Serializable]
    public struct ArcadeAudioSettings
    {
        public int Bpm, LoopBeats;
        public float MelodyGain, RhythmGain, EffectGain;
        public static ArcadeAudioSettings Default => new ArcadeAudioSettings
        { Bpm = 156, LoopBeats = 64, MelodyGain = .3f, RhythmGain = .32f, EffectGain = .72f };
        public void Validate()
        {
            if (Bpm < 60 || Bpm > 240 || LoopBeats < 8 || LoopBeats > 64 || LoopBeats % 8 != 0 ||
                !Gain(MelodyGain) || !Gain(RhythmGain) || !Gain(EffectGain)) throw new ArgumentException("音のテンポ・小節・ゲインが不正です。");
        }
        private static bool Gain(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0 && value <= 1;
    }
    public static class ArcadeAudio
    {
        public const int TrackCount = 4;
        public static string TrackName(int theme)
        { CheckTheme(theme); return new[] { "紙の舞台、開幕！", "青い都市のパルス", "駒たちの大立ち回り", "文化祭キッチン・ラッシュ" }[theme]; }
        public static int Tempo(int theme, in ArcadeAudioSettings settings)
        { settings.Validate(); CheckTheme(theme); return PaperMusicComposer.Tempo(theme, settings); }
        public static float[] Music(int theme, in ArcadeAudioSettings settings)
        { settings.Validate(); CheckTheme(theme); return PaperMusicComposer.Render(theme, settings); }
        public static float[] Effect(SoundCue cue, in ArcadeAudioSettings settings)
        { settings.Validate(); return PaperSoundEffects.Render(cue, settings.EffectGain); }
        private static void CheckTheme(int theme)
        { if (theme < 0 || theme >= TrackCount) throw new ArgumentOutOfRangeException(nameof(theme)); }
    }
}
