using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/音設定")]
    public sealed class AudioSettings : ScriptableObject
    {
        public AniBato.Presentation.ArcadeAudioSettings Style = AniBato.Presentation.ArcadeAudioSettings.Default;
        [Range(0, 1)] public float DuckRatio = .45f;
        [Min(0)] public float DuckSeconds = .16f;
        [Range(0, 1)] public float BgmVolume = .35f;
        [Range(0, 1)] public float SeVolume = .65f;
    }
}
