using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    public enum StageTheme { FutureCity, BoardArena, SchoolFestival }
    [CreateAssetMenu(menuName = "あにバト/ステージ")]
    public sealed class StageData : ScriptableObject
    {
        public StageGeometryData SharedGeometry;
        public string DisplayName;
        public StageTheme Theme;
        public string TimeVariant = "夜";
        public string BgmKey;
        public string ShrinkMessage;
        public Sprite Thumbnail;
        public Sprite FarArtwork;
        public int AppearanceSchemaVersion;
        public Color Background = new Color(0.025f, 0.04f, 0.09f);
        public Color FarColor = new Color(0.06f, 0.1f, 0.18f);
        public Color MidColor = new Color(0.1f, 0.18f, 0.26f);
        public Color FloorColor = new Color(0.17f, 0.24f, 0.35f);
        public Color Accent = new Color(0.3f, 0.46f, 0.58f);
        public float FarParallax = 0.06f;
        public float MidParallax = 0.12f;
        // Retain the original geometry fields to migrate existing tuned Stage assets.
        public StagePlatform[] Platforms;
        public float LeftBlastLine;
        public float RightBlastLine;
        public float BottomBlastLine;
        public BattleVector SpawnP1;
        public BattleVector SpawnP2;
        public StageGeometry ToGeometry() => SharedGeometry ? SharedGeometry.ToGeometry() :
            new StageGeometry(Platforms, LeftBlastLine, RightBlastLine, BottomBlastLine, SpawnP1, SpawnP2);
    }
}
