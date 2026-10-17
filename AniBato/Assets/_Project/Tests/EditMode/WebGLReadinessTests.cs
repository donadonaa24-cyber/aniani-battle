using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using AniBato.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
namespace AniBato.Tests
{
    public sealed class WebGLReadinessTests
    {
        [Test]
        public void PaperMipPaddingPreservesRgbaAndLeavesBackgroundAndPixelImportsAlone()
        {
            var source=Enumerable.Range(0,35).Select(i=>new Color32((byte)i,57,93,(byte)(i%3==0?0:255))).ToArray();
            var packed=WebGLSpritePacking.PadMipChain(source,5,7,out int w,out int h);
            Assert.That(w,Is.EqualTo(8));Assert.That(h,Is.EqualTo(8));
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)Assert.That(packed[y*w+x],Is.EqualTo(x<5&&y<7?source[y*5+x]:default(Color32)));
            foreach(string folder in new[]{"Paper","PaperAction","PaperCore","PaperRoster"})Assert.That(WebGLSpritePacking.UsesMipmaps("Assets/_Project/Art/"+folder+"/sample.png"),Is.True);
            foreach(string folder in new[]{"Sprites","GeneratedStages","PaperEnsemble","Stages"})Assert.That(WebGLSpritePacking.UsesMipmaps("Assets/_Project/Art/"+folder+"/sample.png"),Is.False);
        }
        [Test]
        public void MipmappedAlphaRemainsStableWhenFineDetailsShrinkToSeventeenPixels()
        {
            var texture=new Texture2D(256,256,TextureFormat.RGBA32,true){filterMode=FilterMode.Trilinear};
            var pixels=new Color32[256*256];for(int y=0;y<256;y++)for(int x=0;x<256;x++)pixels[y*256+x]=new Color32(255,255,255,(byte)((x+y)%2==0?255:0));
            texture.SetPixels32(pixels);texture.Apply(true);
            var target=RenderTexture.GetTemporary(17,17,0,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;
            var image=new Texture2D(17,17,TextureFormat.RGBA32,false);
            try
            {
                Graphics.Blit(texture,target);RenderTexture.active=target;image.ReadPixels(new Rect(0,0,17,17),0,0);image.Apply();
                foreach(var pixel in image.GetPixels32())Assert.That((int)pixel.a,Is.InRange(124,131),"Small details must retain average coverage rather than disappear between samples");
                Assert.That(texture.width,Is.EqualTo(256));Assert.That(texture.GetPixels32(0),Is.EqualTo(pixels));
            }
            finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Object.DestroyImmediate(image);Object.DestroyImmediate(texture);}
        }
        [Test]
        public void BlockPaddingPreservesEveryRgbaPixelAndOnlyAddsTransparentTopAndRight()
        {
            var source=Enumerable.Range(0,35).Select(i=>new Color32((byte)i,(byte)(i*2),37,(byte)(i%3==0?0:255))).ToArray();
            var packed=WebGLSpritePacking.Pad(source,5,7,out int w,out int h);
            Assert.That(w,Is.EqualTo(8));Assert.That(h,Is.EqualTo(8));
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)Assert.That(packed[y*w+x],Is.EqualTo(x<5&&y<7?source[y*5+x]:default(Color32)));
        }
        [Test]
        public void PackedSpriteKeepsNativeFramePivotPpuAndWorldBounds()
        {
            var originalTexture=new Texture2D(17,19);var packedTexture=new Texture2D(20,20);
            var source=Sprite.Create(originalTexture,new Rect(2,3,11,13),new Vector2(.27f,.15f),532.5893f,0,SpriteMeshType.FullRect);
            var copy=WebGLSpritePacking.CopyIntoTexture(source,packedTexture);
            try
            {
                Assert.That(copy.texture,Is.SameAs(packedTexture));Assert.That(copy.rect,Is.EqualTo(source.rect));
                Assert.That(copy.pivot,Is.EqualTo(source.pivot));Assert.That(copy.pixelsPerUnit,Is.EqualTo(source.pixelsPerUnit));Assert.That(copy.bounds,Is.EqualTo(source.bounds));
            }
            finally{Object.DestroyImmediate(source);Object.DestroyImmediate(copy);Object.DestroyImmediate(originalTexture);Object.DestroyImmediate(packedTexture);}
        }
        [Test]
        public void FaceHotspotsUseTheNativeSpriteFrameInsteadOfThePaddedTextureHeight()
        {
            var texture=new Texture2D(20,24);var sprite=Sprite.Create(texture,new Rect(0,0,20,21),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
            var data=ScriptableObject.CreateInstance<PaperEnsembleArt>();data.Artwork=sprite;data.Hotspots=new[]{new Rect(.1f,.1f,.5f,.5f)};
            var face=data.Face(0);
            try{Assert.That(face.rect.y,Is.EqualTo(8.4f).Within(.001f));Assert.That(face.rect.width,Is.EqualTo(10));Assert.That(face.rect.height,Is.EqualTo(10.5f));}
            finally{Object.DestroyImmediate(face);Object.DestroyImmediate(sprite);Object.DestroyImmediate(texture);Object.DestroyImmediate(data);}
        }
        [Test]
        public void BrowserDefaultsAvoidModifiersAndDoNotOverlapBetweenPlayers()
        {
            var a=WebInputPolicy.Keys(InputBindings.DefaultP1,0,true);var b=WebInputPolicy.Keys(InputBindings.DefaultP2,1,true);
            Key[] Keys(PlayerKeyBindings k)=>new[]{k.Left,k.Right,k.Up,k.Down,k.Jump,k.Attack,k.Special,k.Guard,k.Dodge,k.Counter,k.Ultimate,k.Dash};
            Assert.That(Keys(a).Intersect(Keys(b)),Is.Empty);
            foreach(var k in Keys(a).Concat(Keys(b)))Assert.That(WebInputPolicy.CanBind(k,true),Is.True,k.ToString());
            Assert.That(WebInputPolicy.CanBind(Key.F5,true),Is.False);Assert.That(WebInputPolicy.CanBind(Key.RightCtrl,true),Is.False);
            Assert.That(WebInputPolicy.Pause(0,true),Is.EqualTo(Key.P));Assert.That(WebInputPolicy.Pause(1,true),Is.EqualTo(Key.O));
            Assert.That(WebInputPolicy.Pause(0,false),Is.EqualTo(Key.Escape));
            Assert.That(WebInputPolicy.Keys(InputBindings.DefaultP2,1,false).Jump,Is.EqualTo(Key.RightCtrl));
        }
        [Test]
        public void RedistributableEmbeddedFontCoversJapaneseRosterAndMenus()
        {
            var font=Resources.Load<Font>("NotoSansJP-Regular");Assert.That(font,Is.Not.Null);
            foreach(char c in "あにバト透真湊シロ蓮結衣黒獅子王千鶴暁龍太勝負設定選択")Assert.That(font.HasCharacter(c),Is.True,c.ToString());
        }
        [Test]
        public void BuildCopiesTrimOnlyUnusedVisualsAndLeaveOriginalDefinitionsUnchanged()
        {
            var original=AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
            var before=original.Characters.Select(JsonUtility.ToJson).ToArray();
            try
            {
                var packed=WebGLScenePacking.CopyForBuild(original);Assert.That(packed,Is.Not.SameAs(original));
                for(int i=0;i<9;i++)
                {
                    var source=original.Character(i);var copy=packed.Character(i);
                    Assert.That(copy,Is.Not.SameAs(source));Assert.That(copy.SelectionSprite,Is.SameAs(source.SelectionSprite));
                    Assert.That(copy.GroundNormals,Is.EqualTo(source.GroundNormals));Assert.That(copy.Specials,Is.EqualTo(source.Specials));Assert.That(copy.Ultimate,Is.SameAs(source.Ultimate));
                    foreach(var pose in copy.PaperMotion.Poses)foreach(var frame in pose.Frames)
                        Assert.That(source.PaperMotion.Poses.SelectMany(p=>p.Frames),Does.Contain(frame));
                    var a=new BattleSimulation(source.ToDefinition(),original.Character(0).ToDefinition(),PhaseOneDefaults.Rules(),original.Stages[0].ToGeometry());
                    var b=new BattleSimulation(copy.ToDefinition(),packed.Character(0).ToDefinition(),PhaseOneDefaults.Rules(),packed.Stages[0].ToGeometry());
                    for(int tick=0;tick<240;tick++)
                    {
                        var input=new FrameInput(tick<60?1:0,0,InputButtons.None,tick%40==0?InputButtons.Attack:InputButtons.None);
                        a.Tick(input,default);b.Tick(input,default);
                        Assert.That(b.P1.Position,Is.EqualTo(a.P1.Position));Assert.That(b.P2.Hearts,Is.EqualTo(a.P2.Hearts));Assert.That(b.P2.NormalHitCount,Is.EqualTo(a.P2.NormalHitCount));
                    }
                }
                Assert.That(original.Characters.Select(JsonUtility.ToJson),Is.EqualTo(before));
                Assert.That(packed.Stages.All(s=>s.GeneratedArt&&s.FarArtwork==null),Is.True);
            }
            finally{WebGLScenePacking.ReleaseCopies();}
        }
    }
}
