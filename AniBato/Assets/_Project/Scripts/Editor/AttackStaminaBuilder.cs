using AniBato.Game;
using UnityEditor;

namespace AniBato.EditorTools
{
    public static class AttackStaminaBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            // Baseline 648 matches: Touma 84.7%, king 81.9%, Akatsuki 65.3%; Ryuta 11.1%.
            int[] first={7,4,4,5,4,4,7,5,3},second={8,5,5,6,5,5,8,6,4},third={12,8,8,9,8,8,12,10,6};
            int[] rest={16,18,18,18,18,18,14,16,20},delay={60,45,45,45,45,45,60,54,36};
            for(int i=0;i<roster.Length;i++)
            {
                var data=roster[i];if(data.AttackStaminaSchemaVersion>=1)continue;
                var character=data.Movement;var s=character.Stamina;
                s.NormalFirstCost=first[i];s.NormalSecondCost=second[i];s.NormalThirdCost=third[i];s.DirectionalNormalCost=first[i]+2;
                s.ChargeMinimumCost=i==0||i==6?14:i==8?8:10;s.ChargeMaximumCost=i==0||i==6?20:i==8?13:15;
                s.GroundRecoveryPerSecond=rest[i];s.ActiveRecoveryPerSecond=2;s.RecoveryDelayFrames=delay[i];s.Validate();
                character.Stamina=s;data.Movement=character;data.AttackStaminaSchemaVersion=1;EditorUtility.SetDirty(data);
            }
        }
    }
}
