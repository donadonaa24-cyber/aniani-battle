using UnityEngine;
using System.Collections.Generic;

namespace AniBato.Game
{
    // Original placeholder scenery made entirely from our own shapes, with two parallax layers.
    public sealed class StageBackgroundView : MonoBehaviour
    {
        private Transform far, mid;
        private StageData data;
        private Camera cameraComponent;
        private readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        private readonly List<Color> colors = new List<Color>();
        private int dimStep = -1;
        public void Build(StageData stage)
        {
            data = stage; cameraComponent = Camera.main;
            far = new GameObject("Far Scenery").transform; far.SetParent(transform, false);
            mid = new GameObject("Mid Scenery").transform; mid.SetParent(transform, false);
            if (cameraComponent) cameraComponent.backgroundColor = data.Background;
            if (data.Theme == StageTheme.FutureCity) City();
            else if (data.Theme == StageTheme.BoardArena) Board();
            else Festival();
        }
        private void Shape(Transform parent, string name, float x, float y, float w, float h, Color color, bool round = false)
        {
            var sprite = ProceduralSprites.Create(parent, name, new Vector2(w, h), color, round, parent == far ? -30 : -20);
            sprite.transform.localPosition = new Vector3(x, y, 0);
            sprites.Add(sprite); colors.Add(color);
        }
        public void SetShrinkStep(int step, int total)
        {
            if (data.Theme != StageTheme.SchoolFestival || step == dimStep) return;
            dimStep = step;
            float brightness = 1 - step / (float)Mathf.Max(1, total) * 0.45f;
            for (int i = 0; i < sprites.Count; i++)
            { var c = colors[i]; sprites[i].color = new Color(c.r * brightness, c.g * brightness, c.b * brightness, c.a); }
        }
        private void City()
        {
            for (int i = -8; i <= 8; i++)
            {
                float x = i * 5, h = 5 + (i * i % 5);
                Shape(far, "Skyline", x, h * 0.5f + 2, 3.7f, h, data.FarColor);
                for (int j = 0; j < 3; j++) Shape(far, "Window Band", x, 3 + j * 2, 2.6f, 0.08f, data.MidColor);
            }
            Shape(far, "Launch Tower", 13, 10, 1.2f, 16, data.MidColor);
            Shape(far, "Tower Halo", 13, 14, 5, 0.2f, data.Accent * 0.6f);
            for (int i = -5; i <= 5; i++) Shape(mid, "Data Conduit", i * 8, 1.3f, 6, 0.08f, data.MidColor);
        }
        private void Board()
        {
            for (int level = 0; level < 5; level++)
            {
                Shape(far, "Tier Tower", -20, 2 + level * 1.4f, 8 - level, 1, data.FarColor);
                Shape(far, "Tier Tower", 20, 2 + level * 1.4f, 8 - level, 1, data.FarColor);
            }
            for (int i = 0; i < 6; i++) for (int j = 0; j < 6; j++)
                Shape(mid, "6x6 Arena Tile", -15 + i * 6, -4 + j * 1.7f, 5.8f, 1.55f,
                    (i + j) % 2 == 0 ? data.FarColor : data.MidColor);
            foreach (float x in new[] { -24f, 24f })
            {
                Shape(mid, "Giant Piece Base", x, 2, 4, 1, data.MidColor);
                Shape(mid, "Giant Piece Body", x, 4, 2.8f, 4, data.MidColor);
                Shape(mid, "Giant Piece Crown", x, 6.5f, 3.6f, 1.6f, data.MidColor, true);
            }
        }
        private void Festival()
        {
            for (int i = -4; i <= 4; i++)
            {
                float x = i * 9;
                Shape(far, "Kitchen Stall", x, 3, 7.5f, 4, data.FarColor);
                Shape(mid, "Stall Awning", x, 5.5f, 8, 0.7f, data.MidColor);
                Shape(mid, "Kitchen Counter", x, 1.5f, 8, 0.8f, data.MidColor);
                Shape(mid, "Cooking Pot", x - 2, 2.4f, 1.4f, 1, data.MidColor, true);
                Shape(mid, "Plate", x + 2, 2.3f, 1.8f, 0.2f, data.Accent * 0.6f);
                Shape(far, "Decoration String", x, 9, 9, 0.08f, data.MidColor);
                for (int flag = 0; flag < 4; flag++) Shape(far, "Festival Flag", x - 3 + flag * 2, 8.5f, 0.7f, 0.8f, data.MidColor);
            }
        }
        private void LateUpdate()
        {
            if (!cameraComponent || !data) return;
            var p = cameraComponent.transform.position;
            far.localPosition = new Vector3(p.x * data.FarParallax, p.y * data.FarParallax, 0);
            mid.localPosition = new Vector3(p.x * data.MidParallax, p.y * data.MidParallax, 0);
        }
    }
}
