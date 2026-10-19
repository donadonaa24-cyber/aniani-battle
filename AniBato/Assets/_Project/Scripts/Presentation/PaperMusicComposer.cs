using System;

namespace AniBato.Presentation
{
    internal static class PaperMusicComposer
    {
        private const int Rest = -99;
        // Original eight-bar phrases; the second half answers the first in a different register.
        private static readonly int[][] Melody =
        {
            new[] { 4,7,9,Rest,7,4,2,Rest, 0,4,7,9,12,Rest,9,7, 9,7,4,Rest,2,4,7,Rest, 4,2,0,Rest,2,4,Rest,Rest,
                    7,9,12,Rest,14,12,9,7, 9,12,7,Rest,4,7,9,Rest, 2,4,7,9,7,4,2,Rest, 7,4,2,Rest,0,Rest,Rest,Rest },
            new[] { 0,7,10,Rest,12,10,7,Rest, 3,7,14,Rest,12,7,3,Rest, 5,12,15,14,12,Rest,10,7, 2,7,9,Rest,10,9,7,Rest,
                    12,Rest,10,7,14,Rest,12,10, 7,3,7,10,12,Rest,7,Rest, 5,7,12,Rest,15,14,12,10, 7,10,9,7,2,Rest,Rest,Rest },
            new[] { 0,Rest,7,9,12,9,7,Rest, 2,4,7,Rest,9,7,4,Rest, 9,12,14,Rest,12,9,7,Rest, 4,7,9,7,4,2,Rest,Rest,
                    12,14,16,Rest,14,12,9,Rest, 7,9,12,9,7,4,2,Rest, 4,7,9,12,14,12,9,7, 4,2,0,Rest,7,Rest,0,Rest },
            new[] { 7,Rest,9,12,Rest,9,7,4, 9,7,4,Rest,2,4,7,Rest, 0,4,7,9,12,Rest,9,Rest, 7,4,2,0,2,Rest,4,Rest,
                    12,Rest,14,16,14,12,9,7, 9,12,7,4,7,Rest,9,Rest, 4,7,9,Rest,12,9,7,4, 7,9,7,4,2,Rest,0,Rest }
        };
        private static readonly int[][] Roots =
        {
            new[] { 0,0,9,9,5,5,7,7, 0,9,5,7,0,5,7,7 },
            new[] { 0,0,8,8,5,5,7,7, 0,8,5,7,0,5,7,7 },
            new[] { 0,0,9,9,5,5,7,7, 0,5,9,7,0,5,7,7 },
            new[] { 0,9,2,7,0,9,5,7, 5,7,0,9,2,7,0,7 }
        };
        public static int Tempo(int theme, in ArcadeAudioSettings settings) => settings.Bpm + new[] { -12, 0, 8, 16 }[theme];
        public static float[] Render(int theme, in ArcadeAudioSettings settings)
        {
            double beat = 60d / Tempo(theme, settings), swing = theme == 0 || theme == 3 ? .09 : 0;
            var synth = new PaperAudioSynth(settings.LoopBeats * beat, true);
            int rootMidi = new[] { 62, 62, 60, 65 }[theme];
            var voice = new[] { PaperVoice.Mallet, PaperVoice.Glass, PaperVoice.Pizzicato, PaperVoice.Keys }[theme];
            for (int pulse = 0; pulse < settings.LoopBeats; pulse++)
            {
                int bar = pulse / 4, b = pulse % 4, chord = Roots[theme][bar % 16];
                bool answer = bar % 16 >= 8, minor = theme == 1 ? chord != 8 : chord == 9 || chord == 2;
                double at = pulse * beat, melody = settings.MelodyGain, rhythm = settings.RhythmGain;
                for (int half = 0; half < 2; half++)
                {
                    int step = pulse * 2 + half, note = Melody[theme][step % 64];
                    if (answer && step % 16 == 0 && note != Rest) note += 12;
                    if (note != Rest)
                    {
                        double time = at + (half == 1 ? .5 + swing : 0) * beat;
                        synth.Note(time, beat * (half == 0 && step % 8 == 6 ? .8 : .62), rootMidi + note + (theme == 1 ? 12 : 0), melody * (half == 0 ? .74 : .59), voice);
                        if (answer && note >= 7 && half == 0)
                            synth.Note(time + beat * .375, beat * .42, rootMidi + note + 12, melody * .12, PaperVoice.Glass);
                    }
                }
                // Bass alternates roots, fifths, and short approaches rather than a single drone.
                int bass = chord + (b == 2 ? 7 : b == 3 ? (chord == 7 ? -1 : 2) : 0);
                synth.Note(at, beat * .84, rootMidi - 24 + bass, rhythm * .83, PaperVoice.Bass);
                if (b == 1 || b == 3) synth.Note(at + beat * .65, beat * .3, rootMidi - 12 + chord + 7, rhythm * .27, PaperVoice.Bass);
                if (b == 0 || b == 2)
                {
                    var chordVoice = theme == 1 ? PaperVoice.Pluck : theme == 2 ? PaperVoice.Pizzicato : PaperVoice.Reed;
                    double chordTime = at + beat * (theme == 1 ? .5 : .04);
                    foreach (int interval in new[] { 0, minor ? 3 : 4, 7 })
                        synth.Note(chordTime, beat * (theme == 1 ? 1.3 : .52), rootMidi - 12 + chord + interval, melody * .18, chordVoice);
                }
                if (theme == 1 || answer)
                {
                    for (int eighth = 0; eighth < 2; eighth++)
                        synth.Note(at + (eighth * .5 + .25) * beat, beat * .35, rootMidi + chord + new[] { 7, minor ? 15 : 16 }[eighth], melody * .12, PaperVoice.Pluck);
                }
                uint seed = (uint)(1741 + theme * 193 + pulse * 29);
                if (b == 0 || b == 2 || (theme == 1 && b == 3)) synth.Drum(at, PaperDrum.Kick, rhythm * .96, seed);
                if (b == 1 || b == 3) synth.Drum(at, theme == 2 ? PaperDrum.LowTom : PaperDrum.Snare, rhythm * .85, seed + 1);
                for (int eighth = 0; eighth < 2; eighth++)
                    synth.Drum(at + (eighth == 0 ? 0 : .5 + swing) * beat, PaperDrum.Shaker, rhythm * (eighth == 0 ? .28 : .43), seed + (uint)eighth + 9);
                if (b == 1 || (theme == 2 && b == 3)) synth.Drum(at + .5 * beat, PaperDrum.Wood, rhythm * .46, seed + 4);
                if (theme == 3 && b == 3) synth.Drum(at + .75 * beat, PaperDrum.Metal, rhythm * .32, seed + 5);
                if (bar % 4 == 3 && b == 3)
                {
                    for (int fill = 0; fill < 3; fill++) synth.Drum(at + fill * .25 * beat, fill == 2 ? PaperDrum.LowTom : PaperDrum.Wood, rhythm * .45, seed + (uint)fill + 20);
                }
            }
            return synth.Finish(.72);
        }
    }
}
