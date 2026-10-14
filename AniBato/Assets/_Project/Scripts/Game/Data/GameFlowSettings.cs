using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/画面進行設定")]
    public sealed class GameFlowSettings : ScriptableObject
    {
        public int TheatreSchemaVersion;
        [Min(0.01f)] public float CountdownNumberSeconds = 1;
        [Min(0.01f)] public float FightBannerSeconds = 0.5f;
        [Min(0)] public float ResultDelaySeconds = 2.5f;
    }
}
