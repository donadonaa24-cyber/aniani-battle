using System;

namespace AniBato.Battle
{
    public enum ProjectileFormation { Default, Converging, AlternatingRush }
    [Serializable]
    public struct ProjectileBehaviorDefinition
    {
        public bool Homing, ConsumeOnHit, WallAfterLanding;
        public float HomingSpeed, FormationRadius, VisualScale;
        public BattleVector VisualOffset;
        public int StaggerFrames, WallFrames, RepeatIntervalFrames;
        public ProjectileFormation Formation;
        public void Validate()
        {
            if (!Finite(HomingSpeed) || !Finite(FormationRadius) || !Finite(VisualScale) || !Finite(VisualOffset.X) || !Finite(VisualOffset.Y) ||
                HomingSpeed < 0 || FormationRadius < 0 || VisualScale < 0 || StaggerFrames < 0 || WallFrames < 0 || RepeatIntervalFrames < 0 ||
                Homing && HomingSpeed <= 0 || WallAfterLanding && WallFrames < 1 || Formation != ProjectileFormation.Default && FormationRadius <= 0)
                throw new ArgumentException("投射物の追跡・時差・壁の定義が不正です。");
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
    [Serializable]
    public struct AftermathDefinition
    {
        public int LifetimeFrames, RepeatIntervalFrames;
        public BattleVector Size;
        public bool Enabled => LifetimeFrames > 0;
        public void Validate()
        {
            if (LifetimeFrames < 0 || RepeatIntervalFrames < 0 || float.IsNaN(Size.X) || float.IsNaN(Size.Y) || float.IsInfinity(Size.X) || float.IsInfinity(Size.Y) ||
                Enabled && (RepeatIntervalFrames < 1 || Size.X <= 0 || Size.Y <= 0))
                throw new ArgumentException("残留攻撃の持続・判定が不正です。");
        }
    }
}
