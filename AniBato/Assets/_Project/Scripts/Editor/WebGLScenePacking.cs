using System.Collections.Generic;
using System.Linq;
using AniBato.Game;
using AniBato.Presentation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
using AnimationState=AniBato.Presentation.AnimationState;
namespace AniBato.EditorTools
{
    // Work only on transient build copies. Original assets, backup artwork and tuning are untouched.
    public sealed class WebGLScenePacking : IProcessSceneWithReport,IPreprocessBuildWithReport,IPostprocessBuildWithReport
    {
        private static readonly Dictionary<Object,Object> copies=new Dictionary<Object,Object>();
        public int callbackOrder=>40;
        public static BattleSetup CopyForBuild(BattleSetup setup)=>(BattleSetup)Copy(setup);
        public static void ReleaseCopies()
        {
            foreach(var copy in copies.Values)if(copy&&!EditorUtility.IsPersistent(copy))Object.DestroyImmediate(copy);
            copies.Clear();
            WebGLSpritePacking.Reset();
        }
        public void OnPreprocessBuild(BuildReport report){copies.Clear();}
        public void OnPostprocessBuild(BuildReport report)
        {
            ReleaseCopies();
        }
        private static bool Retired(AnimationState s)=>s==AnimationState.AirForward||s==AnimationState.AirBack||s==AnimationState.AirDown||s==AnimationState.AirForwardUp||s==AnimationState.AirForwardDown||s==AnimationState.SpecialSide||s==AnimationState.SpecialUp||s==AnimationState.SpecialDown;
        public void OnProcessScene(Scene scene,BuildReport report)
        {
            if(report==null||report.summary.platform!=BuildTarget.WebGL)return;
            foreach(var root in scene.GetRootGameObjects())foreach(var component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if(!component)continue;var serialized=new SerializedObject(component);var property=serialized.GetIterator();
                while(property.NextVisible(true))if(property.propertyType==SerializedPropertyType.ObjectReference&&property.objectReferenceValue)
                {
                    var replacement=Copy(property.objectReferenceValue);
                    if(replacement!=property.objectReferenceValue)property.objectReferenceValue=replacement;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            Debug.Log("WebGL build-only asset copies: "+copies.Count);
        }
        private static Object Copy(Object source)
        {
            if(!source)return source;if(copies.TryGetValue(source,out var existing))return existing;
            if(source is Sprite sprite)
            {
                var c=WebGLSpritePacking.Copy(sprite);if(c!=source)copies.Add(source,c);return c;
            }
            if(source is GeneratedStageArt stageArt)
            {
                var c=Object.Instantiate(stageArt);copies.Add(source,c);
                c.Far=(Sprite)Copy(c.Far);c.Mid=(Sprite)Copy(c.Mid);c.Frame=(Sprite)Copy(c.Frame);
                c.Main=(Sprite)Copy(c.Main);c.Upper=(Sprite)Copy(c.Upper);c.Warning=(Sprite)Copy(c.Warning);c.Removal=(Sprite)Copy(c.Removal);return c;
            }
            if(source is PaperEnsembleArt ensemble)
            {
                var c=Object.Instantiate(ensemble);copies.Add(source,c);c.Artwork=(Sprite)Copy(c.Artwork);return c;
            }
            if(source is CharacterData original)
            {
                var c=Object.Instantiate(original);copies.Add(source,c);c.name=original.name+" Web";
                c.PaperMotion=(PaperCharacterMotion)Copy(original.PaperMotion);
                c.SelectionSprite=(Sprite)Copy(c.SelectionSprite);c.Portrait=(Sprite)Copy(c.Portrait);
                if(original.Animations)
                {
                    var a=Object.Instantiate(original.Animations);a.name=original.Animations.name+" Web "+c.name;
                    a.Bindings=a.Bindings.Where(b=>!Retired(b.State)&&(!c.PaperMotion||!c.PaperMotion.Supports(b.State))).ToArray();
                    var names=new HashSet<string>(a.Bindings.Select(b=>b.Clip));
                    if(c.SpriteEffects!=null)
                        foreach(var n in new[]{c.SpriteEffects.ProjectileClip,c.SpriteEffects.NormalProjectileClip,c.SpriteEffects.DownSpecialClip,c.SpriteEffects.UltimateClip})if(!string.IsNullOrEmpty(n))names.Add(n);
                    a.Clips=a.Clips.Where(clip=>names.Contains(clip.Name)).ToArray();
                    foreach(var clip in a.Clips)clip.Frames=clip.Frames.Select(s=>(Sprite)Copy(s)).ToArray();
                    c.Animations=a;
                    // Keep this clone alive for the entire build and release it afterwards.
                    copies.Add(a,a);
                }
                return c;
            }
            if(source is PaperCharacterMotion motion)
            {
                var c=Object.Instantiate(motion);copies.Add(source,c);
                c.Poses=c.Poses.Where(p=>p!=null&&p.States.Any(s=>!Retired(s))).ToArray();
                foreach(var pose in c.Poses)pose.Frames=pose.Frames.Select(s=>(Sprite)Copy(s)).ToArray();
                if(c.HeavyDamage?.Frames!=null)c.HeavyDamage.Frames=c.HeavyDamage.Frames.Select(s=>(Sprite)Copy(s)).ToArray();
                if(c.Charging?.Frames!=null)c.Charging.Frames=c.Charging.Frames.Select(s=>(Sprite)Copy(s)).ToArray();
                if(c.UpRecovery?.Frames!=null)c.UpRecovery.Frames=c.UpRecovery.Frames.Select(s=>(Sprite)Copy(s)).ToArray();
                c.SummonRift=(Sprite)Copy(c.SummonRift);
                c.LedgeHang=(Sprite)Copy(c.LedgeHang);
                c.Sprite=(Sprite)Copy(c.Sprite);c.RiceBall=(Sprite)Copy(c.RiceBall);c.Mogu=(Sprite)Copy(c.Mogu);c.CurryWave=(Sprite)Copy(c.CurryWave);
                if(c.Projectiles!=null)foreach(var art in c.Projectiles)
                { art.Sprite=(Sprite)Copy(art.Sprite);if(art.Variants!=null)art.Variants=art.Variants.Select(s=>(Sprite)Copy(s)).ToArray(); }
                if(c.AbilityEffects!=null)foreach(var art in c.AbilityEffects){art.Sprite=(Sprite)Copy(art.Sprite);art.AnticipationSprite=(Sprite)Copy(art.AnticipationSprite);}
                // Current normal attacks do not throw a bag. The backup remains in the project.
                c.Bag=null;return c;
            }
            if(source is BattleSetup setup)
            {
                var c=Object.Instantiate(setup);copies.Add(source,c);
                c.Characters=setup.Characters.Select(v=>(CharacterData)Copy(v)).ToArray();
                c.Stages=setup.Stages.Select(v=>(StageData)Copy(v)).ToArray();return c;
            }
            if(source is StageData stage&&stage.GeneratedArt&&stage.GeneratedArt.Complete)
            {
                var c=Object.Instantiate(stage);copies.Add(source,c);c.FarArtwork=null;
                c.GeneratedArt=(GeneratedStageArt)Copy(c.GeneratedArt);c.Thumbnail=(Sprite)Copy(c.Thumbnail);return c;
            }
            return source;
        }
    }
}
