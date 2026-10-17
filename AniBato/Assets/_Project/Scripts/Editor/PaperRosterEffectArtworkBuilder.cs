using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AniBato.Battle;
using AniBato.Game;
using UnityEngine;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    // Only imports selected native artwork. The simulation owns teleportation and field lifetime.
    public static class PaperRosterEffectArtworkBuilder
    {
        [Serializable] public sealed class Manifest { public int schemaVersion; public string key; public Job[] jobs; public Entry[] entries; }
        [Serializable] public sealed class Job { public string id; public int version; }
        [Serializable] public sealed class Entry
        {
            public string id,look,kind,state,phase,source,sha256,anticipationText,alternateAnticipationText;
            public int version;
            public int variantIndex;
            public int sortingOrder = 7;
            public float worldHeight,worldWidth,offsetX,offsetY;
            public float anticipationTextSize = .08f;
            public bool duringStartup,duringRecovery,fitAttackHitbox,fitEachProjectileHitbox;
            public PaperActionArtworkBuilder.Native native;
        }
        public static void Apply(string key,CharacterData data)
        {
            string file="../art_source/paper_roster_action/"+key+"/effects-manifest.json";
            if(!File.Exists(file))return;
            var m=JsonUtility.FromJson<Manifest>(File.ReadAllText(file));
            if(m.schemaVersion!=1||m.key!=key||m.jobs==null||m.entries==null)throw new InvalidOperationException("固有効果の指示が不正: "+key);
            var selected=m.jobs.Select(j=>m.entries.Single(e=>e.id==j.id&&e.version==j.version)).ToArray();
            var paper=data.PaperMotion;
            var imported=new Dictionary<string,Sprite>();
            Sprite ImportEntry(Entry entry)
            {
                if(imported.TryGetValue(entry.sha256,out var cached))return cached;
                var value=Import(key,entry);imported.Add(entry.sha256,value);return value;
            }
            var abilities=selected.Where(e=>e.kind=="ability").GroupBy(e=>e.state).Select(group=>
            {
                var e=group.Single(e=>string.IsNullOrEmpty(e.phase)||e.phase=="impact");
                var prepare=group.SingleOrDefault(e=>e.phase=="prepare");
                if(group.Count()!=(prepare==null?1:2)||prepare!=null&&prepare.look!=e.look)throw new InvalidOperationException("固有効果の予備／決めが不正: "+key+"/"+e.state);
                var state=(AnimationState)Enum.Parse(typeof(AnimationState),e.state);
                var look=(AbilityLook)Enum.Parse(typeof(AbilityLook),e.look);
                var attack=AttackFor(data,state);
                if(look!=attack.Definition.Ability.Look||e.worldHeight<=0)throw new InvalidOperationException("固有効果の技が不一致: "+key+"/"+e.id);
                var sprite=ImportEntry(e);
                return new PaperAbilityArt { State=state,Look=look,Sprite=sprite,Scale=Scale(e),
                    Offset=new Vector2(e.offsetX,e.offsetY),
                    AnticipationSprite=prepare==null?null:ImportEntry(prepare),AnticipationScale=prepare==null?1:Scale(prepare),
                    AnticipationText=prepare?.anticipationText,AlternateAnticipationText=prepare?.alternateAnticipationText,
                    AnticipationTextSize=prepare == null ? .08f : prepare.anticipationTextSize,
                    DuringStartup=prepare!=null||e.duringStartup,DuringRecovery=e.duringRecovery };
            }).ToArray();
            var projectiles=selected.Where(e=>e.kind=="projectile").GroupBy(e=>e.look).Select(group=>
            {
                var variants=group.OrderBy(e=>e.variantIndex).ToArray();var e=variants[0];
                if(variants.Length>1&&!variants.Select(v=>v.variantIndex).SequenceEqual(Enumerable.Range(0,variants.Length)))throw new InvalidOperationException("召喚原画の並びが不正: "+key);
                var look=(AbilityLook)Enum.Parse(typeof(AbilityLook),e.look);
                var attacks=data.Specials.Concat(new[]{data.Ultimate}).Where(a=>a.Definition.SpawnProjectile&&a.Definition.Ability.Look==look).ToArray();
                if(attacks.Length==0||attacks.Length>1&&!e.fitEachProjectileHitbox)throw new InvalidOperationException("飛翔物の判定寸法の選択が不正: "+key);
                var attack=attacks[0].Definition;
                if(variants.Any(v=>!v.fitAttackHitbox||v.fitEachProjectileHitbox!=e.fitEachProjectileHitbox))throw new InvalidOperationException("持続場の表示寸法が未指定: "+key+"/"+e.id);
                var b=e.native.bounds;
                float width=(b.right-b.left+1)/PaperActionArtworkBuilder.PixelsPerUnit;
                return new PaperProjectileArt { Look=look,Sprite=ImportEntry(e),Variants=variants.Length>1?variants.Select(ImportEntry).ToArray():null,
                    VariantVisibleSizes=variants.Length>1?variants.Select(v=>new Vector2((v.native.bounds.right-v.native.bounds.left+1)/PaperActionArtworkBuilder.PixelsPerUnit,Height(v))).ToArray():null,
                    VisibleSize=new Vector2(width,Height(e)),FitToHitbox=e.fitEachProjectileHitbox,
                    Scale=Mathf.Min(attack.Hitbox.Size.X/width,attack.Hitbox.Size.Y/Height(e)),SortingOrder=e.sortingOrder };
            }).ToArray();
            paper.AbilityEffects=(paper.AbilityEffects??Array.Empty<PaperAbilityArt>()).Where(a=>a!=null&&!abilities.Any(n=>n.State==a.State)).Concat(abilities).ToArray();
            paper.Projectiles=(paper.Projectiles??Array.Empty<PaperProjectileArt>()).Where(a=>a!=null&&!projectiles.Any(n=>n.Look==a.Look)).Concat(projectiles).ToArray();
        }
        private static AttackData AttackFor(CharacterData data,AnimationState state)
        {
            switch(state)
            {
                case AnimationState.Attack1:return data.GroundNormals[0];
                case AnimationState.Attack2:return data.GroundNormals[1];
                case AnimationState.Attack3:return data.GroundNormals[2];
                case AnimationState.BackAttack:return data.DirectionalNormals[(int)NormalDirection.Back];
                case AnimationState.CrouchAttack:return data.DirectionalNormals[(int)NormalDirection.Crouch];
                case AnimationState.AirNeutral:return data.DirectionalNormals[(int)NormalDirection.AirNeutral];
                case AnimationState.AirUp:return data.UpNormal;
                case AnimationState.SpecialNeutral:return data.Specials[0];
                case AnimationState.Ultimate:return data.Ultimate;
                default:throw new InvalidOperationException("固有効果の行動が未対応: "+state);
            }
        }
        private static float Height(Entry e)=>(e.native.bounds.bottom-e.native.bounds.top+1)/PaperActionArtworkBuilder.PixelsPerUnit;
        private static float Scale(Entry e)
        {
            if(e.worldHeight<=0||e.native==null||e.native.bounds==null)throw new InvalidOperationException("効果の寸法が不正");
            float width=(e.native.bounds.right-e.native.bounds.left+1)/PaperActionArtworkBuilder.PixelsPerUnit;
            return Mathf.Min(e.worldHeight/Height(e),e.worldWidth>0?e.worldWidth/width:float.PositiveInfinity);
        }
        private static Sprite Import(string key,Entry e)
        {
            var n=e.native;var b=n.bounds;
            var pivot=new Vector2((b.left+b.right+1)*.5f/n.width,1-(b.top+b.bottom+1)*.5f/n.height);
            return PaperActionArtworkBuilder.Import(key+"_effect_"+e.id,e.source,e.sha256,n.width,n.height,pivot);
        }
    }
}
