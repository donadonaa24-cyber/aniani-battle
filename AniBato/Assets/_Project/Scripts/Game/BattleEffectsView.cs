using AniBato.Battle;
using AniBato.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class BattleEffectsView : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private PresentationSettings settings;
        private SpriteRenderer[] sparks;
        private readonly float[] ages = new float[16];
        private readonly bool[] justSparks = new bool[16];
        private readonly int[] sparkPlayers = new int[16];
        private readonly Vector3[] sparkOffsets = new Vector3[16];
        private int nextSpark;
        private Image flash;
        private Text finish;
        public BattleFeedback Feedback { get; } = new BattleFeedback();
        public bool ScreenShakeEnabled { get; private set; }
        public void Configure(BattleRunner value, PresentationSettings data) { runner = value; settings = data; }
        private void Awake()
        {
            ScreenShakeEnabled = PlayerPrefs.GetInt("AniBato.ScreenShake", settings.ScreenShake ? 1 : 0) != 0;
            sparks = new SpriteRenderer[ages.Length];
            for (int i = 0; i < sparks.Length; i++) { sparks[i] = ProceduralSprites.Create(transform, "Hit Spark " + i, Vector2.one, Color.white, sortingOrder: 15); sparks[i].enabled = false; }
            var canvas = new GameObject("Battle Feedback Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvas.transform.SetParent(transform, false); canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<Canvas>().sortingOrder = 25;
            var scale = canvas.GetComponent<CanvasScaler>(); scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scale.referenceResolution = new Vector2(1920, 1080); scale.matchWidthOrHeight = .5f;
            var flashObject = new GameObject("Just Flash", typeof(RectTransform), typeof(Image)); flashObject.transform.SetParent(canvas.transform, false);
            var rect = flashObject.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            flash = flashObject.GetComponent<Image>(); flash.raycastTarget = false; flash.enabled = false;
            var label = new GameObject("Finish Banner", typeof(RectTransform), typeof(Text)); label.transform.SetParent(canvas.transform, false);
            rect = label.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(1500, 220); rect.anchoredPosition = Vector2.zero;
            finish = label.GetComponent<Text>(); finish.font = RuntimeFonts.Japanese; finish.fontSize = 100; finish.alignment = TextAnchor.MiddleCenter;
            finish.color = settings.Warning; finish.raycastTarget = false; finish.enabled = false;
            var outline = label.AddComponent<Outline>(); outline.effectColor = Color.black; outline.effectDistance = new Vector2(3, -3);
            PaperSign.Attach(finish,"Paper Finish Banner");
        }
        private void OnEnable() { if (runner) { runner.EventProduced += OnEvent; runner.MatchRestarted += ResetView; } }
        private void OnDisable() { if (runner) { runner.EventProduced -= OnEvent; runner.MatchRestarted -= ResetView; } }
        public void ResetView()
        { Feedback.Reset(); if (sparks != null) foreach (var spark in sparks) spark.enabled = false; if (flash) flash.enabled = false; if (finish) finish.enabled = false; }
        public void ToggleShake() { ScreenShakeEnabled = !ScreenShakeEnabled; PlayerPrefs.SetInt("AniBato.ScreenShake", ScreenShakeEnabled ? 1 : 0); }
        private void OnEvent(BattleEvent value)
        {
            Feedback.Apply(value, settings.ShakeSeconds, settings.JustFlashSeconds, settings.FinishSeconds);
            if (value.TargetPlayer < 0 || value.Kind != BattleEventKind.Hit && value.Kind != BattleEventKind.Guarded &&
                value.Kind != BattleEventKind.JustGuard && value.Kind != BattleEventKind.JustDodge && value.Kind != BattleEventKind.CounterSuccess && value.Kind != BattleEventKind.Clash) return;
            int player = value.Kind == BattleEventKind.Hit ? value.TargetPlayer : value.SourcePlayer;
            var fighter = player == 0 ? runner.Simulation.P1 : runner.Simulation.P2;
            int slot = nextSpark++ % sparks.Length; ages[slot] = 0;
            sparkPlayers[slot] = player; sparkOffsets[slot] = new Vector3(fighter.Facing * .4f, .35f, 0);
            justSparks[slot] = value.Kind == BattleEventKind.JustGuard || value.Kind == BattleEventKind.JustDodge || value.Kind == BattleEventKind.Clash;
            var spark = sparks[slot]; spark.enabled = true;
            spark.sprite = ProceduralSprites.Shape(value.Kind != BattleEventKind.Hit);
            spark.transform.position = new Vector3(fighter.Position.X + fighter.Facing * .4f, fighter.Position.Y + .35f, 0);
            spark.transform.rotation = Quaternion.Euler(0, 0, 45);
            spark.color = value.Kind == BattleEventKind.Hit ? settings.Warning : justSparks[slot] ? Color.white : settings.GuardTint;
        }
        private void LateUpdate()
        {
            if (runner.Paused) return;
            Feedback.Advance(Time.unscaledDeltaTime);
            flash.enabled = Feedback.FlashRemaining > 0;
            flash.color = new Color(.6f, .85f, 1, .18f * Mathf.Clamp01(Feedback.FlashRemaining / settings.JustFlashSeconds));
            finish.enabled = Feedback.FinishRemaining > 0;
            finish.text = Feedback.FinishText;
            finish.rectTransform.localScale = Vector3.one * (1 + .12f * Mathf.Clamp01(Feedback.FinishRemaining / settings.FinishSeconds));
            for (int i = 0; i < sparks.Length; i++)
            {
                if (!sparks[i].enabled) continue;
                var position = runner.RenderPosition(sparkPlayers[i]);
                sparks[i].transform.position = new Vector3(position.X, position.Y, 0) + sparkOffsets[i];
                ages[i] += Time.unscaledDeltaTime * (justSparks[i] ? .5f : 1);
                float p = ages[i] / settings.SparkSeconds;
                if (p >= 1) { sparks[i].enabled = false; continue; }
                sparks[i].transform.localScale = new Vector3(.8f + p * 1.2f, .12f + p * .18f, 1);
                var color = sparks[i].color; color.a = 1 - p; sparks[i].color = color;
            }
        }
    }
}
