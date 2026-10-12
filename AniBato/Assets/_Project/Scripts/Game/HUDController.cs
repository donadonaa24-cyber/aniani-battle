using AniBato.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    [DefaultExecutionOrder(100)]
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private Camera battleCamera;
        [SerializeField] private PresentationSettings presentation;
        [SerializeField] private Text p1Hud;
        [SerializeField] private Text p2Hud;
        [SerializeField] private Text timer;
        [SerializeField] private Text result;
        [SerializeField] private Text[] counters;
        private readonly float[] bounceUntil = new float[2];
        private readonly float[] breakUntil = new float[2];
        private RectTransform canvasRect;
        private FighterView[] fighterViews;
        private void Start()
        {
            fighterViews = Object.FindObjectsByType<FighterView>();
            foreach (var text in GetComponentsInChildren<Text>(true)) text.font = RuntimeFonts.Japanese;
        }
        private Text[] popups;
        private readonly float[] popupUntil = new float[2];
        private readonly string[] popupText = new string[2];
        public void ConfigurePopups(Text[] value) => popups = value;
        public void Configure(BattleRunner value, Camera cameraValue, PresentationSettings settings,
            Text left, Text right, Text time, Text matchResult, Text[] headCounters)
        { runner = value; battleCamera = cameraValue; presentation = settings; p1Hud = left; p2Hud = right; timer = time; result = matchResult; counters = headCounters; }
        private void Awake() => canvasRect = GetComponent<RectTransform>();
        private void OnEnable() { if (runner) { runner.EventProduced += OnBattleEvent; runner.MatchRestarted += OnRestart; } }
        private void OnDisable() { if (runner) { runner.EventProduced -= OnBattleEvent; runner.MatchRestarted -= OnRestart; } }
        private void OnRestart()
        { for (int i = 0; i < 2; i++) { bounceUntil[i] = 0; breakUntil[i] = 0; popupUntil[i] = 0; } }
        private void OnBattleEvent(BattleEvent battleEvent)
        {
            int target = battleEvent.TargetPlayer;
            if (target < 0) return;
            if (battleEvent.Kind == BattleEventKind.Hit) bounceUntil[target] = Time.unscaledTime + presentation.CounterBounceSeconds;
            if (battleEvent.Kind == BattleEventKind.Break) breakUntil[target] = Time.unscaledTime + presentation.BreakSeconds;
            string label = battleEvent.Kind == BattleEventKind.CounterSuccess ? "COUNTER!" :
                battleEvent.Kind == BattleEventKind.JustGuard ? "JUST GUARD!" : battleEvent.Kind == BattleEventKind.JustDodge ? "JUST DODGE!" :
                battleEvent.Kind == BattleEventKind.ComboFinish ? "COMBO FINISH!" : battleEvent.Kind == BattleEventKind.Break ? "BREAK!" : null;
            if (label != null)
            {
                int player = battleEvent.Kind == BattleEventKind.ComboFinish || battleEvent.Kind == BattleEventKind.Break ? target : battleEvent.SourcePlayer;
                popupText[player] = Time.unscaledTime < popupUntil[player] ? popupText[player] + "\n" + label : label;
                popupUntil[player] = Time.unscaledTime + presentation.PopupSeconds;
            }
        }
        private void LateUpdate()
        {
            var simulation = runner.Simulation;
            if (simulation == null) return;
            UpdatePlayer(simulation.P1, p1Hud, 0, simulation.Rules.NormalHitsPerHeart);
            UpdatePlayer(simulation.P2, p2Hud, 1, simulation.Rules.NormalHitsPerHeart);
            timer.text = Mathf.CeilToInt((float)simulation.Match.RemainingSeconds).ToString("00");
            timer.color = simulation.Match.RemainingSeconds <= 10 ? presentation.Warning : Color.white;
            result.enabled = simulation.Match.IsFinished && !runner.ManagedByFlow;
            if (result.enabled)
            {
                string winner = simulation.Match.Result == MatchResult.Draw ? "DRAW" :
                    simulation.Match.Result == MatchResult.Player1Wins ? simulation.P1.Definition.Name + " WIN!" : simulation.P2.Definition.Name + " WIN!";
                result.text = winner + "\n次戦 P1: " + runner.Character(0).Movement.Name + " / P2: " + runner.Character(1).Movement.Name +
                    "\nF1/F2: P1変更 · F3/F4: P2変更 · R 再戦";
            }
        }
        private void UpdatePlayer(FighterState fighter, Text hud, int index, int threshold)
        {
            hud.text = fighter.Definition.Name + "\n" + new string('♥', fighter.Hearts) + new string('♡', runner.Simulation.Rules.InitialHearts - fighter.Hearts) +
                "   " + fighter.NormalHitCount + "/" + threshold + "\nLINK " + fighter.Gauge + "/" + runner.Simulation.Rules.Gauge.Maximum +
                (GaugeSystem.CanUseUltimate(fighter, runner.Simulation.Rules) ? "  超必殺 READY" : fighter.UltimateUses > 0 ? "  超必殺 使用済" : "");
            if (fighter.BuffRemaining > 0) hud.text += "  機動強化";
            if (fighter.MarkRemaining > 0) hud.text += "  解析マーク";
            if (fighter.ParalysisRemaining > 0) hud.text += "  もぐもぐ";
            if (fighter.Attack != null && fighter.Attack.Definition.NonDamaging && fighter.Attack.Definition.SelfHealNormalHits > 0)
                hud.text += "  食事中";
            if (fighter.Attack != null && fighter.Attack.Definition.RequiresFollowup && !fighter.Attack.IsActive)
                hud.text += "  構え: 通常/特殊で急襲";
            var counter = counters[index];
            bool breaking = Time.unscaledTime < breakUntil[index];
            counter.text = breaking ? "BREAK!" : fighter.NormalHitCount + "/" + threshold;
            counter.color = breaking || fighter.NormalHitCount >= threshold - 1 ? presentation.Warning : Color.white;
            var view = fighterViews == null ? null : System.Array.Find(fighterViews, candidate => candidate.PlayerIndex == index);
            var position = runner.RenderPosition(index);
            float counterY = view && view.UsesAnimationSprites ? view.HeadY + presentation.CounterHeadPadding : position.Y + presentation.CounterHeight;
            Vector3 screen = battleCamera.WorldToScreenPoint(new Vector3(position.X, counterY, 0));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
            counter.rectTransform.anchoredPosition = local;
            float bounce = Mathf.Clamp01((bounceUntil[index] - Time.unscaledTime) / presentation.CounterBounceSeconds);
            counter.rectTransform.localScale = Vector3.one * (1 + Mathf.Sin(bounce * Mathf.PI) * 0.18f);
            counter.enabled = screen.z > 0;
            if (popups == null) return;
            var popup = popups[index]; popup.enabled = Time.unscaledTime < popupUntil[index] && screen.z > 0;
            if (popup.enabled)
            {
                popup.text = popupText[index]; popup.color = presentation.Warning;
                Vector3 popupScreen = battleCamera.WorldToScreenPoint(new Vector3(position.X, position.Y + presentation.PopupHeight, 0));
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, popupScreen, null, out Vector2 popupLocal);
                popup.rectTransform.anchoredPosition = popupLocal;
            }
        }
    }
}


