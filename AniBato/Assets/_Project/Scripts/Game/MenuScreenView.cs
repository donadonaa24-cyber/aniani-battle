using AniBato.Battle;
using UnityEngine;
using UnityEngine.UI;
using AniBato.Presentation;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Game
{
    public sealed class MenuScreenView : MonoBehaviour
    {
        [SerializeField] private Image shade;
        [SerializeField] private Text heading, subtitle, options, footer, banner;
        [SerializeField] private Image[] cards;
        [SerializeField] private Text[] cardLabels;
        [SerializeField] private Text[] controlColumns;
        private readonly Color accent = new Color(0.4f, 0.88f, 1);
        private PaperScreenDecor paperDecor;
        private PaperRosterGrid rosterGrid;
        private void LateUpdate()
        {
            var canvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            transform.localScale = Vector3.one * Mathf.Min(canvas.rect.width / 1920f, canvas.rect.height / 1080f);
        }
        public void Configure(Image backdrop, Text title, Text detail, Text choices, Text help, Text overlay,
            Image[] pictures, Text[] labels, Text[] columns)
        { shade = backdrop; heading = title; subtitle = detail; options = choices; footer = help; banner = overlay; cards = pictures; cardLabels = labels; controlColumns = columns; }
        private void Start()
        {
            paperDecor = GetComponentInChildren<PaperScreenDecor>();
            rosterGrid = GetComponentInChildren<PaperRosterGrid>(true);
            foreach (var text in GetComponentsInChildren<Text>(true)) text.font = RuntimeFonts.Japanese;
        }
        public void Render(GameFlowController flow)
        {
            var screen = flow.Screen;
            subtitle.rectTransform.anchoredPosition = new Vector2(0, screen == GameScreen.CharacterSelect ? 355 : 342);
            subtitle.rectTransform.sizeDelta = new Vector2(1740, screen == GameScreen.CharacterSelect ? 40 : 70);
            subtitle.fontSize = screen == GameScreen.CharacterSelect ? 24 : 28;
            if (paperDecor) paperDecor.Render(flow);
            if (rosterGrid) rosterGrid.Render(flow);
            foreach (var column in controlColumns) column.enabled = screen == GameScreen.Controls;
            bool selections = screen == GameScreen.CharacterSelect || screen == GameScreen.StageSelect || screen == GameScreen.Result;
            options.rectTransform.anchoredPosition = new Vector2(0, selections ? -275 : 0);
            options.rectTransform.sizeDelta = new Vector2(1740, screen == GameScreen.Controls ? 740 : selections ? 210 : 420);
            for (int i = 0; i < cards.Length; i++)
            {
                float x = screen == GameScreen.StageSelect ? (i - 1) * 580 : i == 0 ? -450 : 450;
                cards[i].rectTransform.anchoredPosition = new Vector2(x, 120);
                cards[i].rectTransform.sizeDelta = new Vector2(500, screen == GameScreen.StageSelect ? 282 : 360);
                cards[i].rectTransform.localRotation = Quaternion.identity;
                cards[i].rectTransform.localScale = Vector3.one;
                cardLabels[i].rectTransform.anchoredPosition = new Vector2(x, -100);
            }
            bool battle = screen == GameScreen.Battle || screen == GameScreen.Countdown || screen == GameScreen.LayoutEditor;
            shade.enabled = !battle;
            shade.color = new Color(0.09f, 0.12f, 0.18f, screen == GameScreen.Pause || screen == GameScreen.Controls ? 0.94f : 1);
            heading.enabled = subtitle.enabled = options.enabled = footer.enabled = !battle;
            banner.enabled = battle && screen != GameScreen.LayoutEditor;
            banner.text = screen == GameScreen.Countdown ? flow.CountdownText : "";
            if (screen == GameScreen.Battle && !flow.Runner.Simulation.Match.IsFinished && flow.Runner.Simulation.Shrink.WarningIssued)
            {
                var shrink = flow.Runner.Simulation.Shrink;
                banner.text = flow.Runner.CurrentStage.ShrinkMessage + (shrink.Started ? "  中央へ！" : "  残り10秒で床が縮小");
            }
            if (screen == GameScreen.Battle && banner.text == "" && flow.Series != null)
                banner.text = "ROUND " + flow.Series.RoundNumber + "　P1 " + flow.Series.Score(0) + " − " + flow.Series.Score(1) + (flow.CpuMode ? " CPU" : " P2");
            banner.fontSize = screen == GameScreen.Countdown ? 110 : 28;
            banner.rectTransform.anchoredPosition = screen == GameScreen.Countdown ? Vector2.zero : new Vector2(0, 285);
            for (int i = 0; i < cards.Length; i++) { cards[i].enabled = cardLabels[i].enabled = false; }
            heading.text = ""; subtitle.text = ""; options.text = "";
            var input = flow.Input;
            footer.text = "方向入力 選択    " + input.ConfirmLabel(0) + " 決定    " + input.CancelLabel(0) + " / Esc 戻る    F11 全画面";
            if (screen == GameScreen.Title)
            {
                heading.text = "あにあにバトル！！"; subtitle.text = "70秒、3つのハート。作品を超えて、勝負！";
                options.rectTransform.anchoredPosition = new Vector2(0,-355);
                options.text = "ボタンを押して開演！"; footer.text = "あにバト！！    /    aniani";
            }
            else if (screen == GameScreen.MainMenu)
            {
                heading.text = "あにあにバトル！！"; subtitle.text = "モードを選択";
                options.rectTransform.anchoredPosition=new Vector2(0,-392);options.rectTransform.sizeDelta=new Vector2(1740,80);
                options.text=(WebInputPolicy.Enabled ? Choices(flow.MenuIndex,"CPUチーム対戦","2人チーム対戦","練習モード","設定","終了","オンライン") : Choices(flow.MenuIndex,"CPUチーム対戦","2人チーム対戦","練習モード","設定","終了")).Replace("\n","    ");
                footer.text="左右で選択　"+input.ConfirmLabel(0)+" 決定　／　Esc 戻る";
            }
            else if (screen == GameScreen.CharacterSelect) Characters(flow);
            else if (screen == GameScreen.StageSelect) Stages(flow);
            else if (screen == GameScreen.Pause)
            {
                heading.text = "PAUSE"; subtitle.text = "再開するまで試合は進みません";
                options.rectTransform.sizeDelta = new Vector2(1740, 620);
                options.text = Choices(flow.MenuIndex, "再開", "設定", "技表", flow.PracticeMode ? "練習設定" : "練習設定（練習モードで利用）", "チーム編成へ", "タイトルへ");
            }
            else if (screen == GameScreen.Settings)
            {
                heading.text = "設定"; subtitle.text = "左右または決定で変更。CPU難易度は次の試合から反映";
                options.rectTransform.sizeDelta = new Vector2(1740, 740);
                options.text = Choices(flow.MenuIndex,
                    "BGM音量  " + Mathf.RoundToInt((flow.Audio ? flow.Audio.BgmVolume : 0) * 100) + "%",
                    "SE音量  " + Mathf.RoundToInt((flow.Audio ? flow.Audio.SeVolume : 0) * 100) + "%",
                    "フレームレート  " + FramePacing.Label, "画面揺れ  " + (flow.ScreenShakeEnabled ? "ON" : "OFF"),
                    "開始ヒント  " + (flow.BasicHintsEnabled ? "ON" : "OFF"), LearningText.DashSetting(input),
                    "攻撃判定表示  " + (flow.Runner.DebugBoxes ? "ON" : "OFF"), "CPU難易度  " + Difficulty(flow.Difficulty),
                    "キーコンフィグ", "操作説明", "チュートリアル", "残像  " + (flow.Runner.MotionTrailsEnabled ? "ON" : "OFF（元の表示）"), "操作・HUDの配置",
                    "タッチ移動（戦闘中）  " + TouchMovementSettings.Label(flow.TouchControls.MovementSettings.Combat),
                    "タッチ移動（戦闘以外）  " + TouchMovementSettings.Label(flow.TouchControls.MovementSettings.Menu), "戻る");
            }
            else if (screen == GameScreen.Result) Result(flow);
            else if (screen == GameScreen.Controls)
            {
                heading.text = "操作説明"; subtitle.text = "P1 / P2 キーボード ＆ ゲームパッド";
                options.rectTransform.anchoredPosition = new Vector2(0, -275); options.rectTransform.sizeDelta = new Vector2(1740, 250);
                controlColumns[0].text = "操作\n移動\nジャンプ\n通常攻撃\n特殊攻撃\nガード\n回避\nカウンター\n超必殺\nダッシュ";
                controlColumns[1].text = KeyboardColumn("P1", input.EffectiveKeys(0));
                controlColumns[2].text = KeyboardColumn("P2", input.EffectiveKeys(1));
                controlColumns[3].text = "現在のP1操作\n左右 / 上下";
                for (int action = 4; action <= 11; action++) controlColumns[3].text += "\n" + input.Label(0, (ControlAction)action);
                options.fontSize = 25; options.text = RulesText;
                footer.text = LearningText.DashHelp(input) + " 下でしゃがみ、下18F長押しで上足場から降下。空中の向き固定。\n" + input.Label(0, ControlAction.Pause) + " ポーズ / " + input.ConfirmLabel(0) + " または " + input.CancelLabel(0) + " で戻る";
            }
            else if (screen == GameScreen.CommandList)
            { heading.text = "技表  " + flow.Setup.Character(flow.CommandCharacter).Movement.Name; subtitle.text = "左右：キャラ / 上下：ページ " + (flow.CommandPage + 1) + "/2"; options.rectTransform.sizeDelta = new Vector2(1740, 680); options.text = LearningText.Commands(flow.Setup.Character(flow.CommandCharacter), input, flow.CommandPage); }
            else if (screen == GameScreen.KeyConfig)
            {
                heading.text = "キーコンフィグ"; subtitle.text = flow.CapturingBinding ? "新しいボタンを押してください。Escで取消。" : "決定で変更 / 重複は同じ設定内で入替 / P1・P2は別保存";
                var labels = new string[17]; labels[0] = "プレイヤー  P" + (flow.ConfigPlayer + 1); labels[1] = "機器  " + (flow.ConfigGamepad ? "パッド" : "キーボード");
                var profile = input.Configuration.Profile(flow.ConfigPlayer, flow.ConfigGamepad);
                var pad = input.AssignedGamepad(flow.ConfigPlayer);
                var device = pad == null ? InputLabelDevice.Xbox : InputGlyphs.Detect(pad.layout, pad.description.product, pad.description.manufacturer);
                for (int i = 0; i < 13; i++) labels[i + 2] = LearningText.Actions[i] + "  " + (flow.ConfigGamepad ? InputGlyphs.Pad(profile[(ControlAction)i], device) : ((UnityEngine.InputSystem.Key)profile[(ControlAction)i]).ToString());
                labels[15] = "この割り当てを初期化"; labels[16] = LearningText.DashSetting(input) + "（両者共通）"; options.rectTransform.sizeDelta = new Vector2(1740, 760); options.text = Choices(flow.MenuIndex, labels);
            }
            else if (screen == GameScreen.PracticeSettings)
            { heading.text = "練習設定"; subtitle.text = "練習中：F5 速度／F6 静止／F7 1コマ進める／F9 判定表示"; options.text = Choices(flow.MenuIndex, "相手  " + new[] { "棒立ち", "ガード", "ジャンプ", "ふつうCPU" }[(int)flow.Practice.Opponent], "ハート固定  " + (flow.Practice.FreezeHearts ? "ON" : "OFF"), "時間固定  " + (flow.Practice.FreezeTimer ? "ON" : "OFF"),"練習で判定表示  "+(flow.Practice.DebugDefault?"ON":"OFF"),"速度  "+flow.Runner.PracticeRateLabel,"静止  "+(flow.Runner.PracticePlaybackPaused?"ON":"OFF"),"1コマ進める（静止中）"); }
            if (screen != GameScreen.Controls) options.fontSize = screen == GameScreen.KeyConfig ? 26 : screen == GameScreen.CommandList || screen == GameScreen.Pause || screen == GameScreen.Settings ? 30 : 38;
            if(screen==GameScreen.MainMenu)options.fontSize=WebInputPolicy.Enabled ? 24 : 28;
            if(flow.OnlineMode)
            {
                if(screen==GameScreen.Battle && flow.Online.Waiting) banner.text="通信待ち…　相手の入力が届くまで停止中";
                if(screen==GameScreen.Result) { options.text="両者の決定で次へ　"+flow.Online.Notice; footer.text="退出はブラウザ上の「対戦を終了」"; }
            }
            if(WebInputPolicy.Enabled)
            {
                heading.text=WebInputPolicy.Text(heading.text);subtitle.text=WebInputPolicy.Text(subtitle.text);
                options.text=WebInputPolicy.Text(options.text);footer.text=WebInputPolicy.Text(footer.text);
                if(screen==GameScreen.MainMenu)options.text=options.text.Replace("終了","タイトルへ");
            }
        }
        private void Characters(GameFlowController flow)
        {
            heading.text = "キャラクター選択";
            if(rosterGrid)
            {
                subtitle.text = "3作品・全9人　" + (flow.SelectionPlayer == 0 ? "P1" : flow.CpuMode ? "CPU" : "P2") + "の" + (flow.Selection.Picks(flow.SelectionPlayer).Count + 1) + "/" + flow.Selection.Required + "人目　同チーム内の重複不可";
                options.text=""; footer.text="左右：同じ段　上下：段を切替　" + flow.Input.ConfirmLabel(flow.SelectionPlayer == 0 || flow.CpuMode ? 0 : 1) + " 追加　／　取消で1人戻す　／　Esc 戻る　／　" + flow.Input.CommandListLabel + " 技表";
                return;
            }
            subtitle.text = flow.CpuMode ? "CPU対戦：相手と難易度を選択" : "それぞれのキャラを選び、両者決定でステージへ";
            var controller = flow.Runner.GetComponent<InputController>();
            for (int i = 0; i < 2; i++)
            {
                var character = flow.Setup.Character(i == 0 ? flow.Player1Index : flow.Player2Index);
                cards[i].enabled = cardLabels[i].enabled = true;
                cards[i].sprite = character.PaperMotion && character.PaperMotion.Enabled ? character.PaperMotion.Sprite : character.Animations.Find("idle").Frames[0];
                cards[i].color = Color.white;
                cardLabels[i].text = (i == 0 ? "P1" : flow.CpuMode ? "CPU" : "P2") + "  " + character.Movement.Name +
                    (flow.Ready(i) ? "  READY!" : "  ◀  ▶") + "\n" + Work(character) + "\n" + Type(character);
                cardLabels[i].color = flow.CpuMode && flow.CpuRow == i || flow.Ready(i) ? accent : Color.white;
            }
            options.text = flow.CpuMode ? (flow.CpuRow == 2 ? "▶ " : "") + "難易度  ◀ " + Difficulty(flow.Difficulty) + " ▶" : "";
            footer.text = "方向入力で選択 / P1 " + controller.ConfirmLabel(0) + " 決定・" + controller.CancelLabel(0) + " 戻る / P2 " + controller.ConfirmLabel(1) + " 決定・" + controller.CancelLabel(1) + " 戻る\n" + controller.CommandListLabel + " 技表 / Esc メニューへ";
            subtitle.text += "\n接続順: P1 " + controller.GamepadName(0) + " / P2 " + controller.GamepadName(1);
        }
        private void Stages(GameFlowController flow)
        {
            heading.text = "ステージ選択"; subtitle.text = "3つの世界、共通の地形。最後の10秒で中央へ。";
            for (int i = 0; i < 3; i++)
            {
                cards[i].enabled = cardLabels[i].enabled = true; cards[i].sprite = flow.Setup.Stages[i].Thumbnail;
                cards[i].color = flow.StageIndex == i ? Color.white : new Color(0.8f, 0.82f, 0.86f);
                cardLabels[i].text = (flow.StageIndex == i ? "▶ " : "") + flow.Setup.Stages[i].DisplayName;
                cardLabels[i].color = flow.StageIndex == i ? accent : Color.white;
            }
            options.text = flow.StageIndex == 3 ? "▶ ランダム" : "ランダム";
            var input = flow.Input;
            footer.text = input.Label(0, ControlAction.Left) + " / " + input.Label(0, ControlAction.Right) +
                "（パッド：左右）選択    " + input.ConfirmLabel(0) + " 決定    " + input.CancelLabel(0) + " / Esc 戻る\n上下でも選択可 / F11 全画面";
        }
        private void Result(GameFlowController flow)
        {
            var sim = flow.Runner.Simulation;
            heading.text = sim.Match.Result == MatchResult.Draw ? "DRAW" :
                (sim.Match.Result == MatchResult.Player1Wins ? sim.P1.Definition.Name : sim.P2.Definition.Name) + " WIN!";
            bool draw = sim.Match.Result == MatchResult.Draw;
            subtitle.text = (draw ? "引き分け  /  " : "") + Reason(sim.Match.EndReason) + "  /  " + flow.Runner.CurrentStage.DisplayName;
            if (flow.Series != null)
            {
                subtitle.text += "\nROUND " + flow.Series.RoundNumber + "　チーム得点 P1 " + flow.Series.Score(0) + " − " + flow.Series.Score(1) + (flow.CpuMode ? " CPU" : " P2") + "　2勝先取";
                if (flow.Series.Complete) heading.text = (flow.Series.Winner == 0 ? "P1" : flow.CpuMode ? "CPU" : "P2") + " チーム WIN!";
            }
            for (int i = 0; i < 2; i++)
            {
                int player = !draw && sim.Match.Result == MatchResult.Player2Wins ? 1 - i : i;
                var character = flow.Runner.Character(player); var fighter = player == 0 ? sim.P1 : sim.P2;
                cards[i].enabled = cardLabels[i].enabled = true;
                var animation = flow.Runner.Animation(player);
                animation.State = !draw && i == 0 ? AnimationState.Win : AnimationState.Lose;
                animation.IsAttack = false;
                character.Animations.TrySprite(animation, out Sprite sprite);
                if (character.PaperMotion && character.PaperMotion.TrySprite(animation, fighter.LastDamageLostHeart, out Sprite paper, out PaperPose pose)) sprite = paper;
                cards[i].sprite = sprite; cards[i].color = Color.white;
                cards[i].rectTransform.anchoredPosition = new Vector2(i == 0 ? -450 : 450, 80);
                cards[i].rectTransform.sizeDelta = new Vector2(560, !draw && i == 0 ? 450 : 390);
                if (!draw && i == 0)
                {
                    var motion = PaperPerformance.Evaluate(character.PaperMotion ? character.PaperMotion.Performance : PaperPerformanceSettings.Default, animation);
                    cards[i].rectTransform.localRotation = Quaternion.Euler(0, 0, motion.Degrees);
                    cards[i].rectTransform.anchoredPosition += new Vector2(0, motion.Y * 150);
                }
                cardLabels[i].rectTransform.anchoredPosition = new Vector2(i == 0 ? -450 : 450, -190);
                cardLabels[i].text = (draw ? "DRAW" : i == 0 ? "WINNER!" : "LOSE...") + "  P" + (player + 1) + "  " + fighter.Definition.Name + "\nハート " + fighter.Hearts + "  /  被弾 " + fighter.NormalHitCount;
                cardLabels[i].color = !draw && i == 0 ? accent : Color.white;
            }
            options.text = Choices(flow.MenuIndex, flow.HasNextRound ? (draw ? "同じラウンドを再試合" : "次のラウンドへ") : "再戦", "チーム編成へ", "タイトルへ");
        }
        private string Choices(int selected, params string[] labels)
        {
            string value = "";
            for (int i = 0; i < labels.Length; i++) value += (i == selected ? "▶ " : "   ") + labels[i] + (i + 1 < labels.Length ? "\n" : "");
            return value;
        }
        private static string Work(CharacterData data) => data.Silhouette == 0 ? "星の終わり" : data.Silhouette == 1 ? "推し駒Battle" : "Battle à la carte";
        private static string Type(CharacterData data) => data.Silhouette == 0 ? "機動力・弾・解析で攻める" : data.Silhouette == 1 ? "構えとカウンターで切り返す" : "料理と回復で粘る";
        private static string Difficulty(CpuDifficulty value) => value == CpuDifficulty.Easy ? "やさしい" : value == CpuDifficulty.Normal ? "ふつう" : "むずかしい";
        private static string Reason(MatchEndReason value) => value == MatchEndReason.RingOut ? "場外" : value == MatchEndReason.Hearts ? "KO" : "時間切れ判定";
        private static string KeyboardColumn(string player, PlayerKeyBindings keys)
        {
            return player + "\n" + KeyName(keys.Left) + "/" + KeyName(keys.Right) + "・" + KeyName(keys.Up) + "/" + KeyName(keys.Down) +
                "\n" + KeyName(keys.Jump) + "\n" + KeyName(keys.Attack) + "\n" + KeyName(keys.Special) + "\n" + KeyName(keys.Guard) +
                "\n" + KeyName(keys.Dodge) + "\n" + KeyName(keys.Counter) + "\n" + KeyName(keys.Ultimate) + "\n" + KeyName(keys.Dash);
        }
        private static string KeyName(UnityEngine.InputSystem.Key key) => key.ToString().Replace("LeftArrow", "←").Replace("RightArrow", "→")
            .Replace("UpArrow", "↑").Replace("DownArrow", "↓").Replace("Right", "右").Replace("Left", "左");
        private const string RulesText = "70秒 / ハート3。通常攻撃の被弾3回でハート−1、コンボが途切れても被弾は残る。\n" +
                "全攻撃をガード可能。保持＋防御でスタミナ消費。しゃがみ攻撃はガード崩し＋蓄積1。\n" +
                "LINK100で超必殺：命中でハート−2。スタミナ不足のガードを貫通。ジャストガード消費0。\n" +
                "場外ラインを超えたら敗北。残り15秒で予告、10秒で縮小開始。\n" +
                "時間切れはハートが多い側 → 被弾が少ない側 → 同数なら引き分け。\n" +
                "下でしゃがみ、上足場は下長押しで降下。空中の前後はキャラの向き基準。";
    }
}
