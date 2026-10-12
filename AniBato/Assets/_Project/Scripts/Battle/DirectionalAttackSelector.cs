using System;

namespace AniBato.Battle
{
    public enum NormalDirection { Weak, Crouch, Back, AirNeutral, AirForward, AirBack, AirUp, AirDown, AirForwardUp, AirForwardDown }
    public static class DirectionalAttackSelector
    {
        public static NormalDirection Select(FighterState fighter, BattleVector input, float threshold)
        {
            float forward = input.X * fighter.Facing;
            if (fighter.Grounded) return input.Y < -threshold ? NormalDirection.Crouch : forward < -threshold ? NormalDirection.Back : NormalDirection.Weak;
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
        public static bool Has(in CharacterDefinition definition, NormalDirection direction) => definition.DirectionalNormals != null &&
            (int)direction < definition.DirectionalNormals.Length && definition.DirectionalNormals[(int)direction].ActiveFrames > 0;
        public static AttackDefinition Attack(in CharacterDefinition definition, NormalDirection direction, int chain, bool air) =>
            Has(definition, direction) ? definition.DirectionalNormals[(int)direction] : (air ? definition.AirNormals : definition.GroundNormals)[chain];
    }
}
