using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/攻撃")]
    public sealed class AttackData : ScriptableObject
    {
        public AttackDefinition Definition;
        public bool IsNormal => Definition.IsNormal;
        public bool IsSpecial => Definition.IsSpecial;
        public bool IsUltimate => Definition.IsUltimate;
        public bool IsCloseRange => Definition.IsCloseRange;
        public AttackDefinition ToDefinition() { Definition.Validate(); return Definition; }
    }
}
