using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/音設定")]
    public sealed class AudioSettings : ScriptableObject
    {
        [Range(0, 1)] public float BgmVolume = .35f;
        [Range(0, 1)] public float SeVolume = .65f;
    }
}
