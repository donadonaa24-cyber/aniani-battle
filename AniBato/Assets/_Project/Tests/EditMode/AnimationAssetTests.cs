using System;
using System.IO;
using System.Linq;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class AnimationAssetTests
    {
        [Serializable] private sealed class Manifest { public Entry[] animations = Array.Empty<Entry>(); }
        [Serializable] private sealed class Entry { public string name = ""; public int frames = 0, fps = 0, inbetweenFrames = 0; public bool loop = false; public int OutputFrames => inbetweenFrames > 0 ? inbetweenFrames : frames; }
        [TestCase("toma", "Touma", 124)]
        [TestCase("ren", "Ren", 100)]
        [TestCase("chizuru", "Chizuru", 112)]
        public void ArtThreeIdleHasBodyBreathingWithFixedFeetSizeAndFacePixels(string key, string assetName, int standingHeight)
        {
            const int width = 224, height = 160, count = 8;
            var character = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/" + assetName + ".asset");
            var clip = character.Animations.Find("idle");
            Assert.That(clip.Fps, Is.EqualTo(6)); Assert.That(clip.Loop, Is.True);
            Assert.That(clip.Frames.Length, Is.EqualTo(count));
            Assert.That(clip.HeadHeights, Is.EqualTo(new[] { standingHeight, standingHeight + 1, standingHeight + 2, standingHeight + 1,
                standingHeight, standingHeight - 1, standingHeight - 2, standingHeight - 1 }));
            foreach (var motion in character.Animations.Clips) foreach (var frame in motion.Frames)
            {
                Assert.That(frame.rect.size, Is.EqualTo(new Vector2(width, height)), motion.Name);
                Assert.That(frame.pixelsPerUnit, Is.EqualTo(50), motion.Name);
            }
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/_Project/Art/Sprites/" + key + "/idle.png")), Is.True);
                Assert.That(texture.width, Is.EqualTo(width * count)); Assert.That(texture.height, Is.EqualTo(height));
                var pixels = texture.GetPixels32(); var colors = new System.Collections.Generic.HashSet<Color32>();
                int[] offsets = { 0, -1, -2, -1, 0, 1, 2, 1 }; int torsoChanges = 0;
                for (int frame = 0; frame < count; frame++) for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                {
                    Color32 color = pixels[y * texture.width + frame * width + x];
                    Assert.That(color.a, Is.EqualTo(0).Or.EqualTo(255)); if (color.a != 0) colors.Add(color);
                    if (y < 12) Assert.That(color, Is.EqualTo(pixels[y * texture.width + x]), "Shoes must stay planted.");
                    int sourceY = y + offsets[frame];
                    if (y >= 28 && sourceY >= 0 && sourceY < height)
                        Assert.That(color, Is.EqualTo(pixels[sourceY * texture.width + x]), "Face and torso must translate without changing pixels.");
                    if (frame == 2 && y >= 28 && y < standingHeight / 2 && !color.Equals(pixels[y * texture.width + x])) torsoChanges++;
                }
                Assert.That(torsoChanges, Is.GreaterThan(0), "The torso must breathe, not only the head.");
                Assert.That(colors.Count, Is.LessThanOrEqualTo(32));
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }
        [TestCase("toma", "Touma", 91)]
        [TestCase("ren", "Ren", 77)]
        [TestCase("chizuru", "Chizuru", 84)]
        public void ArtThreeCrouchPreservesPixelsFeetScaleAndCrouchStateBinding(string key, string assetName, int crouchHeight)
        {
            const int width = 224, height = 160, count = 8;
            var character = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/" + assetName + ".asset");
            var set = character.Animations; var clip = set.Find("crouch");
            Assert.That(clip.Fps, Is.EqualTo(6)); Assert.That(clip.Loop, Is.True);
            Assert.That(clip.Frames.Length, Is.EqualTo(count));
            Assert.That(clip.HeadHeights, Is.EqualTo(new[] { crouchHeight, crouchHeight + 1, crouchHeight + 2, crouchHeight + 1,
                crouchHeight, crouchHeight - 1, crouchHeight - 2, crouchHeight - 1 }));
            for (int frame = 0; frame < count; frame++)
            {
                var sample = new AnimationSample { State = AnimationState.Crouch, Age = frame * 10 };
                Assert.That(set.TrySprite(sample, out Sprite sprite), Is.True);
                Assert.That(sprite, Is.SameAs(clip.Frames[frame]), "Crouch must use every new frame at 6fps.");
                Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(width, height)));
                Assert.That(sprite.pixelsPerUnit, Is.EqualTo(50));
                Assert.That(sprite.pivot, Is.EqualTo(new Vector2(112, 0)));
            }
            Assert.That(set.TrySprite(new AnimationSample { State = AnimationState.Crouch, Age = 80 }, out Sprite wrapped), Is.True);
            Assert.That(wrapped, Is.SameAs(clip.Frames[0]));
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/_Project/Art/Sprites/" + key + "/crouch.png")), Is.True);
                Assert.That(texture.width, Is.EqualTo(width * count)); Assert.That(texture.height, Is.EqualTo(height));
                var pixels = texture.GetPixels32(); var colors = new System.Collections.Generic.HashSet<Color32>();
                int[] offsets = { 0, -1, -2, -1, 0, 1, 2, 1 }; int bodyChanges = 0;
                for (int frame = 0; frame < count; frame++) for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                {
                    Color32 color = pixels[y * texture.width + frame * width + x];
                    Assert.That(color.a, Is.EqualTo(0).Or.EqualTo(255)); if (color.a != 0) colors.Add(color);
                    if (y < 12) Assert.That(color, Is.EqualTo(pixels[y * texture.width + x]), "Crouch shoes must stay planted.");
                    int sourceY = y + offsets[frame];
                    if (y >= 28 && sourceY >= 0 && sourceY < height)
                        Assert.That(color, Is.EqualTo(pixels[sourceY * texture.width + x]), "Crouch face and torso must keep their pixels.");
                    if (frame == 2 && y >= 28 && y < crouchHeight / 2 && !color.Equals(pixels[y * texture.width + x])) bodyChanges++;
                }
                Assert.That(bodyChanges, Is.GreaterThan(0));
                Assert.That(colors.Count, Is.LessThanOrEqualTo(32));
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }
        private static System.Collections.Generic.IEnumerable<object[]> ActionCases()
        {
            string[] keys = { "toma", "ren", "chizuru" }, assets = { "Touma", "Ren", "Chizuru" };
            string[] clips = { "jump", "fall", "land", "guard", "dodge", "dash" };
            AnimationState[] states = { AnimationState.Jump, AnimationState.Fall, AnimationState.Land, AnimationState.Guard, AnimationState.Dodge, AnimationState.Dash };
            int[] counts = { 6, 6, 2, 6, 3, 8 };
            bool[] loops = { false, true, false, true, false, true };
            for (int character = 0; character < 3; character++) for (int c = 0; c < clips.Length; c++)
                yield return new object[] { keys[character], assets[character], clips[c], states[c], counts[c], c == 4 ? 12 : 15, loops[c] };
        }
        [TestCaseSource(nameof(ActionCases))]
        public void NewActionClipsUseEveryFixedFrameAtOriginalTiming(string key, string assetName, string name, AnimationState state, int count, int fps, bool loop)
        {
            var set = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/" + assetName + ".asset").Animations;
            var clip = set.Find(name);
            Assert.That(clip.Frames.Length, Is.EqualTo(count)); Assert.That(clip.Fps, Is.EqualTo(fps)); Assert.That(clip.Loop, Is.EqualTo(loop));
            for (int f = 0; f < count; f++)
            {
                var sample = new AnimationSample { State = state, Age = f * 60 / fps };
                Assert.That(set.TrySprite(sample, out Sprite sprite), Is.True); Assert.That(sprite, Is.SameAs(clip.Frames[f]));
                Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(224, 160))); Assert.That(sprite.pivot, Is.EqualTo(new Vector2(112, 0)));
                Assert.That(sprite.pixelsPerUnit, Is.EqualTo(50)); Assert.That(set.HeadHeight(sample), Is.EqualTo(clip.HeadHeights[f]));
            }
            Assert.That(set.TrySprite(new AnimationSample { State = state, Age = count * 60 / fps }, out Sprite end), Is.True);
            Assert.That(end, Is.SameAs(clip.Frames[loop ? 0 : count - 1]));
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/_Project/Art/Sprites/" + key + "/" + name + ".png")), Is.True);
                Assert.That(texture.width, Is.EqualTo(224 * count)); Assert.That(texture.height, Is.EqualTo(160));
                var pixels = texture.GetPixels32(); var colors = new System.Collections.Generic.HashSet<Color32>();
                for (int f = 0; f < count; f++)
                {
                    int top = -1, bottom = 160;
                    for (int y = 0; y < 160; y++) for (int x = 0; x < 224; x++)
                    {
                        Color32 color = pixels[y * texture.width + f * 224 + x]; Assert.That(color.a, Is.EqualTo(0).Or.EqualTo(255));
                        if (color.a == 0) continue; colors.Add(color); top = Math.Max(top, y); bottom = Math.Min(bottom, y);
                        Assert.That(x, Is.InRange(2, 221)); Assert.That(y, Is.LessThanOrEqualTo(156));
                    }
                    Assert.That(clip.HeadHeights[f], Is.EqualTo(top + 1));
                    if (name != "jump" && name != "fall") Assert.That(bottom, Is.Zero, "Grounded actions must keep the foot baseline.");
                }
                Assert.That(colors.Count, Is.LessThanOrEqualTo(32));
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }
        [TestCase("toma", "Touma")][TestCase("ren", "Ren")][TestCase("chizuru", "Chizuru")]
        public void RealJumpStartupAndLandingUseShallowPoseThenApprovedIdle(string key, string assetName)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/" + assetName + ".asset"); var set = data.Animations;
            var rules = AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
            Assert.That(rules.GroundJumpStartupFrames, Is.EqualTo(2)); Assert.That(set.Timing.LandingTicks, Is.EqualTo(8));
            var sim = new AniBato.Battle.BattleSimulation(data.ToDefinition(), data.ToDefinition(), rules, AniBato.Battle.PhaseOneDefaults.Stage());
            var player = new FighterAnimationPlayer(); player.Configure(set.Timing); player.Reset(sim.P1, sim.Match);
            sim.Tick(new AniBato.Battle.FrameInput(pressed: AniBato.Battle.InputButtons.Jump), default);
            for (int tick = 0; tick < 2; tick++)
            {
                player.Advance(sim.P1, sim.Match, false, false, false);
                Assert.That(player.Sample.State, Is.EqualTo(AnimationState.JumpStartup)); Assert.That(sim.P1.Grounded, Is.True);
                Assert.That(set.TrySprite(player.Sample, out Sprite sprite), Is.True); Assert.That(sprite, Is.SameAs(set.Find("land").Frames[0]));
                sim.Tick(default, default);
            }
            Assert.That(sim.P1.Grounded, Is.False); player.Advance(sim.P1, sim.Match, false, false, false);
            Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Jump));
            int flight = 0;
            while (!sim.P1.Grounded && flight++ < 180) { sim.Tick(default, default); player.Advance(sim.P1, sim.Match, false, false, false); }
            Assert.That(sim.P1.Grounded, Is.True);
            for (int tick = 0; tick < 8; tick++)
            {
                Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Land));
                Assert.That(set.TrySprite(player.Sample, out Sprite sprite), Is.True); Assert.That(sprite, Is.SameAs(set.Find("land").Frames[tick < 4 ? 0 : 1]));
                sim.Tick(default, default); player.Advance(sim.P1, sim.Match, false, false, false);
            }
            Assert.That(player.Sample.State, Is.EqualTo(AnimationState.Idle));
            var land = new Texture2D(2, 2); var idle = new Texture2D(2, 2);
            try
            {
                Assert.That(ImageConversion.LoadImage(land, File.ReadAllBytes("Assets/_Project/Art/Sprites/" + key + "/land.png")), Is.True);
                Assert.That(ImageConversion.LoadImage(idle, File.ReadAllBytes("Assets/_Project/Art/Sprites/" + key + "/idle.png")), Is.True);
                var lp = land.GetPixels32(); var ip = idle.GetPixels32();
                for (int y = 0; y < 160; y++) for (int x = 0; x < 224; x++)
                    Assert.That(lp[y * land.width + 224 + x], Is.EqualTo(ip[y * idle.width + x]), "Landing must return to the exact approved idle drawing.");
            }
            finally { UnityEngine.Object.DestroyImmediate(land); UnityEngine.Object.DestroyImmediate(idle); }
        }
        [TestCase("toma", "Touma", 124, "toma-keyvisual-cutin.png")]
        [TestCase("ren", "Ren", 100, "ren-dog-rook.png")]
        [TestCase("chizuru", "Chizuru", 112, "chizuru-casual-cutin.png")]
        public void EveryManifestFrameIsImportedAndBoundToCharacter(string key, string assetName, int height, string portrait)
        {
            string root = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(root, "art_source/" + key + ".manifest.json")));
            var character = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/" + assetName + ".asset");
            Assert.That(character, Is.Not.Null); Assert.That(character.Animations, Is.Not.Null, "先にBuildAllを実行してください。");
            var set = character.Animations; Assert.That(set.Clips.Length, Is.EqualTo(manifest.animations.Length));
            Assert.That(set.Clips.Sum(clip => clip.Frames.Length), Is.EqualTo(manifest.animations.Sum(entry => entry.OutputFrames)));
            Assert.That(set.StandingHeight, Is.EqualTo(height));
            foreach (var entry in manifest.animations)
            {
                string path = AniBato.EditorTools.AnimationSetBuilder.SpriteRoot + key + "/" + entry.name;
                var metadata = JsonUtility.FromJson<SpriteSheetMetadata>(File.ReadAllText(path + ".json"));
                var clip = set.Find(entry.name);
                Assert.That(clip, Is.Not.Null, entry.name); Assert.That(clip.Valid, Is.True, entry.name);
                Assert.That(metadata.frameCount, Is.EqualTo(entry.OutputFrames)); Assert.That(clip.Frames.Length, Is.EqualTo(entry.OutputFrames));
                Assert.That(clip.Fps, Is.EqualTo(entry.fps)); Assert.That(clip.Loop, Is.EqualTo(entry.loop));
                Assert.That(AssetDatabase.LoadAllAssetsAtPath(path + ".png").OfType<Sprite>().Count(), Is.EqualTo(entry.OutputFrames));
                for (int i = 0; i < entry.OutputFrames; i++)
                {
                    Assert.That(clip.Frames[i].name, Is.EqualTo(entry.name + "_" + i.ToString("D2")));
                    Assert.That(clip.Frames[i].rect, Is.EqualTo(new Rect(i * metadata.cellWidth, 0, metadata.cellWidth, metadata.cellHeight)));
                    Assert.That(clip.Frames[i].pixelsPerUnit, Is.EqualTo(50));
                    Assert.That(clip.Frames[i].pivot.x, Is.EqualTo(metadata.pivotX * metadata.cellWidth).Within(0.001));
                    Assert.That(clip.Frames[i].pivot.y, Is.EqualTo(metadata.pivotY * metadata.cellHeight).Within(0.001));
                }
                var importer = (TextureImporter)AssetImporter.GetAtPath(path + ".png");
                Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point)); Assert.That(importer.mipmapEnabled, Is.False);
                Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            }
            foreach (AnimationState state in Enum.GetValues(typeof(AnimationState)))
            {
                Assert.That(set.Bindings.Count(binding => binding.State == state), Is.EqualTo(1));
                Assert.That(set.TrySprite(new AnimationSample { State = state }, out Sprite sprite), Is.True); Assert.That(sprite, Is.Not.Null);
            }
            Assert.That(character.Portrait, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(character.Portrait), Does.EndWith(portrait));
            if (key != "chizuru")
            { Assert.That(character.SpriteEffects, Is.Not.Null); Assert.That(character.SpriteEffects.SchemaVersion, Is.EqualTo(1)); }
            if (character.SpriteEffects != null && character.SpriteEffects.SchemaVersion >= 1)
            {
                foreach (string effect in new[] { character.SpriteEffects.ProjectileClip, character.SpriteEffects.DownSpecialClip, character.SpriteEffects.UltimateClip }.Where(value => !String.IsNullOrEmpty(value)))
                    Assert.That(set.Find(effect) != null && set.Find(effect).Valid, Is.True, effect);
                Assert.That(character.SpriteEffects.DownSpecialOpacity, Is.GreaterThan(0).And.LessThanOrEqualTo(1));
            }
        }
        [TestCase("toma")] [TestCase("ren")] [TestCase("chizuru")]
        public void ReimportKeepsSpriteIdentifiersAndReferencesStable(string key)
        {
            string path = "Assets/_Project/Art/Sprites/" + key + "/idle.png";
            var before = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
            Assert.That(before.Length, Is.EqualTo(8));
            var ids = before.Select(sprite => { AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long id); return guid + "/" + id; }).ToArray();
            var actionIds = new System.Collections.Generic.Dictionary<string, string[]>();
            foreach (string name in new[] { "jump", "fall", "land", "guard", "dodge", "dash" })
            {
                string actionPath = "Assets/_Project/Art/Sprites/" + key + "/" + name + ".png";
                actionIds[actionPath] = AssetDatabase.LoadAllAssetsAtPath(actionPath).OfType<Sprite>().OrderBy(sprite => sprite.name)
                    .Select(sprite => { AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long id); return guid + "/" + id; }).ToArray();
            }
            var setBefore = AssetDatabase.LoadAssetAtPath<CharacterAnimationSet>("Assets/_Project/Data/" + key + "AnimationSet.asset");
            var timing = setBefore.Timing;
            var setAfter = AniBato.EditorTools.AnimationSetBuilder.Build(key); AssetDatabase.SaveAssets();
            Assert.That(setAfter.Timing, Is.EqualTo(timing));
            var after = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
            var afterIds = after.Select(sprite => { AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long id); return guid + "/" + id; }).ToArray();
            Assert.That(afterIds, Is.EqualTo(ids));
            foreach (var entry in actionIds)
            {
                var actual = AssetDatabase.LoadAllAssetsAtPath(entry.Key).OfType<Sprite>().OrderBy(sprite => sprite.name)
                    .Select(sprite => { AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long id); return guid + "/" + id; }).ToArray();
                Assert.That(actual, Is.EqualTo(entry.Value), entry.Key);
            }
        }
        [TestCase(0, "Touma")] [TestCase(1, "Ren")] [TestCase(2, "Chizuru")]
        public void ImportedFramesFreezeAndFlipWithTheirCharacterSample(int index, string assetName)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/" + assetName + ".asset");
            Assert.That(data, Is.Not.Null); Assert.That(data.Animations, Is.Not.Null);
            var set = data.Animations;
            var sim = new AniBato.Battle.BattleSimulation(data.ToDefinition(), AniBato.Battle.PhaseThreeDefaults.Character((index + 1) % 3), AniBato.Battle.PhaseOneDefaults.Rules(), AniBato.Battle.PhaseOneDefaults.Stage());
            var player = new FighterAnimationPlayer(); player.Configure(set.Timing); player.Reset(sim.P1, sim.Match);
            for (int tick = 0; tick < 10; tick++) player.Advance(sim.P1, sim.Match, false, false, false);
            Assert.That(set.TrySprite(player.Sample, out Sprite before), Is.True);
            float head = set.HeadHeight(player.Sample); Assert.That(head, Is.GreaterThan(0));
            sim.P1.Facing = -1; sim.P1.Mode = AniBato.Battle.FighterMode.Move;
            for (int tick = 0; tick < 8; tick++)
            {
                player.Advance(sim.P1, sim.Match, true, false, false);
                Assert.That(set.TrySprite(player.Sample, out Sprite frozen), Is.True);
                Assert.That(frozen, Is.SameAs(before)); Assert.That(set.HeadHeight(player.Sample), Is.EqualTo(head));
                Assert.That(player.Sample.FlipX, Is.False);
            }
            player.Advance(sim.P1, sim.Match, false, false, false);
            Assert.That(player.Sample.FlipX, Is.True); Assert.That(set.TrySprite(player.Sample, out Sprite moving), Is.True);
            Assert.That(moving.name, Does.StartWith("walk_"));
            foreach (var clip in set.Clips)
            {
                Assert.That(clip.HeadHeights.Length, Is.EqualTo(clip.Frames.Length));
                Assert.That(clip.HeadHeights.All(value => value > 0 && value <= clip.Frames[0].rect.height), Is.True);
            }
        }
    }
}


