using System;
using System.IO;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    // The user requested a real throw for normal 3. Migrate the tuned asset once;
    // keep its original near edge, last active-frame reach, timing and damage.
    public static class AkatsukiCardThrowBuilder
    {
        [Serializable] private sealed class Migration
        { public int schemaVersion=1; public string reason="ユーザー指定の通常3段カード投げ。発生・硬直・前進と最遠端を維持。"; public AttackDefinition before,after; }
        public static AttackDefinition Convert(AttackDefinition move)
        {
            if (move.SpawnProjectile) return move;
            if (!move.IsNormal || move.ActiveFrames < 2 || move.DamageDelayFrames != 0 || move.ActiveVelocity.X != 0)
                throw new InvalidOperationException("暁の通常3段の投射移行条件が変わっています。");
            float width = move.Hitbox.Size.X * (2f / 3f);
            float travel = move.Hitbox.Size.X - width;
            move.Hitbox = new BattleRect(new BattleVector(move.Hitbox.Center.X - travel * .5f, move.Hitbox.Center.Y),
                new BattleVector(width, move.Hitbox.Size.Y));
            // Spawn follows owner movement. Subsequent projectile steps also cover
            // the owner's remaining normal advance, ending at the old far edge.
            move.ProjectileSpeed = travel * BattleRuleDefinition.TickRate / (move.ActiveFrames - 1) + move.NormalForwardSpeed;
            move.ProjectileLifetimeFrames = move.ActiveFrames;
            move.SpawnProjectile = true; move.IsCloseRange = false;
            move.Ability.Look = AbilityLook.Card; move.Ability.ProjectileAtDamageFrame = true;
            move.Ability.IndependentNormalProjectile = true; move.Ability.ReplaceOwnProjectiles = true;
            move.Name = "カード投げ 3";
            move.Validate(); return move;
        }
        public static void Apply(CharacterData data)
        {
            // Version 1 was inherited by this roster asset before the card-throw work.
            if (data.NormalWeaponSchemaVersion >= 2) return;
            var before=data.GroundNormals[2].Definition;var after=Convert(before);
            const string audit="../art_source/paper_roster_action/akatsuki/normal3-migration.json";
            if(!File.Exists(audit))File.WriteAllText(audit,JsonUtility.ToJson(new Migration{before=before,after=after},true));
            data.GroundNormals[2].Definition = after;
            EditorUtility.SetDirty(data.GroundNormals[2]);
            data.NormalWeaponSchemaVersion = 2; EditorUtility.SetDirty(data);
        }
    }
}
