using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/対戦ルール")]
    public sealed class BattleRules : ScriptableObject
    {
        public BattleRuleDefinition Definition;
        public BattleRuleDefinition ToDefinition() { Definition.Validate(); return Definition; }
    }
}
