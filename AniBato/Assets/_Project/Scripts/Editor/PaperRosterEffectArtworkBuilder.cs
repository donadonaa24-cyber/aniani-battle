using System;
using System.IO;
using System.Linq;
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
            public string id,look,kind,state,source,sha256;
            public int version;
            public int sortingOrder = 7;
            public float worldHeight;
            public bool duringStartup,duringRecovery,fitAttackHitbox;
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
            var abilities=selected.Where(e=>e.kind=="ability").Select(e=>
            {
                var state=(AnimationState)Enum.Parse(typeof(AnimationState),e.state);
                var look=(AbilityLook)Enum.Parse(typeof(AbilityLook),e.look);
                var attack=state==AnimationState.Ultimate?data.Ultimate:data.Specials[0];
                if(look!=attack.Definition.Ability.Look||e.worldHeight<=0)throw new InvalidOperationException("固有効果の技が不一致: "+key+"/"+e.id);
                var sprite=Import(key,e);
                return new PaperAbilityArt { State=state,Look=look,Sprite=sprite,Scale=e.worldHeight/Height(e),
                    DuringStartup=e.duringStartup,DuringRecovery=e.duringRecovery };
            }).ToArray();
            var projectiles=selected.Where(e=>e.kind=="projectile").Select(e=>
            {
                var look=(AbilityLook)Enum.Parse(typeof(AbilityLook),e.look);
                var attack=data.Specials.Concat(new[]{data.Ultimate}).Single(a=>a.Definition.SpawnProjectile&&a.Definition.Ability.Look==look).Definition;
                if(!e.fitAttackHitbox)throw new InvalidOperationException("持続場の表示寸法が未指定: "+key+"/"+e.id);
                var b=e.native.bounds;
                float width=(b.right-b.left+1)/PaperActionArtworkBuilder.PixelsPerUnit;
                return new PaperProjectileArt { Look=look,Sprite=Import(key,e),Scale=Mathf.Min(attack.Hitbox.Size.X/width,attack.Hitbox.Size.Y/Height(e)),SortingOrder=e.sortingOrder };
            }).ToArray();
            paper.AbilityEffects=(paper.AbilityEffects??Array.Empty<PaperAbilityArt>()).Where(a=>a!=null&&!abilities.Any(n=>n.State==a.State)).Concat(abilities).ToArray();
            paper.Projectiles=(paper.Projectiles??Array.Empty<PaperProjectileArt>()).Where(a=>a!=null&&!projectiles.Any(n=>n.Look==a.Look)).Concat(projectiles).ToArray();
        }
        private static float Height(Entry e)=>(e.native.bounds.bottom-e.native.bounds.top+1)/PaperActionArtworkBuilder.PixelsPerUnit;
        private static Sprite Import(string key,Entry e)
        {
            var n=e.native;var b=n.bounds;
            var pivot=new Vector2((b.left+b.right+1)*.5f/n.width,1-(b.top+b.bottom+1)*.5f/n.height);
            return PaperActionArtworkBuilder.Import(key+"_effect_"+e.id,e.source,e.sha256,n.width,n.height,pivot);
        }
    }
}
