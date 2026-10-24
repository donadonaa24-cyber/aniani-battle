using UnityEngine;

namespace AniBato.Game
{
    public static class MobileControlArt
    {
        private static Sprite circle;
        public static Sprite Circle
        {
            get
            {
                if (circle) return circle;
                const int size = 128;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) {
                    filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    float alpha = Mathf.Clamp01(62.5f - new Vector2(x - 63.5f, y - 63.5f).magnitude);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255));
                }
                texture.SetPixels32(pixels); texture.Apply();
                circle = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size, 0, SpriteMeshType.FullRect);
                circle.hideFlags = HideFlags.HideAndDontSave; return circle;
            }
        }
    }
}
