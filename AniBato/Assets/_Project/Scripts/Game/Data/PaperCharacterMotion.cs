using AniBato.Presentation;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Game
{
    [System.Serializable]
    public sealed class PaperPose
    {
        public string Key;
        public AnimationState[] States;
        public Sprite[] Frames;
        public int HoldTicks = 8;
        public bool Airborne;
        public bool ReverseFacing;
        public float TiltDegrees;
        public float[] FrameHeadHeights, FrameFootBaseYs, FrameFootMinXs, FrameFootMaxXs;
        public float HeadHeight, FootBaseY, FootMinX, FootMaxX;
        public bool Valid => HoldTicks > 0 && Frames != null && Frames.Length > 0 && System.Array.TrueForAll(Frames, frame => frame);
        public Sprite Frame(int age) => Frames[System.Math.Min(Frames.Length - 1, System.Math.Max(0, age) / HoldTicks)];
        public int FrameIndex(in AnimationSample sample) => sample.IsAttack && Frames.Length == 2 ?
            sample.AttackFrame < sample.Startup ? 0 : 1 : System.Math.Min(Frames.Length - 1, System.Math.Max(0, sample.Age) / HoldTicks);
        public Sprite Frame(in AnimationSample sample) => Frames[FrameIndex(sample)];
        private float Layout(float[] values, float fallback, in AnimationSample sample) => values != null && values.Length == Frames.Length ? values[FrameIndex(sample)] : fallback;
        public float Head(in AnimationSample sample) => Layout(FrameHeadHeights, HeadHeight, sample);
        public float Foot(in AnimationSample sample) => Layout(FrameFootBaseYs, FootBaseY, sample);
        public float FootMin(in AnimationSample sample) => Layout(FrameFootMinXs, FootMinX, sample);
        public float FootMax(in AnimationSample sample) => Layout(FrameFootMaxXs, FootMaxX, sample);
    }
    [CreateAssetMenu(menuName = "あにバト/紙人形の待機・歩行")]
    public sealed class PaperCharacterMotion : ScriptableObject
    {
        public int SchemaVersion;
        public int PerformanceSchemaVersion;
        public PaperPerformanceSettings Performance = PaperPerformanceSettings.Default;
        public bool Enabled = true;
        public Sprite Sprite;
        public float StandingHeight = 2.24f;
        public PaperRockingSettings Motion;
        public float WalkLeanDegrees = 15, DashLeanDegrees = 45;
        public PaperPose[] Poses;
        public PaperPose HeavyDamage;
        public Sprite Bag, RiceBall, Mogu, CurryWave;
        public PaperPose Find(AnimationState state, bool lostHeart = false)
        {
            if (!Enabled) return null;
            if (lostHeart && (state == AnimationState.Hitstun || state == AnimationState.Knockback) && HeavyDamage != null && HeavyDamage.Valid) return HeavyDamage;
            return Poses == null ? null : System.Array.Find(Poses, pose => pose != null && pose.Valid && pose.States != null && System.Array.IndexOf(pose.States, state) >= 0);
        }
        public bool Supports(AnimationState state) => Enabled && ((Sprite && (state == AnimationState.Idle || state == AnimationState.Move)) || Find(state) != null);
        public bool TrySprite(in AnimationSample sample, bool lostHeart, out Sprite sprite, out PaperPose pose)
        {
            pose = Find(sample.State, lostHeart); sprite = null;
            if (!Supports(sample.State)) return false;
            sprite = pose != null ? pose.Frame(sample) : Sprite;
            return sprite;
        }
        public PaperRockingSample Transform(in AnimationSample sample, PaperPose pose)
        {
            if (sample.State == AnimationState.Idle) return PaperRockingMotion.Evaluate(Motion, sample.Age, false, sample.FlipX);
            float lean = sample.State == AnimationState.Move ? WalkLeanDegrees : sample.State == AnimationState.Dash ? DashLeanDegrees : 0;
            // Idle and walking share the original standing image and its measured whole-sole span.
            bool standing = pose == null || pose.Key == "idle";
            return PaperTravelLean.Evaluate(lean, sample.FlipX, standing ? Motion.FootMinX : pose.FootMinX, standing ? Motion.FootMaxX : pose.FootMaxX);
        }
    }
}
