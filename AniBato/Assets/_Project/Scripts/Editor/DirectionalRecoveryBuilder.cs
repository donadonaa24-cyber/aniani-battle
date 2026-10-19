using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class DirectionalRecoveryBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            for (int i = 0; i < roster.Length; i++)
            {
                var data = roster[i]; if (data.RecoverySchemaVersion >= 3) continue;
                string path = "Assets/_Project/Data/" + data.name + "Recovery.asset";
                var asset = AssetDatabase.LoadAssetAtPath<AttackData>(path);
                if (!asset)
                {
                    asset = ScriptableObject.CreateInstance<AttackData>(); asset.Definition = Up(i); asset.Definition.Validate();
                    AssetDatabase.CreateAsset(asset, path);
                }
                if (data.RecoverySchemaVersion == 1 && (i == 1 || i == 5 || i == 8))
                {
                    var definition = asset.Definition; definition.Recovery.Speed = Up(i).Recovery.Speed;
                    asset.Definition = definition; EditorUtility.SetDirty(asset);
                }
                if (data.RecoverySchemaVersion >= 1 && data.RecoverySchemaVersion < 3 && (i == 1 || i == 8))
                {
                    var definition = asset.Definition; var updated = Up(i);
                    definition.ActiveFrames = updated.ActiveFrames; definition.RecoveryFrames = updated.RecoveryFrames;
                    asset.Definition = definition; EditorUtility.SetDirty(asset);
                }
                var slots = (AttackData[])data.Specials.Clone(); slots[(int)SpecialSlot.Up] = asset; data.Specials = slots;
                data.RecoverySchemaVersion = 3; EditorUtility.SetDirty(data);
            }
        }
        public static AttackDefinition Up(int index)
        {
            var a = new AttackDefinition { Name = "上復帰", Kind = AttackKind.Special, IsCloseRange = true,
                StartupFrames = 6, ActiveFrames = 30, RecoveryFrames = 16, CooldownFrames = 90,
                Hitbox = new BattleRect(default, new BattleVector(2.3f, 2.3f)), DamageMode = AttackDamageMode.NormalAccumulation,
                HitstopFrames = 3, HitstunFrames = 14, Knockback = new BattleVector(7, 7), DamageRecoilDistance = .18f, DamageRecoilFrames = 6 };
            switch (index)
            {
                case 0: a.Name = "ドローン・レスキュー"; a.Recovery = Float(8, 7); a.ActiveFrames = 60; a.Ability.Look = AbilityLook.Drone; break;
                case 1: a.Name = "飛車・天昇"; a.Recovery = new RecoveryDefinition { Style = RecoveryStyle.VerticalLaunch, Speed = 36 }; a.ActiveFrames = 24; a.RecoveryFrames = 4; a.Ability.Look = AbilityLook.Rook; break;
                case 2: a.Name = "風船リフト"; a.Recovery = Float(9, 3); a.ActiveFrames = 60; break;
                case 3: case 4:
                    a.Name = "意識接続・方向転移"; a.Recovery = new RecoveryDefinition { Style = RecoveryStyle.DirectionalTeleport, Distance = 7.5f };
                    a.ActiveFrames = 3; a.Ability.Look = AbilityLook.Teleport; a.Ability.Conscious = true; break;
                case 5: a.Name = "角・斜め天昇"; a.Recovery = new RecoveryDefinition { Style = RecoveryStyle.DiagonalLaunch, Speed = 32, HorizontalSpeed = 18 }; a.Ability.Look = AbilityLook.Bishop; break;
                case 6:
                    a.Name = "王・二段閃"; a.Recovery = new RecoveryDefinition { Style = RecoveryStyle.DoubleBurst, Speed = 27, BurstFrames = 8, SecondStartFrame = 16 };
                    a.ActiveFrames = 24; a.Ability.Look = AbilityLook.King; break;
                case 7: a.Name = "ばたばたリフト"; a.Recovery = Float(8, 6); a.ActiveFrames = 60; break;
                case 8: a.Name = "ブック・ジャンプ"; a.Recovery = new RecoveryDefinition { Style = RecoveryStyle.VerticalLaunch, Speed = 30 }; a.ActiveFrames = 18; a.RecoveryFrames = 4; break;
                default: throw new System.ArgumentOutOfRangeException(nameof(index));
            }
            return a;
        }
        private static RecoveryDefinition Float(float vertical, float horizontal) => new RecoveryDefinition { Style = RecoveryStyle.Float, Speed = vertical, HorizontalSpeed = horizontal };
    }
}
