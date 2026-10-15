using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    [Serializable]
    public struct GaugeDefinition
    {
        public int Maximum, UltimateCost, UltimateMaxUses;
        public int OnHit, OnDamaged, OnCounter, OnJustGuard, OnJustDodge;
        public void Validate()
        {
            if (Maximum < 1 || UltimateCost < 1 || UltimateCost > Maximum || UltimateMaxUses < 1 ||
                OnHit < 0 || OnDamaged < 0 || OnCounter < 0 || OnJustGuard < 0 || OnJustDodge < 0)
                throw new ArgumentException("ゲージの定義が不正です。");
        }
    }

    public static class GaugeSystem
    {
        public static void Add(FighterState fighter, int amount, int tick, in BattleRuleDefinition rules, List<BattleEvent> events)
        {
            int previous = fighter.Gauge;
            fighter.Gauge = Math.Min(rules.Gauge.Maximum, fighter.Gauge + amount);
            if (previous != fighter.Gauge)
                events.Add(new BattleEvent(BattleEventKind.GaugeChanged, tick, fighter.PlayerIndex, fighter.PlayerIndex, value: fighter.Gauge));
        }
        public static bool CanUseUltimate(FighterState fighter, in BattleRuleDefinition rules) =>
            fighter.UnlimitedUltimates || fighter.Gauge >= rules.Gauge.UltimateCost && fighter.UltimateUses < rules.Gauge.UltimateMaxUses;
    }
}
