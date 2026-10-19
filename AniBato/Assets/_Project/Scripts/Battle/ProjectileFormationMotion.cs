using System;

namespace AniBato.Battle
{
    public static class ProjectileFormationMotion
    {
        public static BattleVector Spawn(in ProjectileBehaviorDefinition behavior, BattleVector target, int index)
        {
            float radius = behavior.FormationRadius;
            if (behavior.Formation == ProjectileFormation.Converging)
            {
                var offset = index % 3 == 0 ? new BattleVector(-radius,radius*.55f) :
                    index % 3 == 1 ? new BattleVector(radius,radius*.55f) : new BattleVector(0,radius);
                return target + offset;
            }
            return target + new BattleVector(index % 2 == 0 ? -radius : radius, 0);
        }
        public static BattleVector Toward(BattleVector from, BattleVector target, float speed)
        {
            var delta=new BattleVector(target.X-from.X,target.Y-from.Y); float distance=(float)Math.Sqrt(delta.X*delta.X+delta.Y*delta.Y);
            if(distance<.00001f)return default;
            return delta*(Math.Min(speed,distance/BattleRuleDefinition.TickSeconds)/distance);
        }
    }
}
