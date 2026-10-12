using System;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using AudioSettings = AniBato.Game.AudioSettings;

namespace AniBato.EditorTools
{
    public static class ProjectBuilder
    {
        private const string DataRoot = "Assets/_Project/Data";
        public const string ScenePath = "Assets/_Project/Scenes/Battle.unity";
        public const string TitleScenePath = "Assets/_Project/Scenes/Title.unity";

        [MenuItem("あにバト/プロジェクト生成（フェーズ5B）")]
        public static void BuildAll()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("再生を停止してから生成してください。");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureFolder(DataRoot); EnsureFolder("Assets/_Project/Scenes");
            ConfigurePlayerSettings();
            var rules = LoadOrCreate<BattleRules>("BattleRules");
            if (rules.Definition.MatchSeconds == 0) rules.Definition = PhaseFiveBDefaults.Rules();
            UpgradeRules(rules);
            EditorUtility.SetDirty(rules);
            var stage = LoadOrCreate<StageData>("Stage");
            if (stage.Platforms == null || stage.Platforms.Length == 0) SetStage(stage, PhaseOneDefaults.Stage());
            EditorUtility.SetDirty(stage);
            var stages = StageThemeBuilder.Build(stage);
            var inputBindings = LoadOrCreate<InputBindings>("InputBindings");
            if (inputBindings.P1.Jump == UnityEngine.InputSystem.Key.None)
            { inputBindings.P1 = InputBindings.DefaultP1; inputBindings.P2 = InputBindings.DefaultP2; }
            UpgradeBindings(inputBindings);
            EditorUtility.SetDirty(inputBindings);
            var presentation = LoadOrCreate<PresentationSettings>("Presentation");
            EditorUtility.SetDirty(presentation);
            var audioSettings = LoadOrCreate<AudioSettings>("AudioSettings"); EditorUtility.SetDirty(audioSettings);
            var cpuDifficulties = new CpuDifficultyData[3];
            for (int i = 0; i < 3; i++)
            {
                cpuDifficulties[i] = LoadOrCreate<CpuDifficultyData>("Cpu" + (CpuDifficulty)i);
                if (cpuDifficulties[i].Definition.DecisionFrames == 0) cpuDifficulties[i].Definition = CpuDefinition.ForDifficulty(i);
                cpuDifficulties[i].Definition = BalanceMigration.Cpu(cpuDifficulties[i].Definition, i);
                cpuDifficulties[i].ToDefinition(); EditorUtility.SetDirty(cpuDifficulties[i]);
            }
            var ground = CreateAttacks("Ground", false);
            var air = CreateAttacks("Air", true);
            // Keep phase 1/2 assets intact for regression and existing tuning.
            CreateCharacter("PlaceholderA", "仮キャラA", new Color(0.1f, 0.8f, 1), false, ground, air);
            CreateCharacter("PlaceholderB", "仮キャラB", new Color(1, 0.35f, 0.5f), true, ground, air);
            var roster = new[] { CreatePlayable("Touma", 0, new Color(0.1f, 0.8f, 1)),
                CreatePlayable("Ren", 1, new Color(1, 0.38f, 0.2f)), CreatePlayable("Chizuru", 2, new Color(0.4f, 0.85f, 0.3f)) };
            var setup = LoadOrCreate<BattleSetup>("BattleSetup");
            if (setup.Characters == null || setup.Characters.Length == 0) setup.Characters = roster;
            if (setup.Stages == null || setup.Stages.Length == 0) setup.Stages = stages;
            if (setup.CpuDifficulties == null || setup.CpuDifficulties.Length == 0) setup.CpuDifficulties = cpuDifficulties;
            if (setup.CpuDifficulties.Length != 3) throw new InvalidOperationException("CPU difficulty assets must have three entries.");
            foreach (var difficulty in setup.CpuDifficulties)
            { if (!difficulty) throw new InvalidOperationException("A CPU difficulty asset is missing."); difficulty.ToDefinition(); }
            var flowSettings = LoadOrCreate<GameFlowSettings>("GameFlowSettings");
            CharacterData p1 = setup.Character(setup.Player1Index), p2 = setup.Character(setup.Player2Index);
            foreach (var character in setup.Characters) character.ToDefinition();
            EditorUtility.SetDirty(setup);
            // Validate before saving a scene; preserve user-edited data on repeated BuildAll calls.
            rules.ToDefinition(); stage.ToGeometry(); p1.ToDefinition(); p2.ToDefinition();
            AssetDatabase.SaveAssets();
            foreach (var theme in setup.Stages) theme.ToGeometry();
            BuildScene(p1, p2, rules, stage, inputBindings, presentation, setup, null);
            // NewScene(Single) unloads assets referenced only from C#; reload before building the Title scene.
            flowSettings = AssetDatabase.LoadAssetAtPath<GameFlowSettings>(DataRoot + "/GameFlowSettings.asset");
            if (!flowSettings) throw new InvalidOperationException("GameFlowSettings.asset could not be loaded for the Title scene.");
            BuildScene(p1, p2, rules, stage, inputBindings, presentation, setup, flowSettings);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(TitleScenePath, true), new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "_Project/Scenes/Title.unity")) ||
                !System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "_Project/Scenes/Battle.unity")))
                throw new InvalidOperationException("Title/Battle scene generation is incomplete.");
            Debug.Log("あにあにバトル！！ フェーズ5B生成完了: " + TitleScenePath);
        }

        private static void UpgradeRules(BattleRules asset)
        {
            var definition = asset.Definition;
            var defaults = PhaseOneDefaults.Rules();
            if (definition.SchemaVersion < 2)
            {
                definition.Defense = defaults.Defense; definition.Gauge = defaults.Gauge;
                definition.ComboFinishKnockback = defaults.ComboFinishKnockback;
                definition.ComboFinishHitstunFrames = defaults.ComboFinishHitstunFrames;
                definition.FollowupImmunityFrames = defaults.FollowupImmunityFrames;
            }
            if (definition.SchemaVersion < 3) definition.Shrink = defaults.Shrink;
            if (definition.SchemaVersion < 4 && definition.Gauge.OnHit == 12 && definition.Gauge.OnDamaged == 8)
            { var gauge = definition.Gauge; gauge.OnHit = defaults.Gauge.OnHit; gauge.OnDamaged = defaults.Gauge.OnDamaged; definition.Gauge = gauge; }
            definition.SchemaVersion = Math.Max(4, definition.SchemaVersion); asset.Definition = BalanceMigration.Rules(definition);
        }
        private static void UpgradeBindings(InputBindings asset)
        {
            if (asset.SchemaVersion >= 2) return;
            var keys = asset.P2; var defaults = InputBindings.DefaultP2;
            // Only replace the legacy default primary keys; custom assignments remain intact.
            if (keys.Jump == UnityEngine.InputSystem.Key.Numpad0) keys.Jump = defaults.Jump;
            if (keys.Attack == UnityEngine.InputSystem.Key.Numpad1) keys.Attack = defaults.Attack;
            if (keys.Special == UnityEngine.InputSystem.Key.Numpad2) keys.Special = defaults.Special;
            if (keys.Guard == UnityEngine.InputSystem.Key.Numpad3) keys.Guard = defaults.Guard;
            if (keys.Dodge == UnityEngine.InputSystem.Key.Numpad4) keys.Dodge = defaults.Dodge;
            if (keys.Counter == UnityEngine.InputSystem.Key.Numpad5) keys.Counter = defaults.Counter;
            if (keys.Ultimate == UnityEngine.InputSystem.Key.Numpad6) keys.Ultimate = defaults.Ultimate;
            if (keys.Dash == UnityEngine.InputSystem.Key.NumpadEnter) keys.Dash = defaults.Dash;
            keys.AlternateJump = AlternativesOrDefaults(keys.AlternateJump, defaults.AlternateJump);
            keys.AlternateAttack = AlternativesOrDefaults(keys.AlternateAttack, defaults.AlternateAttack);
            keys.AlternateSpecial = AlternativesOrDefaults(keys.AlternateSpecial, defaults.AlternateSpecial);
            keys.AlternateGuard = AlternativesOrDefaults(keys.AlternateGuard, defaults.AlternateGuard);
            keys.AlternateDodge = AlternativesOrDefaults(keys.AlternateDodge, defaults.AlternateDodge);
            keys.AlternateCounter = AlternativesOrDefaults(keys.AlternateCounter, defaults.AlternateCounter);
            keys.AlternateUltimate = AlternativesOrDefaults(keys.AlternateUltimate, defaults.AlternateUltimate);
            keys.AlternateDash = AlternativesOrDefaults(keys.AlternateDash, defaults.AlternateDash);
            asset.P2 = keys; asset.SchemaVersion = 2;
        }
        private static UnityEngine.InputSystem.Key[] AlternativesOrDefaults(UnityEngine.InputSystem.Key[] value, UnityEngine.InputSystem.Key[] defaults) =>
            value == null || value.Length == 0 ? defaults : value;

        private static T LoadOrCreate<T>(string name) where T : ScriptableObject
        {
            string path = DataRoot + "/" + name + ".asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }
        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/'); string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            { if (!AssetDatabase.IsValidFolder(current + "/" + parts[i])) AssetDatabase.CreateFolder(current, parts[i]); current += "/" + parts[i]; }
        }
        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "aniani"; PlayerSettings.productName = "あにあにバトル！！";
            PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
            PlayerSettings.allowFullscreenSwitch = true;
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            settings.FindProperty("activeInputHandler").intValue = 2; settings.ApplyModifiedPropertiesWithoutUndo();
        }
        private static AttackData[] CreateAttacks(string prefix, bool airborne)
        {
            var attacks = new AttackData[3];
            for (int i = 0; i < attacks.Length; i++)
            {
                var attack = LoadOrCreate<AttackData>(prefix + "Normal" + (i + 1));
                if (attack.Definition.ActiveFrames == 0)
                    attack.Definition = PhaseOneDefaults.Normal((airborne ? "空中" : "地上") + (i + 1) + "段");
                EditorUtility.SetDirty(attack); attacks[i] = attack;
            }
            return attacks;
        }
        private static CharacterData CreateCharacter(string assetName, string displayName, Color tint, bool round, AttackData[] ground, AttackData[] air)
        {
            var data = LoadOrCreate<CharacterData>(assetName);
            if (data.Movement.MaxJumps == 0)
            {
                CharacterDefinition definition = PhaseOneDefaults.Character(displayName);
                definition.GroundNormals = null; definition.AirNormals = null;
                definition.Specials = null; definition.Ultimate = default;
                data.Movement = definition; data.Tint = tint; data.RoundShape = round;
                data.GroundNormals = ground; data.AirNormals = air;
            }
            if (data.Specials == null || data.Specials.Length == 0)
            {
                data.Specials = new AttackData[4];
                for (int i = 0; i < 4; i++)
                {
                    var attack = LoadOrCreate<AttackData>(assetName + "Special" + (SpecialSlot)i);
                    if (attack.Definition.ActiveFrames == 0) attack.Definition = PhaseTwoDefaults.Special((SpecialSlot)i);
                    EditorUtility.SetDirty(attack); data.Specials[i] = attack;
                }
            }
            if (!data.Ultimate)
            {
                data.Ultimate = LoadOrCreate<AttackData>(assetName + "Ultimate");
                if (data.Ultimate.Definition.ActiveFrames == 0) data.Ultimate.Definition = PhaseTwoDefaults.Ultimate();
                EditorUtility.SetDirty(data.Ultimate);
            }
            EditorUtility.SetDirty(data); return data;
        }
        private static void SetStage(StageData asset, StageGeometry geometry)
        {
            asset.Platforms = new StagePlatform[geometry.PlatformCount];
            for (int i = 0; i < asset.Platforms.Length; i++) asset.Platforms[i] = geometry[i];
            asset.LeftBlastLine = geometry.LeftBlastLine; asset.RightBlastLine = geometry.RightBlastLine;
            asset.BottomBlastLine = geometry.BottomBlastLine; asset.SpawnP1 = geometry.SpawnP1; asset.SpawnP2 = geometry.SpawnP2;
        }
        private static CharacterData CreatePlayable(string name, int index, Color color)
        {
            var data = LoadOrCreate<CharacterData>(name);
            var defaults = PhaseFiveBDefaults.Character(index);
            if (data.Movement.MaxJumps == 0)
            {
                var movement = defaults; movement.GroundNormals = null; movement.AirNormals = null;
                movement.Specials = null; movement.Ultimate = default;
                data.Movement = movement; data.Tint = color; data.Silhouette = index; data.RoundShape = index == 1;
            }
            data.GroundNormals = PlayableAttacks(name + "Ground", data.GroundNormals, defaults.GroundNormals);
            data.AirNormals = PlayableAttacks(name + "Air", data.AirNormals, defaults.AirNormals);
            data.Specials = PlayableAttacks(name + "Special", data.Specials, defaults.Specials);
            if (data.TempoSchemaVersion < 5)
            {
                var old = PhaseThreeDefaults.Character(index);
                for (int slot = 0; slot < data.Specials.Length; slot++)
                {
                    data.Specials[slot].Definition = BalanceMigration.Special(data.Specials[slot].Definition, old.Specials[slot], defaults.Specials[slot]);
                    EditorUtility.SetDirty(data.Specials[slot]);
                }
                data.TempoSchemaVersion = 5;
            }
            if (!data.Ultimate) data.Ultimate = PlayableAttack(name + "Ultimate", defaults.Ultimate);
            string spriteCharacter = index == 0 ? "toma" : index == 1 ? "ren" : "chizuru";
            data.Animations = AnimationSetBuilder.Build(spriteCharacter);
            if (!data.Animations) throw new InvalidOperationException(defaults.Name + "のSpriteForge出力がありません。");
            if (index == 0)
            {
                data.Portrait = ImportPortrait("toma-keyvisual-cutin.png");
                if (data.SpriteEffects == null || data.SpriteEffects.SchemaVersion < 1) data.SpriteEffects = new CharacterSpriteEffects
                {
                    SchemaVersion = 1,
                    ProjectileClip = "pulse", ProjectileScale = 0.65f,
                    DownSpecialClip = "field", DownSpecialScale = 1.8f, DownSpecialOffset = Vector2.zero,
                    UltimateClip = "boundary_glow", UltimateScale = 1.2f, UltimateOffset = new Vector2(0.2f, 0),
                    SideSpecialTint = new Color(0.55f, 0.85f, 1)
                };
            }
            if (index == 1 && (data.SpriteEffects == null || data.SpriteEffects.SchemaVersion < 1)) data.SpriteEffects = new CharacterSpriteEffects
            { SchemaVersion = 1, UltimateClip = "cross_light", UltimateScale = 6.6f, UltimateOffset = Vector2.zero };
            if (index == 2)
            {
                data.Portrait = ImportPortrait("chizuru-casual-cutin.png");
            }
            if (!data.Portrait && index == 1)
                data.Portrait = ImportPortrait("ren-dog-rook.png");
            EditorUtility.SetDirty(data); return data;
        }
        private static AttackData[] PlayableAttacks(string prefix, AttackData[] existing, AttackDefinition[] defaults)
        {
            if (existing == null || existing.Length == 0) existing = new AttackData[defaults.Length];
            for (int i = 0; i < existing.Length && i < defaults.Length; i++)
                if (!existing[i]) existing[i] = PlayableAttack(prefix + (i + 1), defaults[i]);
            return existing;
        }
        private static AttackData PlayableAttack(string name, in AttackDefinition definition)
        {
            var asset = LoadOrCreate<AttackData>(name);
            if (asset.Definition.ActiveFrames == 0) asset.Definition = definition;
            EditorUtility.SetDirty(asset); return asset;
        }
        private static Sprite ImportPortrait(string name)
        {
            string path = "Assets/_Project/Art/Characters/" + name;
            AssetDatabase.ImportAsset(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) return null;
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void BuildScene(CharacterData p1, CharacterData p2, BattleRules rules, StageData stage,
            InputBindings bindings, PresentationSettings presentation, BattleSetup setup, GameFlowSettings flowSettings)
        {
            // Decide before NewScene: NewScene(Single) unloads assets that only C# locals reference (they become fake-null).
            bool isTitle = flowSettings != null;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Reload all serialized dependencies after NewScene, including new CPU/audio data.
            setup = AssetDatabase.LoadAssetAtPath<BattleSetup>(DataRoot + "/BattleSetup.asset");
            rules = AssetDatabase.LoadAssetAtPath<BattleRules>(DataRoot + "/BattleRules.asset");
            stage = AssetDatabase.LoadAssetAtPath<StageData>(DataRoot + "/Stage.asset");
            bindings = AssetDatabase.LoadAssetAtPath<InputBindings>(DataRoot + "/InputBindings.asset");
            presentation = AssetDatabase.LoadAssetAtPath<PresentationSettings>(DataRoot + "/Presentation.asset");
            var audioSettings = AssetDatabase.LoadAssetAtPath<AudioSettings>(DataRoot + "/AudioSettings.asset");
            if (!setup || !rules || !stage || !bindings || !presentation || !audioSettings) throw new InvalidOperationException("Scene assets could not be reloaded.");
            p1 = setup.Character(setup.Player1Index); p2 = setup.Character(setup.Player2Index);
            if (isTitle)
            {
                flowSettings = AssetDatabase.LoadAssetAtPath<GameFlowSettings>(DataRoot + "/GameFlowSettings.asset");
                setup = AssetDatabase.LoadAssetAtPath<BattleSetup>(DataRoot + "/BattleSetup.asset");
                if (!flowSettings || !setup) throw new InvalidOperationException("Title scene assets could not be reloaded.");
            }
            var systems = new GameObject("Battle Systems");
            var input = systems.AddComponent<InputController>(); input.Configure(bindings);
            var runner = systems.AddComponent<BattleRunner>(); runner.Configure(p1, p2, rules, stage, input);
            runner.ConfigureSetup(setup);
            runner.ConfigurePresentation(presentation);
            systems.AddComponent<AudioManager>().Configure(runner, audioSettings);
            systems.AddComponent<BattleEffectsView>().Configure(runner, presentation);
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true;
            cameraObject.AddComponent<AudioListener>();
            camera.orthographicSize = 8; camera.backgroundColor = presentation.Background;
            camera.clearFlags = CameraClearFlags.SolidColor; cameraObject.transform.position = new Vector3(0, 3, -10);
            cameraObject.AddComponent<CameraController>().Configure(runner, presentation);
            var world = new GameObject("Battle World");
            GameObject Child(string name) { var obj = new GameObject(name); obj.transform.SetParent(world.transform, false); return obj; }
            var stageView = Child("Stage").AddComponent<StageView>(); stageView.Configure(stage, presentation, runner);
            Child("Player 1").AddComponent<FighterView>().Configure(runner, p1, presentation, 0);
            Child("Player 2").AddComponent<FighterView>().Configure(runner, p2, presentation, 1);
            Child("Projectiles").AddComponent<ProjectileView>().Configure(runner, presentation);
            Child("Hitboxes").AddComponent<HitboxDebugView>().Configure(runner, presentation);
            var hud = BuildHud(runner, camera, presentation);
            if (isTitle) GameFlowBuilder.Build(runner, input, setup, flowSettings, world, hud, stageView);
            string path = isTitle ? TitleScenePath : ScenePath;
            if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("シーンを保存できませんでした。");
        }
        private static GameObject BuildHud(BattleRunner runner, Camera camera, PresentationSettings presentation)
        {
            var go = new GameObject("Battle HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Text left = CreateText(go.transform, font, "Player 1 HUD", 34, new Vector2(0, 1), new Vector2(0, 1), new Vector2(36, -24), new Vector2(720, 170), TextAnchor.UpperLeft);
            Text right = CreateText(go.transform, font, "Player 2 HUD", 34, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-36, -24), new Vector2(720, 170), TextAnchor.UpperRight);
            Text time = CreateText(go.transform, font, "Timer", 58, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(200, 100), TextAnchor.UpperCenter);
            Text result = CreateText(go.transform, font, "Match Result", 48, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500, 360), TextAnchor.MiddleCenter);
            result.enabled = false;
            Text counter1 = CreateText(go.transform, font, "P1 Head Counter", 28, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 60), TextAnchor.MiddleCenter);
            Text counter2 = CreateText(go.transform, font, "P2 Head Counter", 28, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 60), TextAnchor.MiddleCenter);
            Text popup1 = CreateText(go.transform, font, "P1 Battle Popup", 30, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 110), TextAnchor.MiddleCenter);
            Text popup2 = CreateText(go.transform, font, "P2 Battle Popup", 30, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 110), TextAnchor.MiddleCenter);
            popup1.enabled = popup2.enabled = false;
            Text help = CreateText(go.transform, font, "Controls", 22, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(1880, 88), TextAnchor.LowerCenter);
            help.text = "P1: A/D 移動 · Space 跳 · J 通常 · 方向+K 特殊 · L 防御 · Shift 回避 · I カウンター · U 超必殺 · F ダッシュ\nP2: ←/→ 移動 · 右Ctrl 跳 · Delete 通常 · 方向+End 特殊 · PageDown 防御 · 右Shift 回避 · PageUp カウンター · Home 超必殺 · 右Alt ダッシュ | F9 判定表示 · F11 全画面";
            var hud = go.AddComponent<HUDController>();
            hud.Configure(runner, camera, presentation, left, right, time, result, new[] { counter1, counter2 });
            hud.ConfigurePopups(new[] { popup1, popup2 });
            BuildCutIn(go, runner, font);
            return go;
        }
        private static void BuildCutIn(GameObject canvas, BattleRunner runner, Font font)
        {
            var panel = CreateImage(canvas.transform, "Ultimate CutIn", Vector2.zero, new Vector2(1920, 1080));
            panel.color = new Color(0, 0, 0, 0.8f);
            var labels = new Text[2]; var portraits = new Image[2]; var bands = new Image[2];
            for (int i = 0; i < 2; i++)
            {
                float y = i == 0 ? 180 : -180;
                bands[i] = CreateImage(panel.transform, "P" + (i + 1) + " CutIn Band", new Vector2(0, y), new Vector2(1600, 300));
                labels[i] = CreateText(panel.transform, font, "P" + (i + 1) + " Ultimate Name", 60, new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(180, y), new Vector2(1000, 260), TextAnchor.MiddleCenter);
                portraits[i] = CreateImage(panel.transform, "P" + (i + 1) + " Portrait", new Vector2(-570, y), new Vector2(280, 280));
                portraits[i].preserveAspect = true; portraits[i].enabled = false; bands[i].enabled = false;
            }
            panel.gameObject.SetActive(false);
            canvas.AddComponent<UltimateCutInView>().Configure(runner, panel.gameObject, labels, portraits, bands);
        }
        private static Image CreateImage(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.raycastTarget = false; return image;
        }
        private static Text CreateText(Transform parent, Font font, string name, int size, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 dimensions, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot; rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var text = go.GetComponent<Text>(); text.font = font; text.fontSize = size; text.alignment = alignment; text.color = Color.white;
            text.raycastTarget = false; text.supportRichText = false;
            var outline = go.AddComponent<Outline>(); outline.effectColor = new Color(0, 0, 0, 0.85f); outline.effectDistance = new Vector2(2, -2);
            return text;
        }
    }
}




