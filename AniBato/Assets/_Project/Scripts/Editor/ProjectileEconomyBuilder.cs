using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;

namespace AniBato.EditorTools
{
    public static class ProjectileEconomyBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            foreach(var data in roster)
            {
                if(data.ProjectileEconomySchemaVersion>=1)continue;
                var character=data.Movement;character.Stamina.NormalProjectileCost=6;character.Stamina.SpecialProjectileCost=18;data.Movement=character;
                foreach(var asset in data.GroundNormals.Concat(data.AirNormals).Concat(data.DirectionalNormals).Concat(data.Specials).Append(data.UpNormal).Distinct())
                {
                    if(!asset)continue;var attack=asset.Definition;
                    if(!attack.SpawnProjectile||attack.IsUltimate||attack.Ability.ProjectileChannel!=ProjectileChannel.Shared)continue;
                    attack.RecoveryFrames=System.Math.Max(12,attack.RecoveryFrames);attack.Validate();asset.Definition=attack;EditorUtility.SetDirty(asset);
                }
                data.ProjectileEconomySchemaVersion=1;EditorUtility.SetDirty(data);
            }
        }
    }
}
