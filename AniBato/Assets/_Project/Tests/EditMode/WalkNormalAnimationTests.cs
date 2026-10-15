using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class WalkNormalAnimationTests
    {
        [TestCase("toma", "Touma")][TestCase("ren", "Ren")][TestCase("chizuru", "Chizuru")]
        public void GroundComboUsesNewWindupHitAndReturnAtUnchangedSimulationPhases(string key, string assetName)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/" + assetName + ".asset");
            var set = data.Animations;
            var rules = AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
            var sim = new BattleSimulation(data.ToDefinition(), data.ToDefinition(), rules, PhaseOneDefaults.Stage());
            // Keep this animation-phase test outside the new gun's full travel range.
            sim.P2.Position = new BattleVector(sim.Stage[0].Bounds.Right - 1, sim.P2.Position.Y);
            var opener = data.GroundNormals[0].ToDefinition();
            if (opener.SpawnProjectile)
                Assert.That(sim.P2.Hurtbox.Left - sim.P1.Position.X, Is.GreaterThan(opener.Hitbox.Right +
                    opener.ProjectileSpeed * opener.ProjectileLifetimeFrames / 60f + opener.NormalForwardDistance * 3));
            var player = new FighterAnimationPlayer(); player.Configure(set.Timing); player.Reset(sim.P1, sim.Match);
            int[] startups = new int[3], active = new int[3], recoveries = new int[3];
            int[] expectedStartup = data.GroundNormals.Select(n => n.ToDefinition().StartupFrames).ToArray();
            bool[] queued = new bool[3];
            for (int tick = 0; tick < 200; tick++)
            {
                bool press = tick == 0;
                if (sim.P1.Attack != null && sim.P1.Attack.ChainIndex < 2 && !queued[sim.P1.Attack.ChainIndex])
                { queued[sim.P1.Attack.ChainIndex] = true; press = true; }
                sim.Tick(new FrameInput(pressed: press ? InputButtons.Attack : InputButtons.None), default);
                player.Advance(sim.P1, sim.Match, false, false, false);
                var attack = sim.P1.Attack;
                if (attack == null) { if (tick > 0) break; continue; }
                int chain = attack.ChainIndex; string name = "attack" + (chain + 1);
                var clip = set.Find(name); var binding = set.Bindings.Single(b => b.Clip == name);
                Assert.That(binding.ActiveFrame, Is.EqualTo(2)); Assert.That(clip.Frames.Length, Is.EqualTo(6));
                Assert.That(clip.Fps, Is.EqualTo(15)); Assert.That(clip.Loop, Is.False);
                Assert.That(player.Sample.State, Is.EqualTo((AnimationState)((int)AnimationState.Attack1 + chain)));
                Assert.That(player.Sample.IsAttack, Is.True); Assert.That(player.Sample.AttackFrame, Is.EqualTo(attack.Frame));
                Assert.That(attack.Definition.StartupFrames, Is.EqualTo(expectedStartup[chain]));
                Assert.That(set.TrySprite(player.Sample, out Sprite sprite), Is.True);
                if (attack.Frame < attack.Definition.StartupFrames)
                {
                    startups[chain]++; Assert.That(attack.IsActive, Is.False);
                    Assert.That(sprite, Is.SameAs(clip.Frames[0]).Or.SameAs(clip.Frames[1]));
                }
                else if (attack.IsActive)
                { active[chain]++; Assert.That(sprite, Is.SameAs(clip.Frames[2]), "Only the active attack may show its extended hit pose."); }
                else
                { recoveries[chain]++; Assert.That(System.Array.IndexOf(clip.Frames, sprite), Is.InRange(3, 5)); }
                Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(224, 160))); Assert.That(sprite.pixelsPerUnit, Is.EqualTo(50));
            }
            for (int chain = 0; chain < 3; chain++)
            {
                Assert.That(startups[chain], Is.GreaterThan(0), "All three ground normals must actually start.");
                Assert.That(active[chain], Is.EqualTo(data.GroundNormals[chain].ToDefinition().ActiveFrames));
                Assert.That(recoveries[chain], Is.GreaterThan(0));
            }
            Assert.That(sim.P2.Hearts, Is.EqualTo(rules.InitialHearts)); Assert.That(sim.P2.NormalHitCount, Is.Zero);
        }

        [TestCase("toma")][TestCase("ren")][TestCase("chizuru")]
        public void ImportedWalkAndNormalSheetsExactlyMatchApprovedFixedKeyPixels(string key)
        {
            string root = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
            foreach (string name in new[] { "walk", "dash", "attack1", "attack2", "attack3" })
            {
                int[] keys = name == "walk" ? new[] {1,2,3,4,5,6,7,8} : name == "dash" ? new[] {1,1,2,2,3,3,4,4} : new[] {1,1,2,3,3,3};
                var sheet = new Texture2D(2, 2); var reference = new Texture2D(2, 2);
                try
                {
                    Assert.That(ImageConversion.LoadImage(sheet, File.ReadAllBytes("Assets/_Project/Art/Sprites/" + key + "/" + name + ".png")), Is.True);
                    Assert.That(sheet.width, Is.EqualTo(224 * keys.Length)); Assert.That(sheet.height, Is.EqualTo(160));
                    var pixels = sheet.GetPixels32(); var colors = new System.Collections.Generic.HashSet<Color32>();
                    var walkingKeys = new System.Collections.Generic.List<Color32[]>();
                    for (int frame = 0; frame < keys.Length; frame++)
                    {
                        string group = name == "walk" && keys[frame] % 2 == 0 ? "walk_inbetweens" : name;
                        int sourceIndex = name == "walk" ? (keys[frame] + 1) / 2 : keys[frame];
                        string file = Path.Combine(root, "art_source/art3_walk_attacks/normalized/" + key + "/" + group + "_key_" + sourceIndex.ToString("D2") + ".png");
                        Assert.That(ImageConversion.LoadImage(reference, File.ReadAllBytes(file)), Is.True);
                        Assert.That(reference.width, Is.EqualTo(224)); Assert.That(reference.height, Is.EqualTo(160));
                        var approved = reference.GetPixels32(); bool foot = false; int bottom = 160, top = -1;
                        if (name == "walk")
                        {
                            Assert.That(walkingKeys.Any(p => p.SequenceEqual(approved)), Is.False, "All eight walking poses must be distinct drawings.");
                            walkingKeys.Add(approved);
                        }
                        for (int y = 0; y < 160; y++) for (int x = 0; x < 224; x++)
                        {
                            Color32 color = pixels[y * sheet.width + frame * 224 + x];
                            Assert.That(color, Is.EqualTo(approved[y * 224 + x]), "Fixed keys must never be independently resized or synthesized.");
                            Assert.That(color.a, Is.EqualTo(0).Or.EqualTo(255));
                            if (color.a == 0) continue; colors.Add(color); bottom = System.Math.Min(bottom, y); top = System.Math.Max(top, y);
                            Assert.That(x, Is.InRange(2, 221)); Assert.That(y, Is.LessThanOrEqualTo(156)); if (y == 0) foot = true;
                        }
                        if (name == "dash" && (keys[frame] == 2 || keys[frame] == 4))
                        {
                            Assert.That(foot, Is.False); Assert.That(bottom, Is.InRange(4, 32));
                            Assert.That(top + 1, Is.EqualTo((key == "toma" ? 124 : key == "ren" ? 100 : 112) + 2));
                        }
                        else if (key == "ren" && name == "attack1" && keys[frame] == 2)
                        {
                            Assert.That(foot, Is.False); Assert.That(bottom, Is.InRange(4, 64));
                            Assert.That(top + 1, Is.EqualTo(102));
                        }
                        else Assert.That(foot, Is.True, "Ground contacts including the defined hood/arm supports must touch the common baseline.");
                        bool inverted = key == "ren" && (name == "attack1" && keys[frame] == 3 || name == "attack2" || name == "attack3" && keys[frame] == 1);
                        int depth = key == "toma" ? 25 : key == "ren" ? 31 : 34;
                        int headLow = inverted ? 0 : top - depth + 1, headHigh = inverted ? depth - 1 : top;
                        int headLeft = 224, headRight = -1, cream = 0, floorCream = 0;
                        for (int y = headLow; y <= headHigh; y++) for (int x = 0; x < 224; x++)
                        {
                            Color32 color = approved[y * 224 + x]; if (color.a == 0) continue;
                            headLeft = System.Math.Min(headLeft, x); headRight = System.Math.Max(headRight, x);
                            if (color.r > 200 && color.g > 180 && color.b > 140)
                            { cream++; if (y < 4) floorCream++; }
                        }
                        int expectedHead = key == "toma" ? 37 : key == "ren" ? 58 : 43;
                        Assert.That(headRight - headLeft + 1, Is.InRange(expectedHead - 2, expectedHead + 2),
                            "Compare the actual head band: upper head when upright, floor-side hood when inverted.");
                        if (inverted)
                        {
                            Assert.That(cream, Is.GreaterThanOrEqualTo(10), "The original cream dog hood must be at the floor.");
                            Assert.That(floorCream, Is.GreaterThan(0), "The hood crown, rather than an enlarged shoe, supports the inverted pose.");
                            Assert.That(top, Is.GreaterThan(61), "The torso and kicking legs must extend above the floor-side hood.");
                        }
                    }
                    Assert.That(colors.Count, Is.LessThanOrEqualTo(32));
                }
                finally { Object.DestroyImmediate(sheet); Object.DestroyImmediate(reference); }
            }
        }
    }
}
