using System.Linq;
using UnityEngine;

namespace AniBato.Game
{
    public static class HudLayoutGroup
    {
        public static UiLayoutTarget[] Ensure(Transform canvas)
        {
            for (int player = 0; player < 2; player++)
            {
                string prefix = "P" + (player + 1);
                if (canvas.Find(prefix + " HUD Layout")) continue;
                var card = canvas.GetComponentsInChildren<PaperFighterHud>(true).FirstOrDefault(c => c.name == prefix + " Paper HUD");
                if (!card) continue;
                var group = new GameObject(prefix + " HUD Layout", typeof(RectTransform)); group.transform.SetParent(canvas, false);
                var rect = (RectTransform)group.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(player, 1);
                rect.sizeDelta = new Vector2(680, 420); rect.anchoredPosition = new Vector2(player == 0 ? 28 : -28, -18);
                rect.localScale = Vector3.one * (PaperFighterHud.CompactScale * 2);
                Move(card.transform, rect, player, 0);
                Move(canvas.Find("Player " + (player + 1) + " Special Cooldowns"), rect, player, -266);
                Move(canvas.Find(prefix + " Status Paper"), rect, player, -228);
                group.AddComponent<UiLayoutTarget>().Initialize("hud.p" + (player + 1), prefix + " ステータス", 1, addHitArea: true);
            }
            return canvas.GetComponentsInChildren<UiLayoutTarget>(true);
        }
        private static void Move(Transform child, RectTransform parent, int player, float y)
        {
            if (!child) return; child.SetParent(parent, false); var rect = (RectTransform)child;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(player, 1);
            rect.anchoredPosition = new Vector2(0, y); rect.localScale = Vector3.one;
        }
    }
}
