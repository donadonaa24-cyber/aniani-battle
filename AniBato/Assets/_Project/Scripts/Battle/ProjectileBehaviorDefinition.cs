using System;

namespace AniBato.Battle
{
    public enum ProjectileFormation { Default, Converging, AlternatingRush, RoyalMarch }
    [Serializable]
    public struct ProjectileBehaviorDefinition
    {
        public bool Homing, ConsumeOnHit, WallAfterLanding, MainPlatformOnly, ExplodeOnLanding;
        public BattleVector ExplosionSize;
        public int ExplosionFrames;
        public float FormationVerticalSpread;
        public int MarchSpawnFrames, MarchCommandFrames, MarchTravelFrames, MarchExitFrames;
        public float MarchWidth0, MarchWidth1, MarchWidth2;
        public float MarchWidth(int index) => index == 0 ? MarchWidth0 : index == 1 ? MarchWidth1 : MarchWidth2;
        public float HomingSpeed, FormationRadius, VisualScale;
        public BattleVector VisualOffset;
        public int StaggerFrames, WallFrames, RepeatIntervalFrames;
        public ProjectileFormation Formation;
        public void Validate()
        {
            if (!Finite(HomingSpeed) || !Finite(FormationRadius) || !Finite(VisualScale) || !Finite(VisualOffset.X) || !Finite(VisualOffset.Y) ||
                HomingSpeed < 0 || FormationRadius < 0 || VisualScale < 0 || StaggerFrames < 0 || WallFrames < 0 || RepeatIntervalFrames < 0 ||
                !Finite(FormationVerticalSpread) || FormationVerticalSpread < 0 || ExplosionFrames < 0 ||
                ExplodeOnLanding && (ExplosionFrames < 1 || !Finite(ExplosionSize.X) || !Finite(ExplosionSize.Y) || ExplosionSize.X <= 0 || ExplosionSize.Y <= 0) ||
                Formation == ProjectileFormation.RoyalMarch && (MarchSpawnFrames < 1 || MarchCommandFrames < 1 || MarchTravelFrames < 1 || MarchExitFrames < 1 ||
                    !Finite(MarchWidth0) || !Finite(MarchWidth1) || !Finite(MarchWidth2) || MarchWidth0 <= 0 || MarchWidth1 <= 0 || MarchWidth2 <= 0) ||
                Homing && HomingSpeed <= 0 || WallAfterLanding && WallFrames < 1 ||
                Formation != ProjectileFormation.Default && Formation != ProjectileFormation.RoyalMarch && FormationRadius <= 0)
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
