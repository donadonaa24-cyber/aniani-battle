using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    // Independent assets preserve all previously tuned normal and neutral attacks.
    public static class DirectionalSpecialBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            for (int i = 0; i < roster.Length; i++)
            {
                var data = roster[i]; if (data.DirectionalSpecialSchemaVersion >= 2) continue;
                string path = "Assets/_Project/Data/" + data.name + "DirectionalDown.asset";
                var asset = AssetDatabase.LoadAssetAtPath<AttackData>(path);
                if (!asset)
                {
                    asset = ScriptableObject.CreateInstance<AttackData>(); asset.Definition = Down(i); asset.Definition.Validate();
                    AssetDatabase.CreateAsset(asset, path);
                }
                if (data.DirectionalSpecialSchemaVersion == 1)
                {
                    var definition = asset.Definition; definition.DamageRecoilDistance = .18f; definition.DamageRecoilFrames = 6;
                    asset.Definition = definition; EditorUtility.SetDirty(asset);
                }
                var slots = (AttackData[])data.Specials.Clone(); slots[(int)SpecialSlot.Down] = asset; data.Specials = slots;
                data.DirectionalSpecialSchemaVersion = 2; EditorUtility.SetDirty(data);
            }
        }
        public static AttackDefinition Down(int character)
        {
            var a = new AttackDefinition { Kind = AttackKind.Special, IsCloseRange = true, StartupFrames = 12, ActiveFrames = 1,
                RecoveryFrames = 18, Hitbox = Box(0, 0, 1, 1), CooldownFrames = 360, HitstopFrames = 3, HitstunFrames = 12,
                DamageMode = AttackDamageMode.NormalAccumulation, Knockback = new BattleVector(5, 3), DamageRecoilDistance = .18f, DamageRecoilFrames = 6 };
            switch (character)
            {
                case 0: case 4:
                    a.Name = "意識接続・背後転移"; a.NonDamaging = true; a.StartupFrames = 15; a.ActiveFrames = 1; a.RecoveryFrames = 12;
                    a.Ability = new AttackAbility { Motion = AbilityMotion.TeleportBehind, TargetGap = 1.6f, TeleportRange = 50, Conscious = true, Look = AbilityLook.Teleport };
                    break;
                case 1: case 5: case 6:
                    a.Name = character == 1 ? "飛車・超ガード" : character == 5 ? "角・超ガード" : "王・超ガード";
                    a.StartupFrames = 6; a.ActiveFrames = 36; a.RecoveryFrames = 18; a.Hitbox = Box(0, 0, 4.2f, 3);
                    a.Knockback = new BattleVector(character == 6 ? 17 : 13, 8);
                    a.Ability = new AttackAbility { SuperGuard = true, Look = character == 1 ? AbilityLook.Rook : character == 5 ? AbilityLook.Bishop : AbilityLook.King };
                    break;
                case 2:
                    a.Name = "もぐもぐタイム"; a.NonDamaging = true; a.StartupFrames = 0; a.ActiveFrames = 150; a.RecoveryFrames = 0;
                    a.SelfEffectsOnComplete = true; a.SelfHealNormalHits = 1; a.CooldownFrames = 1200;
                    a.Ability = new AttackAbility { GroundOnly = true, Look = AbilityLook.Meal }; break;
                case 3: case 7:
                    a.Name = character == 3 ? "意識地雷" : "設置・罠カード"; a.SpawnProjectile = true; a.IsCloseRange = false;
                    a.Hitbox = Box(.75f, 0, 1.2f, .4f); a.ProjectileLifetimeFrames = 1200; a.CooldownFrames = character == 3 ? 300 : 120;
                    a.Ability = new AttackAbility { AllowZeroProjectileSpeed = true, ProjectileAtDamageFrame = true,
                        GroundOnly = true, PlaceOnGround = true, ConsumePlacementOnHit = true, PlacementLimit = character == 3 ? 1 : 2,
                        ProjectileChannel = ProjectileChannel.Placed, Look = character == 3 ? AbilityLook.Mine : AbilityLook.TrapCard };
                    break;
                case 8:
                    a.Name = "英語の本・集中読書"; a.NonDamaging = true; a.StartupFrames = 0; a.ActiveFrames = 45; a.RecoveryFrames = 0;
                    a.SelfEffectsOnComplete = true; a.CooldownFrames = 600;
                    a.SelfBuff = new BuffDefinition { DurationFrames = 300, MoveMultiplier = 1.5f, DashMultiplier = 1.5f };
                    a.Ability.Look = AbilityLook.Reading; break;
                default: throw new System.ArgumentOutOfRangeException(nameof(character));
            }
            return a;
        }
        private static BattleRect Box(float x, float y, float width, float height) => new BattleRect(new BattleVector(x, y), new BattleVector(width, height));
    }
}
