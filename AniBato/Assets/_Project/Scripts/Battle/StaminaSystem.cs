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
        public int NormalProjectileCost, SpecialProjectileCost;
        public int NormalFirstCost, NormalSecondCost, NormalThirdCost, DirectionalNormalCost;
        public int ChargeMinimumCost, ChargeMaximumCost;
        public bool Enabled => Maximum > 0;
        public void Validate()
        {
            if (Maximum < 0 || GroundRecoveryPerSecond < 0 || ActiveRecoveryPerSecond < 0 || RecoveryDelayFrames < 0 ||
                JumpCost < 0 || AirJumpCost < 0 || GroundDashCost < 0 || AirDashCost < 0 || DodgeCost < 0 ||
                GuardDrainPerSecond < 0 || NormalGuardCost < 0 || SpecialGuardCost < 0 || UltimateGuardCost < 0 ||
                GuardBreakFrames < 0 || GuardLockFrames < 0 || NormalProjectileCost < 0 || SpecialProjectileCost < 0 ||
                NormalFirstCost < 0 || NormalSecondCost < 0 || NormalThirdCost < 0 || DirectionalNormalCost < 0 ||
                ChargeMinimumCost < 0 || ChargeMaximumCost < ChargeMinimumCost) throw new ArgumentException("Invalid stamina definition.");
        }
    }
    // One displayed point = 60 integer units. Rates integrate exactly at the fixed 60 Hz.
    public static class StaminaSystem
    {
        public const int UnitsPerPoint = 60;
        public static bool CanSpend(FighterState fighter, int cost) => SupportStatus.Infinite(fighter) || !fighter.Definition.Stamina.Enabled || fighter.StaminaUnits >= cost * UnitsPerPoint;
        public static bool Spend(FighterState fighter, int cost, bool groundJumpRescue = false)
        {
            if (!fighter.Definition.Stamina.Enabled || SupportStatus.Infinite(fighter)) return true;
            if (!CanSpend(fighter, cost) && !groundJumpRescue) { Reject(fighter); return false; }
            fighter.StaminaUnits = Math.Max(0, fighter.StaminaUnits - cost * UnitsPerPoint);
            fighter.StaminaDelayRemaining = fighter.Definition.Stamina.RecoveryDelayFrames;
            return true;
        }
        public static void Reject(FighterState fighter) => fighter.StaminaRejectedRemaining = 30;
        public static int GuardCost(FighterState fighter, AttackKind kind) => kind == AttackKind.Ultimate ? fighter.Definition.Stamina.UltimateGuardCost :
            kind == AttackKind.Special ? fighter.Definition.Stamina.SpecialGuardCost : fighter.Definition.Stamina.NormalGuardCost;
        public static int NormalCost(FighterState fighter, in AttackDefinition attack, int chain, bool air, int chargeFrames, in ChargeAttackDefinition charge)
        {
            var s=fighter.Definition.Stamina;
            int cost=air || attack.NormalDirection!=NormalDirection.Weak ? s.DirectionalNormalCost :
                chain>=2?s.NormalThirdCost:chain==1?s.NormalSecondCost:s.NormalFirstCost;
            if(charge.Enabled && chargeFrames>=charge.MinimumFrames)
                cost=Math.Max(cost,s.ChargeMinimumCost+(int)Math.Ceiling((s.ChargeMaximumCost-s.ChargeMinimumCost)*charge.Strength(chargeFrames)));
            return Math.Max(cost,ProjectileCost(fighter,attack));
        }
        public static int ProjectileCost(FighterState fighter, in AttackDefinition attack) =>
            !attack.SpawnProjectile || attack.IsUltimate || attack.Ability.ProjectileChannel != ProjectileChannel.Shared ? 0 :
            attack.IsNormal ? fighter.Definition.Stamina.NormalProjectileCost : fighter.Definition.Stamina.SpecialProjectileCost;
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
            if (SupportStatus.Infinite(fighter) || !data.Enabled || fighter.HitstopRemaining > 0 || fighter.Mode == FighterMode.KO || LedgeSystem.Attached(fighter)) return;
            if (fighter.Mode == FighterMode.Guard)
            {
                fighter.StaminaUnits = Math.Max(0, fighter.StaminaUnits - data.GuardDrainPerSecond);
                fighter.StaminaDelayRemaining = data.RecoveryDelayFrames; return;
            }
            // In the current economy, a committed attack cannot regenerate its own cost.
            // The delay begins after its last recovery tick. Legacy fixtures retain their old rates.
            if(data.NormalFirstCost>0 && (fighter.Attack!=null || fighter.Mode==FighterMode.Charging))
            {fighter.StaminaDelayRemaining=data.RecoveryDelayFrames;return;}
            if (fighter.StaminaDelayRemaining > 0) { fighter.StaminaDelayRemaining--; return; }
            bool resting = fighter.Grounded && (fighter.Mode == FighterMode.Idle || fighter.Mode == FighterMode.Crouch ||
                data.NormalFirstCost>0 && fighter.Mode==FighterMode.Move);
            fighter.StaminaUnits = Math.Min(data.Maximum * UnitsPerPoint, fighter.StaminaUnits +
                (resting ? data.GroundRecoveryPerSecond : data.ActiveRecoveryPerSecond));
        }
    }
}
