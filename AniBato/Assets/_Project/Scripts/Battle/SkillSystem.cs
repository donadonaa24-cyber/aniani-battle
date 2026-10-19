using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    [Serializable]
    public struct BuffDefinition
    {
        public int DurationFrames;
        public float MoveMultiplier;
        public float DashMultiplier;
        public float StartupMultiplier;
        public void Validate()
        {
            if (DurationFrames < 0 || MoveMultiplier < 0 || DashMultiplier < 0 || StartupMultiplier < 0)
                throw new ArgumentException("強化の定義が不正です。");
        }
    }

    [Serializable]
    public struct CounterEffectDefinition
    {
        public BuffDefinition Buff;
        public int SelfHealNormalHits;
        public float KnockbackMultiplier;
        public void Validate()
        {
            Buff.Validate();
            if (SelfHealNormalHits < 0 || KnockbackMultiplier < 0) throw new ArgumentException("カウンター効果が不正です。");
        }
    }

    // Effects are data driven, and never refer to character names or Unity assets.
    public static class SkillSystem
    {
        public const int MaxParalysisFrames = 45;
        internal static float Multiplier(float value) => value > 0 ? value : 1;
        internal static void Step(FighterState fighter)
        {
            if (fighter.BuffRemaining > 0) fighter.BuffRemaining--;
            if (fighter.MarkRemaining > 0) fighter.MarkRemaining--;
            for (int i = 0; i < fighter.SpecialCooldowns.Length; i++)
                if (fighter.SpecialCooldowns[i] > 0) fighter.SpecialCooldowns[i]--;
        }
        public static void Heal(FighterState fighter, int amount) => fighter.NormalHitCount = Math.Max(0, fighter.NormalHitCount - amount);
        public static void Buff(FighterState fighter, in BuffDefinition buff)
        {
            if (buff.DurationFrames == 0) return;
            fighter.Buff = buff; fighter.BuffRemaining = buff.DurationFrames;
        }
        internal static void OnActive(FighterState fighter)
        {
            var attack = fighter.Attack;
            if (attack == null || !attack.IsActive || attack.SelfEffectApplied || attack.Definition.SelfEffectsOnComplete) return;
            attack.SelfEffectApplied = true;
            Heal(fighter, attack.Definition.SelfHealNormalHits);
            Buff(fighter, attack.Definition.SelfBuff);
        }
        internal static void OnComplete(FighterState fighter, AttackInstance attack)
        {
            if (!attack.Definition.SelfEffectsOnComplete || attack.SelfEffectApplied) return;
            attack.SelfEffectApplied = true;
            Heal(fighter, attack.Definition.SelfHealNormalHits); Buff(fighter, attack.Definition.SelfBuff);
        }
        internal static void OnHit(FighterState target, in AttackDefinition definition)
        {
            target.ParalysisRemaining = Math.Max(target.ParalysisRemaining, Math.Min(MaxParalysisFrames, definition.ParalysisFrames));
            target.MarkRemaining = Math.Max(target.MarkRemaining, definition.MarkFrames);
        }
    }

    public sealed class DataCounterEffect : ICounterEffect
    {
        private readonly CounterEffectDefinition definition;
        public DataCounterEffect(in CounterEffectDefinition value) { value.Validate(); definition = value; }
        public void OnSuccess(FighterState defender, FighterState attacker, int tick, in BattleRuleDefinition rules, List<BattleEvent> events)
        {
            SkillSystem.Heal(defender, definition.SelfHealNormalHits);
            SkillSystem.Buff(defender, definition.Buff);
            attacker.Velocity *= SkillSystem.Multiplier(definition.KnockbackMultiplier);
        }
    }
}
