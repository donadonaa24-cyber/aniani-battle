using System;

namespace AniBato.Battle
{
    [Serializable]
    public struct DefenseDefinition
    {
        public int GuardReleaseFrames, GuardStunFrames, JustWindowFrames, JustGuardStunFrames;
        public float GuardPushSpeed;
        public int DodgeInvulnerableFrames, DodgeRecoveryFrames, JustDodgeRecoveryFrames;
        public float GroundDodgeSpeed, AirDodgeSpeed;
        public int CounterWindowFrames, CounterFailureFrames, CounterSuccessFrames, CounterCooldownFrames;
        public int CounterTargetStunFrames, CounterTargetImmunityFrames, JustAttackerStunFrames;
        public BattleVector CounterKnockback;
        public void Validate()
        {
            if (GuardReleaseFrames < 0 || GuardStunFrames < 1 || JustWindowFrames < 1 || JustGuardStunFrames < 0 ||
                GuardPushSpeed < 0 || DodgeInvulnerableFrames < JustWindowFrames || DodgeRecoveryFrames < 1 ||
                JustDodgeRecoveryFrames < 0 || GroundDodgeSpeed < 0 || AirDodgeSpeed < 0 || CounterWindowFrames < 1 ||
                CounterFailureFrames < 1 || CounterSuccessFrames < 1 || CounterCooldownFrames < 1 ||
                CounterTargetStunFrames < 1 || CounterTargetImmunityFrames < 1 || JustAttackerStunFrames < 1)
                throw new ArgumentException("防御の定義が不正です。");
        }
    }
}
