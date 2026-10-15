using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class CombatInputBuilder
    {
        public static CpuDefinition Cpu(CpuDefinition value, int difficulty)
        {
            if (value.SchemaVersion >= 6) return value;
            var old = CpuDefinition.ForDifficulty(difficulty);
            if (value.WhiffApproachAttempts == old.WhiffApproachAttempts) value.WhiffApproachAttempts = difficulty == 0 ? 6 : difficulty == 1 ? 4 : 3;
            value.SchemaVersion = 6; return value;
        }
        public static void Apply(CharacterData[] roster)
        {
            foreach (var character in roster)
            {
                if (character.UpNormal) continue;
                string path = "Assets/_Project/Data/" + character.name + "UpNormal.asset";
                var asset = AssetDatabase.LoadAssetAtPath<AttackData>(path);
                if (!asset)
                {
                    asset = ScriptableObject.CreateInstance<AttackData>();
                    var definition = character.GroundNormals[0].Definition;
                    definition.Name = character.Movement.Name + " 上攻撃"; definition.NormalDirection = NormalDirection.Up;
                    definition.SpawnProjectile = false; definition.IsCloseRange = true;
                    definition.ProjectileSpeed = 0; definition.ProjectileLifetimeFrames = 0;
                    definition.Hitbox = new BattleRect(new BattleVector(.35f, 1.05f), new BattleVector(1.2f, 1.45f));
                    definition.HasSecondaryHitbox = false; definition.ActiveVelocity = default;
                    definition.BreaksGuard = false; definition.Knockback = new BattleVector(1.5f, 5);
                    asset.Definition = definition; AssetDatabase.CreateAsset(asset, path);
                }
                character.UpNormal = asset; EditorUtility.SetDirty(character);
            }
        }
    }
}
