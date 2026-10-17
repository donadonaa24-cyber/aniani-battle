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
        public bool RotateWithRush;
        public bool SwitchOnProjectileRelease;
        public float TiltDegrees;
        public float[] FrameHeadHeights, FrameFootBaseYs, FrameFootMinXs, FrameFootMaxXs, FrameOriginYs;
        public float HeadHeight, FootBaseY, FootMinX, FootMaxX;
        public bool Valid => HoldTicks > 0 && Frames != null && Frames.Length > 0 && System.Array.TrueForAll(Frames, frame => frame);
        public Sprite Frame(int age) => Frames[System.Math.Min(Frames.Length - 1, System.Math.Max(0, age) / HoldTicks)];
        public int FrameIndex(in AnimationSample sample) => sample.IsAttack && Frames.Length == 2 ?
            sample.AttackFrame < (SwitchOnProjectileRelease ? sample.ProjectileReleaseFrame : sample.Startup + sample.DamageDelay) ? 0 : 1 : System.Math.Min(Frames.Length - 1, System.Math.Max(0, sample.Age) / HoldTicks);
        public Sprite Frame(in AnimationSample sample) => Frames[FrameIndex(sample)];
        private float Layout(float[] values, float fallback, in AnimationSample sample) => values != null && values.Length == Frames.Length ? values[FrameIndex(sample)] : fallback;
        public float Head(in AnimationSample sample) => Layout(FrameHeadHeights, HeadHeight, sample);
        public float Foot(in AnimationSample sample) => Layout(FrameFootBaseYs, FootBaseY, sample);
        public float FootMin(in AnimationSample sample) => Layout(FrameFootMinXs, FootMinX, sample);
        public float FootMax(in AnimationSample sample) => Layout(FrameFootMaxXs, FootMaxX, sample);
        public float OriginY(in AnimationSample sample) => Layout(FrameOriginYs, 0, sample);
    }
    [System.Serializable]
    public sealed class PaperProjectileArt
    {
        public AniBato.Battle.AbilityLook Look;
        public Sprite Sprite;
        public float Scale = 1;
        public bool RotateWithVelocity, FlipWithFacing;
        public float RotationOffsetDegrees;
        public int SortingOrder = 7;
    }
    [System.Serializable]
    public sealed class PaperAbilityArt
    {
        public AnimationState State;
        public AniBato.Battle.AbilityLook Look;
        public Sprite Sprite;
        public float Scale = 1;
        public Vector2 Offset;
        public Sprite AnticipationSprite;
        public float AnticipationScale = 1;
        public string AnticipationText, AlternateAnticipationText;
        public float AnticipationTextSize = .08f;
        public bool DuringStartup, DuringRecovery;
        public int StepTicks = 6;
        public float RockDegrees = 3;
        public bool Anticipating(in AnimationSample sample) => sample.IsAttack && sample.State == State && sample.AttackFrame < sample.Startup + sample.DamageDelay;
        public Sprite SpriteFor(in AnimationSample sample) => AnticipationSprite && Anticipating(sample) ? AnticipationSprite : Sprite;
        public float ScaleFor(in AnimationSample sample) => AnticipationSprite && Anticipating(sample) ? AnticipationScale : Scale;
        public bool Visible(in AnimationSample sample) => sample.IsAttack && sample.State == State &&
            (sample.AttackFrame < sample.Startup + sample.DamageDelay ? DuringStartup :
             sample.AttackFrame < sample.Startup + sample.Active || DuringRecovery);
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
        public int DisplayTimingSchemaVersion;
        public int UltimateImpactMinimumTicks=96, CurryAfterglowTicks=24;
        public PaperPose[] Poses;
        public PaperPose HeavyDamage;
        public Sprite Bag, RiceBall, Mogu, CurryWave;
        public PaperProjectileArt[] Projectiles;
        public PaperAbilityArt[] AbilityEffects;
        public PaperAbilityArt AbilityEffect(AnimationState state) => !Enabled || AbilityEffects == null ? null :
            System.Array.Find(AbilityEffects, art => art != null && art.Sprite && art.Scale > 0 && art.State == state);
        public PaperAbilityArt AbilityEffect(AniBato.Battle.AbilityLook look) => !Enabled || AbilityEffects == null || look == AniBato.Battle.AbilityLook.Default ? null :
            System.Array.Find(AbilityEffects, art => art != null && art.Sprite && art.Scale > 0 && art.Look == look);
        public PaperProjectileArt Projectile(AniBato.Battle.AbilityLook look) => !Enabled || Projectiles == null ? null :
            System.Array.Find(Projectiles, art => art != null && art.Sprite && art.Scale > 0 && art.Look == look);
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
