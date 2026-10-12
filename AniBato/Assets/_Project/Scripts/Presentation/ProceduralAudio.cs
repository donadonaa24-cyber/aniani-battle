using System;
using AniBato.Battle;

namespace AniBato.Presentation
{
    public enum SoundCue { NormalHit, SpecialHit, Guard, Just, Counter, Break, ComboFinish, Ultimate, KO, RingOut, Countdown, Move, Confirm, Cancel, Shrink }
    public static class ProceduralAudio
    {
        public const int SampleRate = 16000;
        public static SoundCue? ForEvent(BattleEvent value)
        {
            switch (value.Kind)
            {
                case BattleEventKind.Hit: return value.AttackKind == AttackKind.Normal ? SoundCue.NormalHit : SoundCue.SpecialHit;
                case BattleEventKind.Guarded: return SoundCue.Guard;
                case BattleEventKind.JustGuard: case BattleEventKind.JustDodge: return SoundCue.Just;
                case BattleEventKind.CounterSuccess: return SoundCue.Counter;
                case BattleEventKind.Break: case BattleEventKind.GuardBroken: return SoundCue.Break;
                case BattleEventKind.ComboFinish: return SoundCue.ComboFinish;
                case BattleEventKind.UltimateStarted: return SoundCue.Ultimate;
                case BattleEventKind.KO: return SoundCue.KO;
                case BattleEventKind.RingOut: return SoundCue.RingOut;
                case BattleEventKind.TimeUp: return SoundCue.KO;
                case BattleEventKind.StageShrinkWarning: case BattleEventKind.StageShrinkStarted:
                case BattleEventKind.StageSegmentWarning: return SoundCue.Shrink;
                default: return null;
            }
        }
        // Original synthesized motifs, no recordings. Fixed seed noise makes generation reproducible.
        public static float[] Effect(SoundCue cue)
        {
            float duration = cue == SoundCue.Ultimate || cue == SoundCue.KO || cue == SoundCue.RingOut ? .65f : .18f;
            var samples = new float[(int)(SampleRate * duration)]; uint noise = 913u;
            double phase = 0; float frequency = 180 + (int)cue * 47;
            for (int i = 0; i < samples.Length; i++)
            {
                double t = i / (double)SampleRate, progress = i / (double)samples.Length;
                phase += 2 * Math.PI * frequency * (1 + (cue == SoundCue.Just || cue == SoundCue.Ultimate ? progress : -progress * .65)) / SampleRate;
                noise ^= noise << 13; noise ^= noise >> 17; noise ^= noise << 5;
                double envelope = Math.Min(1, t / .005) * Math.Pow(1 - progress, 2);
                double grit = cue <= SoundCue.Guard || cue == SoundCue.Break ? ((noise & 65535) / 32768.0 - 1) * .2 : 0;
                samples[i] = (float)((Math.Sin(phase) * .24 + Math.Sin(phase * 2) * .08 + grit) * envelope);
            }
            return samples;
        }
        public static float[] Music(int theme)
        {
            if (theme < 0 || theme > 3) throw new ArgumentOutOfRangeException(nameof(theme));
            const double beat = .25; const int notes = 32;
            var samples = new float[(int)(SampleRate * notes * beat)];
            int[] motif = { 0, 7, 12, 7, 3, 10, 15, 10, 5, 12, 17, 12, 7, 14, 19, 14 };
            double root = theme == 0 ? 220 : theme == 1 ? 196 : theme == 2 ? 261.6256 : 174.6141;
            for (int i = 0; i < samples.Length; i++)
            {
                double t = i / (double)SampleRate; int note = (int)(t / beat);
                double age = t - note * beat, envelope = Math.Min(1, age / .008) * Math.Min(1, (beat - age) / .04);
                double frequency = root * Math.Pow(2, motif[(note + theme * 4) % motif.Length] / 12.0);
                double tone = Math.Sin(2 * Math.PI * frequency * age) + .2 * Math.Sin(4 * Math.PI * frequency * age);
                double bass = Math.Sin(2 * Math.PI * root * .5 * age) * Math.Exp(-age * 12);
                samples[i] = (float)((tone * .09 + bass * .08) * envelope);
            }
            return samples;
        }
    }
}
