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
            var setBefore = AssetDatabase.LoadAssetAtPath<CharacterAnimationSet>("Assets/_Project/Data/" + key + "AnimationSet.asset");
            var timing = setBefore.Timing;
            var setAfter = AniBato.EditorTools.AnimationSetBuilder.Build(key); AssetDatabase.SaveAssets();
            Assert.That(setAfter.Timing, Is.EqualTo(timing));
            var after = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
            var afterIds = after.Select(sprite => { AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long id); return guid + "/" + id; }).ToArray();
            Assert.That(afterIds, Is.EqualTo(ids));
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


