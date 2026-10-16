using UnityEngine;

namespace AniBato.Game
{
    public static class FramePacing
    {
        private static readonly int[] rates = { -1, 60, 120, 144, -1 };
        private static readonly string[] labels = { "モニター同期", "60", "120", "144", "無制限" };
        public static int Mode { get; private set; }
        public static string Label => WebInputPolicy.Enabled?"ブラウザに同期":labels[Mode];
        public static void Load() => Apply(Mathf.Clamp(PlayerPrefs.GetInt("AniBato.FramePacing", 0), 0, 4));
        public static void Cycle(int direction)
        { if(WebInputPolicy.Enabled)return;Apply((Mode + direction + 5) % 5); PlayerPrefs.SetInt("AniBato.FramePacing", Mode);PlayerPrefs.Save(); }
        private static void Apply(int mode)
        { Mode = mode; QualitySettings.vSyncCount = mode == 0 ? 1 : 0; Application.targetFrameRate = WebInputPolicy.Enabled?-1:rates[mode]; }
    }
}
