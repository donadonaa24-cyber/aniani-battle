using AniBato.Battle;
using AniBato.Game;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class RosterMoveSetBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            for (int i = 0; i < roster.Length; i++)
            {
                var data = roster[i];
                if (data.MoveSetSchemaVersion >= 1)
                {
                    if (i == 0 && data.MoveSetSchemaVersion < 2) GunProjectileSlot(data);
                    continue;
                }
                if (i != 2)
                {
                    data.GroundNormals = Normals(data, data.GroundNormals, "Ground", i);
                    data.AirNormals = Normals(data, data.AirNormals, "AirBackup", i);
                    var directions = (AttackData[])data.DirectionalNormals.Clone();
                    foreach (int direction in new[] { 1, 2, 3 })
                    {
                        var normal = Normal(directions[direction].Definition, i, direction);
                        directions[direction] = Asset(data.name + "R3Direction" + direction, normal);
                    }
                    data.DirectionalNormals = directions;
                    data.UpNormal = Asset(data.name + "R3Up", Normal(data.UpNormal.Definition, i, 10));
                }
                var special = Special(i, data.Specials[0].Definition);
                var slots = (AttackData[])data.Specials.Clone();
                slots[0] = Asset(data.name + "R3Special", special); data.Specials = slots;
                data.Ultimate = Asset(data.name + "R3Ultimate", Ultimate(i, data.Ultimate.Definition));
                data.PrototypeSharedMoves = false; data.ActionArtworkPending = true;
                data.MoveSetSchemaVersion = 1; EditorUtility.SetDirty(data);
                if (i == 0) GunProjectileSlot(data);
            }
        }
        private static void GunProjectileSlot(CharacterData data)
        {
            foreach (var asset in data.GroundNormals.Concat(data.AirNormals).Concat(data.DirectionalNormals).Append(data.UpNormal).Where(a => a).Distinct())
            {
                var value = asset.Definition; if (value.Ability.Look != AbilityLook.Bullet) continue;
                value.Ability.IndependentNormalProjectile = true; value.Ability.ReplaceOwnProjectiles = true;
                asset.Definition = value; EditorUtility.SetDirty(asset);
            }
            data.MoveSetSchemaVersion = 2; EditorUtility.SetDirty(data);
        }
        private static AttackData[] Normals(CharacterData owner, AttackData[] source, string label, int index)
        {
            var result = new AttackData[source.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = Asset(owner.name + "R3" + label + i, Normal(source[i].Definition, index, i));
            return result;
        }
        private static AttackDefinition Normal(AttackDefinition value, int index, int key)
        {
            string[] styles = { "拳銃射撃", "カポエラー", "フライパン", "義手打撃", "体当たり・噛みつき", "肉球パンチ", "獅子のひっかき", "カード斬り", "素手打撃" };
            value.Name = styles[index] + " " + key;
            if (index == 0)
            {
                bool up = value.NormalDirection == NormalDirection.Up, down = value.NormalDirection == NormalDirection.Crouch;
                float x = value.NormalDirection == NormalDirection.Back ? -1.1f : up ? .25f : 1.1f;
                value.Hitbox = Box(x, up ? 1.05f : down ? -.55f : .35f, .32f, .28f);
                value.SpawnProjectile = true; value.IsCloseRange = false;
                value.ProjectileSpeed = up ? 1 : 26; value.ProjectileLifetimeFrames = 36;
                value.ProjectileLaunchVelocity = new BattleVector(0, up ? 26 : down ? -8 : 0);
                value.Ability = new AttackAbility { Look = AbilityLook.Bullet, ProjectileAtDamageFrame = true };
                value.HasSecondaryHitbox = false; value.ActiveVelocity = default;
            }
            return value;
        }
        private static AttackDefinition Special(int index, AttackDefinition old)
        {
            var a = Attack("特殊", AttackKind.Special, 16, 5, 22, Box(1.2f, .1f, 2.2f, 1.8f));
            a.CooldownFrames = 90;
            switch (index)
            {
                case 0:
                    a.Name = "ドローン特攻"; a.StartupFrames = 12; a.Hitbox = Box(1.1f,.35f,.8f,.55f);
                    Shot(ref a, AbilityLook.Drone, 16, 80); break;
                case 1:
                    a.Name = "飛車変身・高速突撃"; a.ActiveFrames = 8;
                    a.Ability = Rush(AbilityLook.Rook, 8, new BattleVector(48,0), new BattleVector(48,0)); break;
                case 2:
                    a = old; a.Name = "おにぎり投げ"; a.Ability.Look = AbilityLook.RiceBall; break;
                case 3:
                    a.Name = "意識フィールド・瞬間移動"; a.StartupFrames = 18;
                    a.Ability = new AttackAbility { Motion = AbilityMotion.Teleport, TeleportRange = 8, TargetGap = 1.5f, Look = AbilityLook.Teleport }; break;
                case 4:
                    a.Name = "ALMA融合・電磁パルス"; a.StartupFrames = 18; a.ActiveFrames = 8;
                    a.Hitbox = Box(0,0,6,4); a.ParalysisFrames = 12; a.Ability.Look = AbilityLook.Emp; break;
                case 5:
                    a.Name = "角変身・斜め急襲"; a.ActiveFrames = 20; a.Hitbox = Box(.8f,-.2f,2.2f,2);
                    a.Ability = Rush(AbilityLook.Bishop, 9, new BattleVector(15,18), new BattleVector(18,-24)); break;
                case 6:
                    a.Name = "王変身・部下招集"; a.StartupFrames = 24;
                    Drop(ref a, AbilityLook.King, 8, 1.2f, 1.4f, 14, 3, 1.5f); break;
                case 7:
                    a.Name = "カード投げ"; a.StartupFrames = 10; a.Hitbox = Box(1.1f,.25f,.65f,.4f);
                    Shot(ref a, AbilityLook.Card, 20, 60); break;
                case 8:
                    a.Name = "バット一閃"; a.StartupFrames = 15; a.Hitbox = Box(1.6f,.2f,2.7f,1.8f);
                    a.Knockback = new BattleVector(7,5); a.Ability.Look = AbilityLook.Bat; break;
            }
            return a;
        }
        private static AttackDefinition Ultimate(int index, AttackDefinition old)
        {
            var a = Attack("超必殺", AttackKind.Ultimate, 28, 12, 30, Box(2,0,6,4));
            a.Knockback = new BattleVector(12,10); a.HitstopFrames = 8; a.HitstunFrames = 24;
            switch (index)
            {
                case 0:
                    a.Name = "画面外ロケット"; a.StartupFrames = 30; a.ActiveFrames = 1;
                    Drop(ref a, AbilityLook.Rocket, 13, 1.8f, 2.8f, 20, 1, 0); break;
                case 1:
                    a.Name = "飛車連続急襲"; a.StartupFrames = 24; a.ActiveFrames = 18; a.Hitbox = Box(1,.1f,3,2.2f);
                    a.Ability = Rush(AbilityLook.Rook, 9, new BattleVector(42,0), new BattleVector(-42,0)); break;
                case 2:
                    a = old; a.Name = "満腹カレーをぶちまける"; a.Ability.Look = AbilityLook.Curry; break;
                case 3:
                    a.Name = "意識解放・虚空フィールド"; a.ActiveFrames = 1; a.Hitbox = Box(0,1.6f,5,4);
                    Shot(ref a, AbilityLook.BlackHole, 0, 180); a.Ability.AllowZeroProjectileSpeed = true;
                    a.Ability.ShotOrigin = ShotOrigin.StageCenter; a.Ability.ReplaceOwnProjectiles = true; break;
                case 4:
                    a.Name = "ALMA・二進数空間"; a.StartupFrames = 24; a.ActiveFrames = 36; a.DamageDelayFrames = 24;
                    a.Hitbox = Box(0,0,9,5); a.Ability.Look = AbilityLook.Alma; break;
                case 5:
                    a.Name = "角・交差急襲"; a.StartupFrames = 26; a.ActiveFrames = 24; a.Hitbox = Box(.6f,0,3,2.6f);
                    a.Ability = Rush(AbilityLook.Bishop, 10, new BattleVector(24,20), new BattleVector(-24,-22)); break;
                case 6:
                    a.Name = "王将・駒軍勢総攻撃"; a.StartupFrames = 32; a.ActiveFrames = 1;
                    Drop(ref a, AbilityLook.King, 12, 2.2f, 2.8f, 20, 3, 2.6f); break;
                case 7:
                    a.Name = "砂錬成・頭上ピラミッド"; a.StartupFrames = 36; a.ActiveFrames = 1;
                    Drop(ref a, AbilityLook.Pyramid, 13, 2.8f, 3, 16, 1, 0); break;
                case 8:
                    a.Name = "英語の本・謎の呪文"; a.StartupFrames = 34; a.ActiveFrames = 20; a.DamageDelayFrames = 8;
                    a.Hitbox = Box(2.5f,0,7,4.5f); a.Ability.Look = AbilityLook.Spell; break;
            }
            return a;
        }
        private static AttackDefinition Attack(string name, AttackKind kind, int startup, int active, int recovery, BattleRect box) => new AttackDefinition
        { Name = name, Kind = kind, IsCloseRange = true, StartupFrames = startup, ActiveFrames = active, RecoveryFrames = recovery,
            Hitbox = box, HitstopFrames = 4, HitstunFrames = 14, Knockback = new BattleVector(5,4), DamageRecoilDistance = .18f, DamageRecoilFrames = 6 };
        private static BattleRect Box(float x,float y,float width,float height) => new BattleRect(new BattleVector(x,y),new BattleVector(width,height));
        private static AttackAbility Rush(AbilityLook look,int change,BattleVector first,BattleVector second) =>
            new AttackAbility { Look = look, Motion = AbilityMotion.TwoPhaseRush, ChangeFrame = change, FirstVelocity = first, SecondVelocity = second };
        private static void Shot(ref AttackDefinition a,AbilityLook look,float speed,int lifetime)
        { a.SpawnProjectile = true; a.IsCloseRange = false; a.ProjectileSpeed = speed; a.ProjectileLifetimeFrames = lifetime; a.Ability.Look = look; a.Ability.ProjectileAtDamageFrame = true; }
        private static void Drop(ref AttackDefinition a,AbilityLook look,float height,float width,float depth,float speed,int count,float spacing)
        {
            a.Hitbox = Box(0,height,width,depth); Shot(ref a,look,0,100);
            a.ProjectileLaunchVelocity = new BattleVector(0,-speed); a.ProjectileGravity = 12;
            a.Ability.AllowZeroProjectileSpeed = true; a.Ability.ShotOrigin = ShotOrigin.OpponentAbove;
            a.Ability.TargetRange = 14;
            a.Ability.ProjectileCount = count; a.Ability.ProjectileSpacing = spacing; a.Ability.ReplaceOwnProjectiles = a.IsUltimate;
        }
        private static AttackData Asset(string key,AttackDefinition definition)
        {
            string path = "Assets/_Project/Data/" + key + ".asset"; var asset = AssetDatabase.LoadAssetAtPath<AttackData>(path);
            if (!asset) { asset = ScriptableObject.CreateInstance<AttackData>(); asset.Definition = definition; asset.Definition.Validate(); AssetDatabase.CreateAsset(asset,path); }
            return asset;
        }
    }
}
