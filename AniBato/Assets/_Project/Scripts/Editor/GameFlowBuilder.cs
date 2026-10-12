using AniBato.Game;
using UnityEngine;
using UnityEngine.UI;

namespace AniBato.EditorTools
{
    public static class GameFlowBuilder
    {
        public static void Build(BattleRunner runner, InputController input, BattleSetup setup, GameFlowSettings settings,
            GameObject world, GameObject hud, StageView stage)
        {
            var canvas = new GameObject("Game Screens", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<Canvas>().sortingOrder = 20;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            var root = new GameObject("Menu Viewport", typeof(RectTransform)); root.transform.SetParent(canvas.transform, false);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
            var view = root.AddComponent<MenuScreenView>();
            Image shade = Picture(root.transform, "Screen Background", Vector2.zero, new Vector2(5000, 5000));
            var title = Label(root.transform, "Screen Title", new Vector2(0, 432), new Vector2(1740, 120), 62);
            var subtitle = Label(root.transform, "Screen Subtitle", new Vector2(0, 342), new Vector2(1740, 70), 28);
            var options = Label(root.transform, "Screen Options", Vector2.zero, new Vector2(1740, 420), 38);
            var footer = Label(root.transform, "Screen Help", new Vector2(0, -455), new Vector2(1770, 112), 24);
            footer.color = new Color(0.65f, 0.73f, 0.84f);
            var banner = Label(root.transform, "Battle Banner", Vector2.zero, new Vector2(1740, 200), 110);
            banner.color = new Color(1, 0.76f, 0.35f);
            var cards = new Image[3]; var labels = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                cards[i] = Picture(root.transform, "Selection Picture " + i, Vector2.zero, new Vector2(500, 360)); cards[i].preserveAspect = true;
                labels[i] = Label(root.transform, "Selection Label " + i, Vector2.zero, new Vector2(550, 140), 30);
            }
            var columns = new Text[4];
            float[] positions = { -730, -370, 0, 500 }; float[] widths = { 260, 320, 380, 500 };
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = Label(root.transform, "Control Table Column " + i, new Vector2(positions[i], 105), new Vector2(widths[i], 420), 27);
                columns[i].alignment = TextAnchor.MiddleLeft; columns[i].lineSpacing = 1.25f;
            }
            view.Configure(shade, title, subtitle, options, footer, banner, cards, labels, columns);
            var flow = runner.gameObject.AddComponent<GameFlowController>();
            flow.Configure(runner, input, setup, settings, view, world, hud, stage);
            world.SetActive(false); hud.SetActive(false);
        }
        private static Image Picture(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.raycastTarget = false; return image;
        }
        private static Text Label(Transform parent, string name, Vector2 position, Vector2 size, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = size;
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize; text.color = Color.white; text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false; text.supportRichText = false; return text;
        }
    }
}
