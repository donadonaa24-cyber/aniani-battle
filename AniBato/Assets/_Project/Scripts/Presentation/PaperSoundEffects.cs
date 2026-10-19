using System;

namespace AniBato.Presentation
{
    internal static class PaperSoundEffects
    {
        public static double Duration(SoundCue cue)
        {
            switch (cue)
            {
                case SoundCue.Move: return .085;
                case SoundCue.Confirm: return .24;
                case SoundCue.Cancel: return .18;
                case SoundCue.Swing: return .19;
                case SoundCue.NormalHit: return .24;
                case SoundCue.SpecialHit: return .38;
                case SoundCue.Guard: return .22;
                case SoundCue.Just: return .5;
                case SoundCue.Counter: return .48;
                case SoundCue.Break: return .48;
                case SoundCue.ComboFinish: return .54;
                case SoundCue.Ultimate: return 1.15;
                case SoundCue.KO: return 1.45;
                case SoundCue.RingOut: return .85;
                case SoundCue.Countdown: return .26;
                case SoundCue.Shrink: return .6;
                case SoundCue.Fight: return .78;
                case SoundCue.TimeUp: return 1.05;
                case SoundCue.Result: return 1.65;
                default: throw new ArgumentOutOfRangeException(nameof(cue));
            }
        }
        public static float[] Render(SoundCue cue, double gain)
        {
            var synth = new PaperAudioSynth(Duration(cue));
            uint seed = (uint)(9173 + (int)cue * 131);
            switch (cue)
            {
                case SoundCue.Move:
                    synth.Drum(0, PaperDrum.Wood, gain * .34, seed); break;
                case SoundCue.Confirm:
                    synth.Note(0, .16, 79, gain * .43, PaperVoice.Mallet);
                    synth.Note(.065, .17, 86, gain * .32, PaperVoice.Mallet); break;
                case SoundCue.Cancel:
                    synth.Note(0, .12, 72, gain * .28, PaperVoice.Pizzicato);
                    synth.Note(.055, .12, 67, gain * .26, PaperVoice.Pizzicato); break;
                case SoundCue.Swing:
                    synth.Paper(0, .17, gain * 1.3, seed, false); break;
                case SoundCue.NormalHit:
                    synth.Paper(0, .17, gain * 1.8, seed, true);
                    synth.Drum(0, PaperDrum.Wood, gain * .76, seed + 1);
                    synth.Drum(.006, PaperDrum.LowTom, gain * .55, seed + 2); break;
                case SoundCue.SpecialHit:
                    synth.Paper(0, .23, gain * 2.1, seed, true);
                    synth.Drum(0, PaperDrum.Kick, gain * .9, seed + 1);
                    synth.Drum(.018, PaperDrum.Metal, gain * .7, seed + 2);
                    synth.Glide(.02, .26, 260, 85, gain * .35); break;
                case SoundCue.Guard:
                    synth.Drum(0, PaperDrum.Wood, gain * 1.05, seed);
                    synth.Note(0, .18, 64, gain * .4, PaperVoice.Pizzicato);
                    synth.Paper(.014, .09, gain * .6, seed + 1, true); break;
                case SoundCue.Just:
                    synth.Drum(0, PaperDrum.Metal, gain * .6, seed);
                    Chime(synth, new[] { 84,91,96 }, .055, gain * .5); break;
                case SoundCue.Counter:
                    synth.Paper(0, .18, gain * 1.2, seed, true);
                    synth.Drum(0, PaperDrum.LowTom, gain, seed + 1);
                    Chime(synth, new[] { 72,79,84 }, .045, gain * .43); break;
                case SoundCue.Break:
                    for (int n = 0; n < 4; n++)
                    { synth.Paper(n * .055, .11, gain * (1.5 - n * .2), seed + (uint)n, true); synth.Drum(n * .045, PaperDrum.Wood, gain * .55, seed + (uint)n + 7); }
                    synth.Glide(0, .4, 180, 45, gain * .48); break;
                case SoundCue.ComboFinish:
                    synth.Paper(0, .32, gain * 2.5, seed, true);
                    synth.Drum(0, PaperDrum.Kick, gain * 1.2, seed + 1);
                    synth.Drum(.025, PaperDrum.LowTom, gain * .8, seed + 2);
                    synth.Glide(.02, .36, 230, 48, gain * .52); break;
                case SoundCue.Ultimate:
                    synth.Paper(0, .65, gain * .85, seed, false);
                    synth.Glide(0, .72, 110, 650, gain * .4);
                    Chime(synth, new[] { 62,69,74,81 }, .12, gain * .4);
                    synth.Drum(.6, PaperDrum.Kick, gain, seed + 1);
                    synth.Drum(.6, PaperDrum.Metal, gain * .8, seed + 2); break;
                case SoundCue.KO:
                    synth.Drum(0, PaperDrum.Kick, gain * 1.2, seed);
                    synth.Paper(0, .3, gain * 2, seed + 1, true);
                    Chime(synth, new[] { 62,69,74,78 }, .13, gain * .55);
                    foreach (int note in new[] { 50,62,66,69 }) synth.Note(.54, .8, note, gain * .25, PaperVoice.Reed); break;
                case SoundCue.RingOut:
                    synth.Paper(0, .45, gain * .9, seed, false);
                    synth.Glide(0, .6, 650, 100, gain * .5);
                    synth.Drum(.58, PaperDrum.LowTom, gain * .6, seed + 1); break;
                case SoundCue.Countdown:
                    synth.Drum(0, PaperDrum.Wood, gain * .65, seed);
                    synth.Note(0, .23, 74, gain * .45, PaperVoice.Mallet); break;
                case SoundCue.Shrink:
                    for (int n = 0; n < 3; n++)
                    { synth.Drum(n * .16, PaperDrum.Wood, gain * .65, seed + (uint)n); synth.Paper(n * .16, .12, gain * .8, seed + (uint)n + 4, false); }
                    break;
                case SoundCue.Fight:
                    synth.Drum(0, PaperDrum.LowTom, gain * .8, seed);
                    Chime(synth, new[] { 62,66,69,74 }, .08, gain * .5);
                    foreach (int note in new[] { 62,69,74 }) synth.Note(.25, .48, note, gain * .24, PaperVoice.Reed); break;
                case SoundCue.TimeUp:
                    Chime(synth, new[] { 81,76,72,69 }, .16, gain * .45);
                    synth.Drum(.64, PaperDrum.Wood, gain * .65, seed); break;
                case SoundCue.Result:
                    Chime(synth, new[] { 62,66,69,74,78,81 }, .13, gain * .42);
                    foreach (int note in new[] { 50,62,66,69,74 }) synth.Note(.78, .82, note, gain * .23, PaperVoice.Reed);
                    synth.Drum(.78, PaperDrum.Metal, gain * .5, seed); break;
            }
            return synth.Finish(.82);
        }
        private static void Chime(PaperAudioSynth synth, int[] notes, double spacing, double gain)
        {
            for (int n = 0; n < notes.Length; n++) synth.Note(n * spacing, .36, notes[n], gain, PaperVoice.Mallet);
        }
    }
}
