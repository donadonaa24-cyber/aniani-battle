using System;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    /// <summary>
    /// 2026-10-15 user request: normal-attack hitboxes reached too far to fight up close.
    /// Halves the horizontal width of every playable character's normal-attack hitbox while keeping the
    /// edge nearest the fighter fixed (only the side that reaches toward the opponent shrinks).
    /// One-shot: run with -executeMethod AniBato.EditorTools.HitboxTuning.HalveNormalReach.
    /// </summary>
    public static class HitboxTuning
    {
        private const string DataRoot = "Assets/_Project/Data";
        private static readonly string[] Characters = { "Touma", "Ren", "Chizuru" };

        public static void HalveNormalReach()
        {
            var report = new System.Text.StringBuilder("asset,part,beforeLeft,beforeRight,afterLeft,afterRight\n");
            int changed = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:AttackData", new[] { DataRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                if (!Characters.Any(c => name.StartsWith(c, StringComparison.Ordinal))) continue;
                var data = AssetDatabase.LoadAssetAtPath<AttackData>(path);
                if (!data || !data.Definition.IsNormal || data.Definition.SpawnProjectile) continue;
                var definition = data.Definition;
                definition.Hitbox = HalveKeepingNearEdge(definition.Hitbox, name, "main", report);
                if (definition.HasSecondaryHitbox)
                    definition.SecondaryHitbox = HalveKeepingNearEdge(definition.SecondaryHitbox, name, "secondary", report);
                definition.Validate();
                data.Definition = definition;
                EditorUtility.SetDirty(data);
                changed++;
            }
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("../TestResults");
            File.WriteAllText("../TestResults/hitbox_halve_report.csv", report.ToString());
            Debug.Log("HitboxTuning: halved normal-attack reach on " + changed + " assets.");
        }

        /// <summary>
        /// Correction for the first pass (2026-10-15), which anchored centered boxes at their back edge.
        /// Restores those rows from the saved pass-1 "before" values and re-applies the centered rule.
        /// </summary>
        /// <summary>Restores ultimate frequency after the reach change (fewer hits => less gauge). Gauge: OnHit 12->14, OnDamaged 8->10.</summary>
        public static void RaiseGaugeAfterReachHalving()
        {
            var rules = AssetDatabase.LoadAssetAtPath<BattleRules>(DataRoot + "/BattleRules.asset");
            var definition = rules.Definition;
            definition.Gauge.OnHit = 14; definition.Gauge.OnDamaged = 10;
            rules.Definition = definition; EditorUtility.SetDirty(rules); AssetDatabase.SaveAssets();
            Debug.Log("HitboxTuning: gauge OnHit=14 OnDamaged=10");
        }

        public static void FixCenteredFromPassOne()
        {
            var lines = File.ReadAllLines("../TestResults/hitbox_halve_report_pass1.csv").Skip(1);
            int fixedCount = 0;
            foreach (var line in lines)
            {
                var cells = line.Split(',');
                if (cells.Length < 4 || cells[1] != "main") continue;
                float left = float.Parse(cells[2], System.Globalization.CultureInfo.InvariantCulture);
                float right = float.Parse(cells[3], System.Globalization.CultureInfo.InvariantCulture);
                if (!(right > 0f && left < -CenteredBackReach)) continue;
                var data = AssetDatabase.LoadAssetAtPath<AttackData>(DataRoot + "/" + cells[0] + ".asset");
                var definition = data.Definition;
                var original = new BattleRect(new BattleVector((left + right) * 0.5f, definition.Hitbox.Center.Y),
                    new BattleVector(right - left, definition.Hitbox.Size.Y));
                definition.Hitbox = HalveKeepingNearEdge(original, cells[0], "main", null);
                data.Definition = definition; EditorUtility.SetDirty(data); fixedCount++;
                Debug.Log($"HitboxTuning fix: {cells[0]} {original.Left:0.###}..{original.Right:0.###} -> {definition.Hitbox.Left:0.###}..{definition.Hitbox.Right:0.###}");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("HitboxTuning: fixed centered boxes on " + fixedCount + " assets.");
        }

        private const float CenteredBackReach = HitboxReach.CenteredBackReach;

        // Rect is in facing-forward local space (+X = toward the opponent when facing right).
        // Keep the edge closer to the fighter: the left edge, unless the box lies entirely behind (back attacks).
        internal static BattleRect HalveKeepingNearEdge(BattleRect rect, string name, string part, System.Text.StringBuilder report)
        {
            var result = HitboxReach.HalveTowardOpponent(rect);
            report?.AppendLine($"{name},{part},{rect.Left:0.###},{rect.Right:0.###},{result.Left:0.###},{result.Right:0.###}");
            return result;
        }
    }
}
