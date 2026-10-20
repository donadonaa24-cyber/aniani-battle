using AniBato.Battle;
using AniBato.Game;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace AniBato.EditorTools
{
    public static class RoyalMarchBuilder
    {
        [System.Serializable] private sealed class Adoption { public string source,sha256; public int width,height; }
        public static void Apply(CharacterData king)
        {
            var art=JsonUtility.FromJson<Adoption>(File.ReadAllText("../art_source/king_march/adoption.json"));
            king.PaperMotion.SummonRift=PaperActionArtworkBuilder.Import("king_dimensional_rift",art.source,art.sha256,art.width,art.height,new Vector2(.5f,.5f));
            EditorUtility.SetDirty(king.PaperMotion);
            if(king.RoyalMarchSchemaVersion>=1)return;
            var asset=king.Specials[0];var n=asset.Definition;
            float height=king.PaperMotion.StandingHeight;var minions=king.PaperMotion.Projectile(AbilityLook.King);
            var sizes=minions.VariantVisibleSizes;
            n.StartupFrames=1;n.ActiveFrames=85;n.DamageDelayFrames=60;n.RecoveryFrames=16;n.CooldownFrames=300;
            n.ProjectileLifetimeFrames=300;n.ProjectileSpeed=0;n.ProjectileGravity=0;n.ProjectileLaunchVelocity=default;
            n.Hitbox=new BattleRect(default,new BattleVector(height*.7f,height));
            n.Ability.AllowZeroProjectileSpeed=true;n.Ability.ProjectileAtDamageFrame=false;n.Ability.ShotOrigin=ShotOrigin.Owner;
            n.ProjectileBehavior=new ProjectileBehaviorDefinition { Formation=ProjectileFormation.RoyalMarch,
                MarchSpawnFrames=60,MarchCommandFrames=24,MarchTravelFrames=180,MarchExitFrames=24,
                MarchWidth0=height*sizes[0].x/sizes[0].y,MarchWidth1=height*sizes[1].x/sizes[1].y,MarchWidth2=height*sizes[2].x/sizes[2].y };
            n.Validate();asset.Definition=n;EditorUtility.SetDirty(asset);king.RoyalMarchSchemaVersion=1;EditorUtility.SetDirty(king);
        }
    }
}
