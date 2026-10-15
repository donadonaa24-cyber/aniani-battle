using AniBato.EditorTools;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace AniBato.Tests
{
    public sealed class ProjectAssetTests
    {
        [Test]
        public void GeneratedNormalCpuDataMeetsUltimateTargetAndBothFightersAttack()
        {
            const string root = "Assets/_Project/Data/";
            var setup = AssetDatabase.LoadAssetAtPath<BattleSetup>(root + "BattleSetup.asset");
            var rules = AssetDatabase.LoadAssetAtPath<BattleRules>(root + "BattleRules.asset");
            // Six prototype entrants share these three tuned move sets; test the three distinct sets here.
            var rows = CpuBalanceStudy.Run(setup.Characters.Take(3).Select(c => c.ToDefinition()).ToArray(), rules.ToDefinition(), setup.Stages[0].ToGeometry(),
                new[] { setup.CpuDifficulties[1].ToDefinition() });
            Assert.That(rows.Count, Is.EqualTo(270));
            Assert.That(rows.All(r => r.Attacks[0] > 0 && r.Attacks[1] > 0 && r.HeartsLost > 0), Is.True);
            Assert.That(rows.Count(r => r.UltimateStarts > 0) / (double)rows.Count, Is.GreaterThanOrEqualTo(.5));
        }
        [Test]
        public void GeneratedAssetsConvertToValidSimulationDefinitions()
        {
            const string root = "Assets/_Project/Data/";
            var rules = AssetDatabase.LoadAssetAtPath<BattleRules>(root + "BattleRules.asset");
            var stage = AssetDatabase.LoadAssetAtPath<StageData>(root + "Stage.asset");
            var p1 = AssetDatabase.LoadAssetAtPath<CharacterData>(root + "Touma.asset");
            var p2 = AssetDatabase.LoadAssetAtPath<CharacterData>(root + "Ren.asset");
            Assert.That(rules, Is.Not.Null, "先にBuildAllを実行してください。");
            Assert.That(stage, Is.Not.Null); Assert.That(p1, Is.Not.Null); Assert.That(p2, Is.Not.Null);
            var simulation = new AniBato.Battle.BattleSimulation(p1.ToDefinition(), p2.ToDefinition(), rules.ToDefinition(), stage.ToGeometry());
            Assert.That(simulation.Match.RemainingTicks, Is.EqualTo(4200));
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ProjectBuilder.ScenePath), Is.Not.Null);
            Assert.That(EditorBuildSettings.scenes[0].path, Is.EqualTo(ProjectBuilder.TitleScenePath));
            Assert.That(rules.Definition.SchemaVersion, Is.EqualTo(8));
            Assert.That(rules.Definition.Charge.Enabled, Is.True);
            Assert.That(rules.Definition.Charge.MinimumFrames, Is.EqualTo(60));
            Assert.That(rules.Definition.Charge.MaximumFrames, Is.EqualTo(180));
            Assert.That(rules.Definition.GroundJumpStartupFrames, Is.EqualTo(2));
            var setup = AssetDatabase.LoadAssetAtPath<BattleSetup>(root + "BattleSetup.asset");
            Assert.That(setup, Is.Not.Null); Assert.That(setup.Characters.Length, Is.EqualTo(9));
            Assert.That(setup.CpuDifficulties.Length, Is.EqualTo(3));
            foreach (var difficulty in setup.CpuDifficulties) Assert.DoesNotThrow(() => difficulty.ToDefinition());
            Assert.That(AssetDatabase.LoadAssetAtPath<AudioSettings>(root + "AudioSettings.asset"), Is.Not.Null);
            Assert.That(setup.Character(2).ToDefinition().Name, Is.EqualTo("千鶴"));
            foreach (var character in setup.Characters)
            {
                Assert.That(character.Specials.Length, Is.EqualTo(4));
                Assert.That(character.Specials.All(a => a && a.IsSpecial), Is.True);
                Assert.That(character.Ultimate.IsUltimate, Is.True);
                Assert.That(character.GroundNormals.Length, Is.EqualTo(3));
                Assert.That(character.DirectionalNormals.Length, Is.EqualTo(10));
                Assert.That(character.DirectionalNormals.Skip(1).All(a => a && a.IsNormal), Is.True);
                Assert.That(character.Movement.Stamina.Maximum, Is.GreaterThan(0));
                Assert.That(character.AccessibilitySchemaVersion, Is.EqualTo(6));
                Assert.That(character.NormalMotionSchemaVersion, Is.EqualTo(1));
                Assert.That(character.DamageReactionSchemaVersion, Is.EqualTo(1));
                Assert.That(character.NormalThrowSchemaVersion, Is.EqualTo(1));
                foreach (var normal in character.GroundNormals.Concat(character.AirNormals).Concat(character.DirectionalNormals.Skip(1)))
                    Assert.That(normal.Definition.NormalForwardDistance, Is.EqualTo(.18f), character.name + "/" + normal.name);
                foreach (var attack in character.GroundNormals.Concat(character.AirNormals).Concat(character.DirectionalNormals.Skip(1)).Concat(character.Specials).Concat(new[] { character.Ultimate }))
                {
                    Assert.That(attack.Definition.DamageRecoilDistance, Is.EqualTo(.18f));
                    Assert.That(attack.Definition.DamageRecoilFrames, Is.EqualTo(6));
                }
                Assert.That(character.Movement.UltimateCutInFrames, Is.EqualTo(30));
            }
            Assert.That(setup.Character(1).Portrait, Is.Not.Null); Assert.That(setup.Character(2).Portrait, Is.Not.Null);
            var chizuru = setup.Character(2);
            Assert.That(chizuru.NormalWeaponSchemaVersion, Is.EqualTo(1));
            Assert.That(chizuru.GroundNormals.Any(a => a.Definition.SpawnProjectile), Is.False);
            Assert.That(chizuru.GroundNormals[2].Definition.IsCloseRange, Is.True);
            // 2026-10-15: normal-attack reach halved toward the opponent (user request), see HitboxReach.
            Assert.That(chizuru.GroundNormals[2].Definition.Hitbox, Is.EqualTo(HitboxReach.HalveTowardOpponent(PhaseFiveBDefaults.Character(2).GroundNormals[2].Hitbox)));
            Assert.That(chizuru.SpriteEffects.NormalProjectileClip, Is.EqualTo("bag_throw"));
            Assert.That(chizuru.SpriteEffects.NormalProjectileScale, Is.EqualTo(1));
            Assert.That(chizuru.SpriteEffects.NormalProjectileSchemaVersion, Is.EqualTo(1));
            var bag = chizuru.Animations.Find("bag_throw");
            Assert.That(bag, Is.Not.Null); Assert.That(bag.Frames.Length, Is.EqualTo(4)); Assert.That(bag.Fps, Is.EqualTo(12));
            Assert.That(bag.Loop, Is.True);
            foreach (var frame in bag.Frames) { Assert.That(frame.rect.width, Is.EqualTo(224)); Assert.That(frame.rect.height, Is.EqualTo(160)); Assert.That(frame.pixelsPerUnit, Is.EqualTo(50)); }
        }
        [Test]
        public void StageThemesShareTheSameGeometryAndHaveThumbnailsAndShrinkRules()
        {
            var setup = AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
            Assert.That(setup.Stages.Length, Is.EqualTo(3));
            var geometry = setup.Stages[0].SharedGeometry;
            Assert.That(geometry, Is.Not.Null);
            for (int i = 0; i < setup.Stages.Length; i++)
            {
                Assert.That(setup.Stages[i].SharedGeometry, Is.SameAs(geometry));
                Assert.That(setup.Stages[i].Thumbnail, Is.Not.Null);
                Assert.That(setup.Stages[i].FarArtwork, Is.Not.Null);
                Assert.That(setup.Stages[i].AppearanceSchemaVersion, Is.EqualTo(2));
                Assert.That(setup.Stages[i].Thumbnail.texture.width, Is.EqualTo(480));
                Assert.That(setup.Stages[i].FarArtwork.texture.width, Is.EqualTo(1280));
                Assert.That(setup.Stages[i].Theme, Is.EqualTo((StageTheme)i));
                Assert.That(setup.Stages[i].ToGeometry().PlatformCount, Is.EqualTo(3));
            }
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ProjectBuilder.TitleScenePath), Is.Not.Null);
            var rules = AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
            Assert.That(rules.Shrink.Enabled, Is.True); Assert.That(rules.Shrink.StartRemainingSeconds, Is.EqualTo(10));
            Assert.That(AssetDatabase.LoadAssetAtPath<PresentationSettings>("Assets/_Project/Data/Presentation.asset").CameraSchemaVersion, Is.EqualTo(1));
        }
        [Test]
        public void KeyboardBindingsDoNotOverlapBetweenPlayers()
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputBindings>("Assets/_Project/Data/InputBindings.asset");
            Assert.That(asset, Is.Not.Null);
            var p1 = asset.P1; var p2 = asset.P2;
            var first = new[] { p1.Left, p1.Right, p1.Up, p1.Down, p1.Jump, p1.Attack, p1.Special, p1.Guard, p1.Dodge, p1.Counter, p1.Ultimate, p1.Dash };
            var second = new[] { p2.Left, p2.Right, p2.Up, p2.Down, p2.Jump, p2.Attack, p2.Special, p2.Guard, p2.Dodge, p2.Counter, p2.Ultimate, p2.Dash };
            Assert.That(first, Has.None.EqualTo(UnityEngine.InputSystem.Key.None));
            Assert.That(second, Has.None.EqualTo(UnityEngine.InputSystem.Key.None));
            foreach (var key in first) Assert.That(second, Has.None.EqualTo(key));
            var allFirst = AllKeys(p1).Where(k => k != Key.None).ToArray();
            var allSecond = AllKeys(p2).Where(k => k != Key.None).ToArray();
            foreach (var key in allFirst) Assert.That(allSecond, Has.None.EqualTo(key));
            Assert.That(asset.SchemaVersion, Is.EqualTo(2));
        }
        private static IEnumerable<Key> AllKeys(PlayerKeyBindings keys)
        {
            foreach (var key in new[] { keys.Left, keys.Right, keys.Up, keys.Down, keys.Jump, keys.Attack, keys.Special,
                keys.Guard, keys.Dodge, keys.Counter, keys.Ultimate, keys.Dash }) yield return key;
            foreach (var alternatives in new[] { keys.AlternateLeft, keys.AlternateRight, keys.AlternateUp, keys.AlternateDown,
                keys.AlternateJump, keys.AlternateAttack, keys.AlternateSpecial, keys.AlternateGuard,
                keys.AlternateDodge, keys.AlternateCounter, keys.AlternateUltimate, keys.AlternateDash })
                if (alternatives != null) foreach (var key in alternatives) yield return key;
        }
    }
}
