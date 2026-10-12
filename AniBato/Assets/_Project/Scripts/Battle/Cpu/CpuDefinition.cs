using System;

namespace AniBato.Battle
{
    [Serializable]
    public struct CpuDefinition
    {
        public int ReactionFrames, DecisionFrames;
        public int SchemaVersion, AttackRestFrames, WhiffApproachAttempts;
        public float AttackChance, SpecialChance, ComboChance, GuardChance, DodgeChance, CounterChance, JustChance, JumpChance;
        public float EdgeMargin, CloseDistance, MediumDistance, RetreatDistance;
        public void Validate()
        {
            if (ReactionFrames < 0 || DecisionFrames < 1 || AttackRestFrames < 0 || WhiffApproachAttempts < 0 || EdgeMargin < 1 || CloseDistance <= 0 ||
                MediumDistance <= CloseDistance || RetreatDistance <= 0) throw new ArgumentException("CPU距離・反応定義が不正です。");
            foreach (float value in new[] { AttackChance, SpecialChance, ComboChance, GuardChance, DodgeChance, CounterChance, JustChance, JumpChance })
                if (float.IsNaN(value) || value < 0 || value > 1) throw new ArgumentException("CPU確率は0〜1です。");
        }
        public static CpuDefinition ForDifficulty(int difficulty)
        {
            var definition = LegacyDifficulty(difficulty); definition.SchemaVersion = 5;
            definition.AttackRestFrames = difficulty == 0 ? 105 : difficulty == 1 ? 120 : 90;
            definition.WhiffApproachAttempts = difficulty == 2 ? 8 : 12;
            definition.GuardChance = difficulty == 0 ? .25f : difficulty == 1 ? .82f : .94f;
            definition.DodgeChance = difficulty == 0 ? .12f : difficulty == 1 ? .38f : .62f;
            definition.CounterChance = difficulty == 0 ? .06f : difficulty == 1 ? .32f : .55f;
            definition.JustChance = difficulty == 0 ? .02f : difficulty == 1 ? .18f : .55f;
            definition.SpecialChance = difficulty == 0 ? .1f : .14f;
            return definition;
        }
        public static CpuDefinition LegacyDifficulty(int difficulty)
        {
            if (difficulty < 0 || difficulty > 2) throw new ArgumentOutOfRangeException(nameof(difficulty));
            return new CpuDefinition
            {
                ReactionFrames = difficulty == 0 ? 18 : difficulty == 1 ? 8 : 3,
                DecisionFrames = difficulty == 0 ? 10 : difficulty == 1 ? 5 : 3,
                AttackChance = difficulty == 0 ? .65f : .9f, SpecialChance = difficulty == 0 ? .12f : .22f,
                ComboChance = difficulty == 0 ? .3f : difficulty == 1 ? .8f : .95f,
                GuardChance = difficulty == 0 ? .12f : difficulty == 1 ? .5f : .7f,
                DodgeChance = difficulty == 0 ? .08f : difficulty == 1 ? .25f : .45f,
                CounterChance = difficulty == 0 ? .05f : difficulty == 1 ? .22f : .4f,
                JustChance = difficulty == 0 ? .02f : difficulty == 1 ? .12f : .3f,
                JumpChance = .05f, EdgeMargin = 2.2f, CloseDistance = 2.5f, MediumDistance = 7, RetreatDistance = 1.2f
            };
        }
    }
    public interface ICpuBrain { FrameInput NextInput(BattleSimulation simulation, int player); }
}
