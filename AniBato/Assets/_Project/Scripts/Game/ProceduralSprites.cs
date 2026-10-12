using UnityEngine;

namespace AniBato.Game
{
    public static class ProceduralSprites
    {
        private static Sprite square;
        private static Sprite round;
        private static Material material;
        public static Material Material
        {
            get
            {
                if (!material) material = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };
                return material;
            }
        }
        public static Sprite Shape(bool circle = false)
        {
            if (circle ? round : square) return circle ? round : square;
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
            var colors = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                colors[y * size + x] = !circle || new Vector2(x - 15.5f, y - 15.5f).sqrMagnitude <= 240 ? Color.white : Color.clear;
            texture.SetPixels(colors); texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            if (circle) round = sprite; else square = sprite;
            return sprite;
        }
        public static SpriteRenderer Create(Transform parent, string name, Vector2 size, Color tint, bool circle = false, int sortingOrder = 0)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Shape(circle); renderer.sharedMaterial = Material; renderer.color = tint; renderer.sortingOrder = sortingOrder;
            return renderer;
        }
    }
}
