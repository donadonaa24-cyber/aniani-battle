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
        private static string Slot(string label, FighterState fighter, SpecialSlot slot)
        {
            int ticks = fighter.SpecialCooldown(slot);
            string state = ticks > 0 ? (Math.Ceiling(ticks * 10d / BattleRuleDefinition.TickRate) / 10).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "秒" :
                slot == SpecialSlot.Up && fighter.UpSpecialUsed ? "着地待ち" : "OK";
            return label + " " + state;
        }
    }
}
