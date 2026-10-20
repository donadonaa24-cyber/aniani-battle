using System;
using AniBato.Battle;

namespace AniBato.Presentation
{
    public static class SpecialCooldownDisplay
    {
        public static string Text(FighterState fighter, in CombatInputDefinition policy)
        {
            string result = Slot("N", fighter, SpecialSlot.Neutral);
            if (policy.DownSpecialEnabled) result += "   " + Slot("↓", fighter, SpecialSlot.Down);
            if (policy.UpRecoveryEnabled) result += "   " + Slot("↑", fighter, SpecialSlot.Up);
            int limit = fighter.Definition.Special(SpecialSlot.Down).Ability.PlacementLimit;
            if (policy.DownSpecialEnabled && limit > 0) result += "   設置 " + fighter.PlacedCount + "/" + limit;
            return result;
        }
        public static string State(FighterState fighter, SpecialSlot slot)
        {
            int ticks = fighter.SpecialCooldown(slot);
            string state = ticks > 0 ? Seconds(ticks) :
                slot == SpecialSlot.Up && fighter.UpSpecialUsed ? "着地待ち" : "OK";
            if (slot == SpecialSlot.Down)
            {
                var ability = fighter.Definition.Special(slot).Ability;
                if (ability.PlacementLimit > 0 && fighter.PlacedCount >= ability.PlacementLimit) return state + "\n設置上限";
                if (ability.GroundOnly && !fighter.Grounded) return state + "\n地上専用";
            }
            if (slot == SpecialSlot.Up && fighter.UpSpecialUsed && ticks > 0) state += "\n要着地";
            return state;
        }
        public static string Seconds(int ticks) => (Math.Ceiling(ticks * 10d / BattleRuleDefinition.TickRate) / 10)
            .ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "秒";
        public static bool Ready(FighterState f, SpecialSlot slot) => f.SpecialCooldown(slot) == 0 &&
            (slot != SpecialSlot.Up || !f.UpSpecialUsed) &&
            (!f.Definition.Special(slot).Ability.GroundOnly || f.Grounded) &&
            (f.Definition.Special(slot).Ability.PlacementLimit == 0 || f.PlacedCount < f.Definition.Special(slot).Ability.PlacementLimit);
        private static string Slot(string label, FighterState fighter, SpecialSlot slot) => label + " " + State(fighter, slot);
    }
}
