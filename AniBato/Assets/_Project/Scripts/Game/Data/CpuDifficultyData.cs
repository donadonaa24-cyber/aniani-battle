using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/CPU難易度")]
    public sealed class CpuDifficultyData : ScriptableObject
    {
        public CpuDefinition Definition;
        public CpuDefinition ToDefinition() { Definition.Validate(); return Definition; }
    }
}
