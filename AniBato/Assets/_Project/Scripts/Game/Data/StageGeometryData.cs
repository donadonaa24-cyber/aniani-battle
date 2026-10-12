using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/共通ステージ地形")]
    public sealed class StageGeometryData : ScriptableObject
    {
        public StagePlatform[] Platforms;
        public float LeftBlastLine, RightBlastLine, BottomBlastLine;
        public BattleVector SpawnP1, SpawnP2;
        public StageGeometry ToGeometry() => new StageGeometry(Platforms, LeftBlastLine, RightBlastLine, BottomBlastLine, SpawnP1, SpawnP2);
    }
}
