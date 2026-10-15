using System;

namespace AniBato.Battle
{
    public enum NormalDirection { Weak, Crouch, Back, AirNeutral, AirForward, AirBack, AirUp, AirDown, AirForwardUp, AirForwardDown, Up }
    public static class DirectionalAttackSelector
    {
        public static NormalDirection Select(FighterState fighter, BattleVector input, float threshold, bool singleAerial = false)
        {
            float forward = input.X * fighter.Facing;
            if (fighter.Grounded) return input.Y > threshold && Has(fighter.Definition, NormalDirection.Up) ? NormalDirection.Up :
                input.Y < -threshold ? NormalDirection.Crouch : forward < -threshold ? NormalDirection.Back : NormalDirection.Weak;
            if (singleAerial) return NormalDirection.AirNeutral;
            float diagonal = fighter.Definition.DiagonalThreshold > 0 ? fighter.Definition.DiagonalThreshold : threshold;
            if (forward > diagonal && Math.Abs(input.Y) > diagonal)
            {
                var direction = input.Y > 0 ? NormalDirection.AirForwardUp : NormalDirection.AirForwardDown;
                if (Has(fighter.Definition, direction)) return direction;
                return fighter.Definition.DiagonalFallbackVertical ? (input.Y > 0 ? NormalDirection.AirUp : NormalDirection.AirDown) : NormalDirection.AirForward;
            }
            if (input.Y > threshold) return NormalDirection.AirUp;
            if (input.Y < -threshold) return NormalDirection.AirDown;
            return forward > threshold ? NormalDirection.AirForward : forward < -threshold ? NormalDirection.AirBack : NormalDirection.AirNeutral;
        }
        public static bool Has(in CharacterDefinition definition, NormalDirection direction) => direction == NormalDirection.Up ? definition.UpNormal.ActiveFrames > 0 : definition.DirectionalNormals != null &&
            (int)direction < definition.DirectionalNormals.Length && definition.DirectionalNormals[(int)direction].ActiveFrames > 0;
        public static AttackDefinition Attack(in CharacterDefinition definition, NormalDirection direction, int chain, bool air) =>
            direction == NormalDirection.Up && Has(definition, direction) ? definition.UpNormal :
                Has(definition, direction) ? definition.DirectionalNormals[(int)direction] : (air ? definition.AirNormals : definition.GroundNormals)[chain];
    }
}
