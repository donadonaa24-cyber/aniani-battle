using System;
using AniBato.Battle;

namespace AniBato.Presentation
{
    public enum SoundCue { NormalHit, SpecialHit, Guard, Just, Counter, Break, ComboFinish, Ultimate, KO, RingOut, Countdown, Move, Confirm, Cancel, Shrink, Swing, Fight, TimeUp, Result }
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
                case BattleEventKind.TimeUp: return SoundCue.TimeUp;
                case BattleEventKind.AttackStarted: return SoundCue.Swing;
                case BattleEventKind.StageShrinkWarning: case BattleEventKind.StageShrinkStarted:
                case BattleEventKind.StageSegmentWarning: return SoundCue.Shrink;
                default: return null;
            }
        }
        public static float[] Effect(SoundCue cue) => ArcadeAudio.Effect(cue, ArcadeAudioSettings.Default);
        public static float[] Music(int theme) => ArcadeAudio.Music(theme, ArcadeAudioSettings.Default);
    }
}
