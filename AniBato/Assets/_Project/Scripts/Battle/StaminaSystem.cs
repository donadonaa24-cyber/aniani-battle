using System;

namespace AniBato.Battle
{
    [Serializable]
    public struct StaminaDefinition
    {
        public int Maximum, GroundRecoveryPerSecond, ActiveRecoveryPerSecond, RecoveryDelayFrames;
        public int JumpCost, AirJumpCost, GroundDashCost, AirDashCost, DodgeCost;
        public int GuardDrainPerSecond, NormalGuardCost, SpecialGuardCost, UltimateGuardCost;
        public int GuardBreakFrames, GuardLockFrames;
        public bool Enabled => Maximum > 0;
        public void Validate()
        {
            if (Maximum < 0 || GroundRecoveryPerSecond < 0 || ActiveRecoveryPerSecond < 0 || RecoveryDelayFrames < 0 ||
                JumpCost < 0 || AirJumpCost < 0 || GroundDashCost < 0 || AirDashCost < 0 || DodgeCost < 0 ||
                GuardDrainPerSecond < 0 || NormalGuardCost < 0 || SpecialGuardCost < 0 || UltimateGuardCost < 0 ||
                GuardBreakFrames < 0 || GuardLockFrames < 0) throw new ArgumentException("Invalid stamina definition.");
        }
    }
    // One displayed point = 60 integer units. Rates integrate exactly at the fixed 60 Hz.
    public static class StaminaSystem
    {
        public const int UnitsPerPoint = 60;
        public static bool CanSpend(FighterState fighter, int cost) => !fighter.Definition.Stamina.Enabled || fighter.StaminaUnits >= cost * UnitsPerPoint;
        public static bool Spend(FighterState fighter, int cost, bool groundJumpRescue = false)
        {
            if (!fighter.Definition.Stamina.Enabled) return true;
            if (!CanSpend(fighter, cost) && !groundJumpRescue) { Reject(fighter); return false; }
            fighter.StaminaUnits = Math.Max(0, fighter.StaminaUnits - cost * UnitsPerPoint);
            fighter.StaminaDelayRemaining = fighter.Definition.Stamina.RecoveryDelayFrames;
            return true;
        }
        public static void Reject(FighterState fighter) => fighter.StaminaRejectedRemaining = 30;
        public static int GuardCost(FighterState fighter, AttackKind kind) => kind == AttackKind.Ultimate ? fighter.Definition.Stamina.UltimateGuardCost :
            kind == AttackKind.Special ? fighter.Definition.Stamina.SpecialGuardCost : fighter.Definition.Stamina.NormalGuardCost;
        public static void BreakGuard(FighterState fighter)
        {
            fighter.Mode = FighterMode.Recovery; fighter.Attack = null; fighter.DashRemaining = 0;
            fighter.GuardStunRemaining = 0;
            fighter.ActionFrames = Math.Max(fighter.ActionFrames, fighter.Definition.Stamina.GuardBreakFrames);
            fighter.GuardLockRemaining = fighter.Definition.Stamina.GuardLockFrames;
            fighter.Velocity = new BattleVector(0, fighter.Velocity.Y); Reject(fighter);
        }
        public static void Step(FighterState fighter)
        {
            if (fighter.StaminaRejectedRemaining > 0) fighter.StaminaRejectedRemaining--;
            if (fighter.GuardLockRemaining > 0) fighter.GuardLockRemaining--;
            var data = fighter.Definition.Stamina;
            if (!data.Enabled || fighter.HitstopRemaining > 0 || fighter.Mode == FighterMode.KO) return;
            if (fighter.Mode == FighterMode.Guard)
            {
                fighter.StaminaUnits = Math.Max(0, fighter.StaminaUnits - data.GuardDrainPerSecond);
                fighter.StaminaDelayRemaining = data.RecoveryDelayFrames; return;
            }
            if (fighter.StaminaDelayRemaining > 0) { fighter.StaminaDelayRemaining--; return; }
            bool resting = fighter.Grounded && (fighter.Mode == FighterMode.Idle || fighter.Mode == FighterMode.Crouch);
            fighter.StaminaUnits = Math.Min(data.Maximum * UnitsPerPoint, fighter.StaminaUnits +
                (resting ? data.GroundRecoveryPerSecond : data.ActiveRecoveryPerSecond));
        }
    }
}
