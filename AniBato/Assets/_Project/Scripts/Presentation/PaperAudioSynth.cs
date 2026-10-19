using System;

namespace AniBato.Presentation
{
    internal enum PaperVoice { Mallet, Pizzicato, Keys, Reed, Glass, Pluck, Bass }
    internal enum PaperDrum { Kick, Snare, Wood, Shaker, Metal, LowTom }

    // Offline PCM rendering: no callbacks, device state, samples, or random global state.
    internal sealed class PaperAudioSynth
    {
        private const double Tau = Math.PI * 2;
        private readonly float[] samples;
        private readonly bool wrap;
        public PaperAudioSynth(double seconds, bool loop = false)
        { samples = new float[(int)Math.Ceiling(seconds * ProceduralAudio.SampleRate)]; wrap = loop; }
        private static double Sin(double frequency, double t) => frequency < ProceduralAudio.SampleRate * .45 ? Math.Sin(Tau * frequency * t) : 0;
        private static double Noise(ref uint seed)
        { seed ^= seed << 13; seed ^= seed >> 17; seed ^= seed << 5; return (seed & 65535) / 32768d - 1; }
        private void Add(int index, double value)
        {
            if (wrap) index = (index % samples.Length + samples.Length) % samples.Length;
            if (index >= 0 && index < samples.Length) samples[index] += (float)value;
        }
        public void Note(double at, double duration, double midi, double gain, PaperVoice voice)
        {
            double f = 440 * Math.Pow(2, (midi - 69) / 12);
            int start = (int)Math.Round(at * ProceduralAudio.SampleRate), count = (int)(duration * ProceduralAudio.SampleRate);
            for (int i = 0; i < count; i++)
            {
                double t = i / (double)ProceduralAudio.SampleRate, p = t / duration;
                double envelope = Math.Min(1, t / .005) * Math.Min(1, (duration - t) / .035);
                double v;
                switch (voice)
                {
                    case PaperVoice.Mallet:
                        v = Sin(f, t) + .38 * Sin(f * 2.76, t) * Math.Exp(-t * 16) + .16 * Sin(f * 5.4, t) * Math.Exp(-t * 28);
                        envelope *= Math.Exp(-p * 4.5); break;
                    case PaperVoice.Pizzicato:
                        v = Sin(f, t) + .4 * Sin(f * 2, t) * Math.Exp(-t * 5) + .22 * Sin(f * 3, t) * Math.Exp(-t * 10) + .1 * Sin(f * 4, t) * Math.Exp(-t * 18);
                        envelope *= Math.Exp(-p * 4); break;
                    case PaperVoice.Keys:
                        v = Sin(f, t) + .3 * Sin(f * 2, t) * Math.Exp(-t * 4) + .12 * Sin(f * 3.002, t) * Math.Exp(-t * 8);
                        envelope *= Math.Exp(-p * 3.2); break;
                    case PaperVoice.Reed:
                        v = .72 * Sin(f, t) + .22 * Sin(f * 2, t) + .13 * Sin(f * 3, t) + .06 * Sin(f * 4, t);
                        envelope *= .65 + .35 * Math.Exp(-p * 5); break;
                    case PaperVoice.Glass:
                        v = Sin(f, t) + .25 * Sin(f * 2.005, t) * Math.Exp(-t * 4) + .09 * Sin(f * 3.99, t) * Math.Exp(-t * 8);
                        envelope *= Math.Exp(-p * 3.8); break;
                    case PaperVoice.Pluck:
                        v = .8 * Sin(f, t) + .3 * Sin(f * 2, t) * Math.Exp(-t * 12) + .18 * Sin(f * 3, t) * Math.Exp(-t * 20);
                        envelope *= Math.Exp(-p * 3.5); break;
                    default:
                        v = .9 * Sin(f, t) + .17 * Sin(f * 2, t);
                        envelope *= Math.Exp(-p * 2.8); break;
                }
                Add(start + i, v * envelope * gain);
            }
        }
        public void Drum(double at, PaperDrum drum, double gain, uint seed)
        {
            double duration = drum == PaperDrum.Shaker ? .075 : drum == PaperDrum.Metal ? .3 : .22;
            int start = (int)Math.Round(at * ProceduralAudio.SampleRate), count = (int)(duration * ProceduralAudio.SampleRate);
            double low = 0, previous = 0;
            for (int i = 0; i < count; i++)
            {
                double t = i / (double)ProceduralAudio.SampleRate, raw = Noise(ref seed);
                low += .18 * (raw - low); double high = raw - previous; previous = raw;
                double value;
                switch (drum)
                {
                    case PaperDrum.Kick:
                        value = Math.Sin(Tau * (51 * t + 1.7 * (1 - Math.Exp(-t * 42)))) * Math.Exp(-t * 24); break;
                    case PaperDrum.Snare:
                        value = (.8 * low + .23 * Sin(175, t)) * Math.Exp(-t * 33); break;
                    case PaperDrum.Wood:
                        value = (Sin(720, t) * .55 + Sin(1134, t) * .25 + low * .3) * Math.Exp(-t * 65); break;
                    case PaperDrum.Shaker:
                        value = high * .22 * Math.Exp(-t * 60); break;
                    case PaperDrum.Metal:
                        value = (Sin(1640, t) + .35 * Sin(2381, t) + .2 * low) * .32 * Math.Exp(-t * 24); break;
                    default:
                        value = Math.Sin(Tau * (83 * t + .9 * (1 - Math.Exp(-t * 24)))) * Math.Exp(-t * 19); break;
                }
                Add(start + i, value * gain * Math.Min(1, t / .0015) * Math.Min(1, (duration - t) / .01));
            }
        }
        public void Paper(double at, double duration, double gain, uint seed, bool impact)
        {
            int start = (int)(at * ProceduralAudio.SampleRate), count = (int)(duration * ProceduralAudio.SampleRate);
            double low = 0, slower = 0;
            for (int i = 0; i < count; i++)
            {
                double t = i / (double)ProceduralAudio.SampleRate, p = t / duration;
                low += .28 * (Noise(ref seed) - low); slower += .035 * (low - slower);
                double envelope = impact ? Math.Exp(-p * 6) : Math.Pow(Math.Sin(Math.PI * p), 1.5);
                // Filtered rustle, with three folds, gives paper its softer edge.
                double folds = .7 + .3 * Math.Sin(Tau * 37 * t) * Math.Sin(Tau * 19 * t);
                Add(start + i, (low - slower) * envelope * gain * folds * Math.Min(1, t / .002));
            }
        }
        public void Glide(double at, double duration, double fromHz, double toHz, double gain)
        {
            int start = (int)(at * ProceduralAudio.SampleRate), count = (int)(duration * ProceduralAudio.SampleRate);
            for (int i = 0; i < count; i++)
            {
                double t = i / (double)ProceduralAudio.SampleRate, p = t / duration;
                double phase = fromHz * t + (toHz - fromHz) * t * p * .5;
                Add(start + i, Math.Sin(Tau * phase) * Math.Sin(Math.PI * p) * Math.Exp(-p * 1.5) * gain);
            }
        }
        public float[] Finish(double ceiling)
        {
            double peak = 0;
            for (int i = 0; i < samples.Length; i++) { samples[i] = (float)Math.Tanh(samples[i]); peak = Math.Max(peak, Math.Abs(samples[i])); }
            double gain = peak > ceiling ? ceiling / peak : 1;
            for (int i = 0; i < samples.Length; i++)
            {
                double seam = Math.Min(1, Math.Min(i, samples.Length - 1 - i) / (ProceduralAudio.SampleRate * .002));
                samples[i] = (float)(samples[i] * gain * seam);
            }
            return samples;
        }
    }
}
