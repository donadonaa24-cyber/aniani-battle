using System;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class PaperSupportBuilder
    {
        [Serializable] private sealed class Manifest { public bool complete; public Entry[] entries; }
        [Serializable] private sealed class Entry { public string key,source,sha256; public int width,height,left,top,right,bottom; public float worldHeight; }
        public static void Apply(BattleSetup setup)
        {
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("../art_source/paper_support_items/adoption.json"));
            if(!manifest.complete || manifest.entries.Length!=14)throw new InvalidOperationException("14 support originals required.");
            var art=AssetDatabase.LoadAssetAtPath<PaperSupportArt>("Assets/_Project/Data"+"/PaperSupportArt.asset");
            if(!art){art=ScriptableObject.CreateInstance<PaperSupportArt>();AssetDatabase.CreateAsset(art,"Assets/_Project/Data"+"/PaperSupportArt.asset");}
            art.Entries=manifest.entries.Select(e=>new SupportSprite{Key=e.key,Height=e.worldHeight,
                VisibleSize=new Vector2(e.right-e.left+1,e.bottom-e.top+1)/PaperActionArtworkBuilder.PixelsPerUnit,
                Sprite=PaperActionArtworkBuilder.Import("support_"+e.key,e.source,e.sha256,e.width,e.height,
                    new Vector2((e.left+e.right+1)*.5f/e.width,1-(e.bottom+1f)/e.height))}).ToArray();
            art.Rift=setup.Character(6).PaperMotion.SummonRift;
            art.Card=setup.Character(7).PaperMotion.Projectile(AbilityLook.Card).Sprite;
            if(!art.Rift || !art.Card)throw new InvalidOperationException("Support portal/card source missing.");
            art.FieldShader=Shader.Find("Hidden/AniBato/SupportConscious");
            if(!art.FieldShader)throw new InvalidOperationException("Support world filter missing.");
            setup.SupportArt=art; EditorUtility.SetDirty(art);EditorUtility.SetDirty(setup);
        }
    }
}
