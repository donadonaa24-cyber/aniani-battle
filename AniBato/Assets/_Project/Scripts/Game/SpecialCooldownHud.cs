using AniBato.Battle;
using AniBato.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class SpecialCooldownHud : MonoBehaviour
    {
        [SerializeField] private Text[] labels = new Text[5];
        [SerializeField] private Image[] bars = new Image[5], cards = new Image[5];
        private readonly SpecialSlot[] slots = new SpecialSlot[4];
        [SerializeField] private BattleRunner runner;
        [SerializeField] private int player;
        private const float Width = 680, Gap = 10;
        public void Configure(BattleRunner value, int index)
        {
            runner = value; player = index;
            for (int i = 0; i < cards.Length; i++)
            {
                var card = new GameObject("Cooldown " + i, typeof(RectTransform), typeof(Image));
                card.transform.SetParent(transform, false); cards[i] = card.GetComponent<Image>(); cards[i].raycastTarget = false;
                var text = new GameObject("P" + (player + 1) + " Special Status " + i, typeof(RectTransform), typeof(Text));
                text.transform.SetParent(card.transform, false); labels[i] = text.GetComponent<Text>();
                labels[i].font = RuntimeFonts.Japanese; labels[i].fontSize = 20; labels[i].alignment = TextAnchor.UpperCenter;
                labels[i].raycastTarget = false; labels[i].supportRichText = false;
                var rect = labels[i].rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(3, 12); rect.offsetMax = new Vector2(-3, -5);
                var bar = new GameObject("Cooldown Progress", typeof(RectTransform), typeof(Image));
                bar.transform.SetParent(card.transform, false); bars[i] = bar.GetComponent<Image>(); bars[i].raycastTarget = false;
                bars[i].rectTransform.anchorMin = bars[i].rectTransform.anchorMax = bars[i].rectTransform.pivot = new Vector2(0, 0);
                bars[i].rectTransform.anchoredPosition = new Vector2(6, 5);
            }
        }
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            if (runner == null || runner.Simulation == null) return;
            var sim = runner.Simulation; var f = player == 0 ? sim.P1 : sim.P2; var policy = sim.Rules.CombatInput;
            int count = 0; slots[count++] = SpecialSlot.Neutral;
            if (!policy.SingleSpecial) slots[count++] = SpecialSlot.Side;
            if (policy.DownSpecialEnabled || !policy.SingleSpecial) slots[count++] = SpecialSlot.Down;
            if (policy.UpRecoveryEnabled || !policy.SingleSpecial) slots[count++] = SpecialSlot.Up;
            float width = (Width - Gap * count) / (count + 1);
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].gameObject.SetActive(i <= count); if (i > count) continue;
                var rect = cards[i].rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(i * (width + Gap), 0); rect.sizeDelta = new Vector2(width, 120);
                bool ready; float progress; string title, state;
                if (i == count)
                {
                    title = "超必殺"; ready = GaugeSystem.CanUseUltimate(f, sim.Rules);
                    state = f.UnlimitedUltimates ? "OK・無制限" : ready ? "OK" : f.UltimateUses >= sim.Rules.Gauge.UltimateMaxUses ? "使用済み" :
                        "ゲージ " + f.Gauge + "/" + sim.Rules.Gauge.UltimateCost;
                    progress = f.UnlimitedUltimates ? 1 : Mathf.Clamp01(f.Gauge / (float)sim.Rules.Gauge.UltimateCost);
                }
                else
                {
                    var slot = slots[i]; title = slot == SpecialSlot.Neutral ? policy.SingleSpecial ? "N/横 特殊" : "N 特殊" :
                        slot == SpecialSlot.Side ? "横 特殊" : slot == SpecialSlot.Down ? "↓ 特殊" : "↑ 復帰";
                    ready = SpecialCooldownDisplay.Ready(f, slot); state = SpecialCooldownDisplay.State(f, slot);
                    int total = f.Definition.Special(slot).CooldownFrames;
                    progress = total == 0 ? 1 : Mathf.Clamp01(1 - f.SpecialCooldown(slot) / (float)total);
                    if (slot == SpecialSlot.Down && f.Definition.Special(slot).Ability.PlacementLimit > 0)
                        state += "\n設置 " + f.PlacedCount + "/" + f.Definition.Special(slot).Ability.PlacementLimit;
                    if (slot == SpecialSlot.Neutral)
                    {
                        var march = System.Linq.Enumerable.FirstOrDefault(sim.Projectiles,p=>p.Owner==f&&p.RoyalMarch);
                        if (march != null) state += "\n" + (march.MarchExitAge>=0?"退場中":march.MarchCommand?"号令":march.CanDamage?"行進中":"登場中");
                    }
                }
                labels[i].text = title + "\n" + state;
                if (f.Attack != null && ((i == count && f.Attack.Definition.IsUltimate) ||
                    i < count && f.Attack.Definition.IsSpecial && slots[i] == f.CurrentSpecial))
                {
                    bool teleport = RecoveryMotionSystem.IsCharging(f);
                    if (teleport || AbilityChargeSystem.IsCharging(f))
                    {
                        int frames = teleport ? f.Attack.RecoveryChargeFrames : f.Attack.AbilityChargeFrames;
                        int maximum = teleport ? f.Attack.Definition.Recovery.MaximumChargeFrames : f.Attack.Definition.AbilityCharge.MaximumFrames;
                        labels[i].text = title + "\n溜め " + SpecialCooldownDisplay.Seconds(frames) + "\n離して発動";
                        progress = Mathf.Clamp01(frames/(float)maximum); ready = false;
                    }
                }
                labels[i].color = ready ? new Color(.65f, 1, .8f) : new Color(1, .85f, .58f);
                cards[i].color = new Color(.035f, .055f, .10f, .9f);
                bars[i].color = ready ? new Color(.3f, .9f, .65f) : new Color(.45f, .65f, 1);
                bars[i].rectTransform.sizeDelta = new Vector2((width - 12) * progress, 5);
            }
        }
    }
}
