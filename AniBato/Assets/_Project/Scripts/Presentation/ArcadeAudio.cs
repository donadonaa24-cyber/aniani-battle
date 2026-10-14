using System;

namespace AniBato.Presentation
{
    [Serializable]
    public struct ArcadeAudioSettings
    {
        public int Bpm, LoopBeats;
        public float MelodyGain, RhythmGain, EffectGain;
        public static ArcadeAudioSettings Default => new ArcadeAudioSettings { Bpm = 180, LoopBeats = 32, MelodyGain = .16f, RhythmGain = .21f, EffectGain = .6f };
        public void Validate()
        {
            if (Bpm < 60 || Bpm > 240 || LoopBeats < 8 || LoopBeats > 64 || LoopBeats % 8 != 0 ||
                !Gain(MelodyGain) || !Gain(RhythmGain) || !Gain(EffectGain)) throw new ArgumentException("音のテンポ・小節・ゲインが不正です。");
        }
        private static bool Gain(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0 && value <= 1;
    }
    public static class ArcadeAudio
    {
        private const double Tau = Math.PI * 2;
        private static double Note(double root, int semitone) => root * Math.Pow(2, semitone / 12.0);
        private static double Noise(ref uint seed) { seed ^= seed << 13; seed ^= seed >> 17; seed ^= seed << 5; return (seed & 65535) / 32768.0 - 1; }
        public static float[] Music(int theme, in ArcadeAudioSettings settings)
        {
            settings.Validate(); if (theme < 0 || theme > 3) throw new ArgumentOutOfRangeException(nameof(theme));
            double beat = 60d / settings.Bpm;
            var result = new float[(int)(ProceduralAudio.SampleRate * settings.LoopBeats * beat)];
            int[][] motifs = { new[] { 0,7,12,10, 3,7,15,12 }, new[] { 0,12,7,10, 2,14,9,7 }, new[] { 0,4,7,12, 9,7,4,2 }, new[] { 0,7,3,10, 12,15,10,7 } };
            double[] roots = { 220, 196, 261.625565, 174.614116 }; uint noise = (uint)(7231 + theme * 83);
            for (int i = 0; i < result.Length; i++)
            {
                double t = i / (double)ProceduralAudio.SampleRate; int pulse = (int)(t / beat);
                double b = t / beat - pulse; int step = (int)(t / (beat / 4)); double a = t - step * beat / 4;
                int section = pulse / 8 % 4, semitone = motifs[theme][step % 8] + (section == 2 ? 12 : 0);
                double f = Note(roots[theme], semitone), env = Math.Min(1, a / .003) * Math.Exp(-a * 25);
                double sine = Math.Sin(Tau * f * a);
                double lead = (section % 2 == 0 ? sine + .3 * Math.Sin(Tau * f * 2 * a) : Math.Tanh(sine * 2)) * env;
                if (section == 1 && step % 8 >= 6) lead = 0; // a short gap makes the next hook read clearly
                double bass = Math.Sin(Tau * Note(roots[theme] * .25, pulse / 4 % 2 == 0 ? 0 : 5) * b * beat) * Math.Exp(-b * 5);
                double kickAge = b * beat;
                double kick = Math.Sin(Tau * (48 * kickAge + 2.8 * (1 - Math.Exp(-kickAge * 35)))) * Math.Exp(-kickAge * 27);
                double n = Noise(ref noise);
                double snare = pulse % 2 == 1 ? (n * .65 + Math.Sin(Tau * 180 * kickAge) * .2) * Math.Exp(-kickAge * 35) : 0;
                double hatAge = t % (beat / (section == 3 ? 4 : 2));
                double hat = n * .17 * Math.Exp(-hatAge * 130);
                double chirp = pulse % 8 == 7 ? Math.Sin(Tau * (620 * kickAge + 900 * kickAge * kickAge)) * Math.Exp(-kickAge * 16) * .3 : 0;
                double mix = lead * settings.MelodyGain + (kick + snare + hat + bass * .5 + chirp) * settings.RhythmGain;
                double seam = Math.Min(1, Math.Min(i, result.Length - 1 - i) / (ProceduralAudio.SampleRate * .003));
                result[i] = (float)(Math.Tanh(mix) * seam);
            }
            return result;
        }
        public static float[] Effect(SoundCue cue, in ArcadeAudioSettings settings)
        {
            settings.Validate();
            bool finish = cue == SoundCue.KO || cue == SoundCue.RingOut || cue == SoundCue.TimeUp || cue == SoundCue.Result;
            bool impact = cue == SoundCue.NormalHit || cue == SoundCue.SpecialHit || cue == SoundCue.ComboFinish || cue == SoundCue.Break;
            bool shiny = cue == SoundCue.Just || cue == SoundCue.Counter || cue == SoundCue.Confirm || cue == SoundCue.Fight || cue == SoundCue.Result;
            double duration = finish ? .75 : cue == SoundCue.Ultimate ? .65 : .22;
            var result = new float[(int)(ProceduralAudio.SampleRate * duration)]; uint seed = (uint)(3913 + (int)cue * 17);
            for (int i = 0; i < result.Length; i++)
            {
                double t = i / (double)ProceduralAudio.SampleRate, p = t / duration, n = Noise(ref seed);
                double attack = Math.Min(1, t / .002), env = attack * (1 - p) * (1 - p);
                double f = 170 + (int)cue * 37;
                double tone;
                if (impact) tone = Math.Sin(Tau * (65 * t + 2.2 * (1 - Math.Exp(-t * 45)))) * .55 + n * .42 * Math.Exp(-t * 32);
                else if (shiny) tone = Math.Sin(Tau * f * t) * .35 + Math.Sin(Tau * f * 1.5 * t) * .22 + Math.Sin(Tau * f * 2 * t) * .12;
                else tone = Math.Sin(Tau * (f * t - f * t * p * .35)) * .55 + n * .12 * Math.Exp(-t * 24);
                if (finish) { int note = Math.Min(3, (int)(p * 4)); tone = Math.Sin(Tau * Note(f * .5, cue == SoundCue.TimeUp ? 12 - note * 4 : note * 4) * t) * .5; }
                if (cue == SoundCue.Guard) tone += Math.Sin(Tau * 1250 * t) * Math.Exp(-t * 35) * .3;
                if (cue == SoundCue.Swing) tone = n * Math.Sin(Math.PI * p) * .6;
                result[i] = (float)(Math.Tanh(tone * settings.EffectGain) * env);
            }
            return result;
        }
    }
}
