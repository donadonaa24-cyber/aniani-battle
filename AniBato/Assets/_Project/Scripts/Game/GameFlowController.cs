using AniBato.Battle;
using AniBato.Presentation;
using UnityEngine;

namespace AniBato.Game
{
    public enum GameScreen { Title, MainMenu, CharacterSelect, StageSelect, Countdown, Battle, Pause, Result, Controls, CommandList, KeyConfig, PracticeSettings, Settings }
    public enum CpuDifficulty { Easy, Normal, Hard }

    [DefaultExecutionOrder(-100)]
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private InputController input;
        [SerializeField] private BattleSetup setup;
        [SerializeField] private GameFlowSettings settings;
        [SerializeField] private MenuScreenView view;
        [SerializeField] private GameObject world, hud;
        [SerializeField] private StageView stageView;
        private GameScreen controlsReturn;
        private GameScreen panelReturn;
        public InputController Input => input;
        public bool PracticeMode { get; private set; }
        public bool TutorialMode { get; private set; }
        public PracticeOptions Practice { get; } = new PracticeOptions();
        public int CommandCharacter { get; private set; }
        public int CommandPage { get; private set; }
        public int ConfigPlayer { get; private set; }
        public bool ConfigGamepad { get; private set; }
        public bool CapturingBinding { get; private set; }
        public bool BasicHintsEnabled { get; private set; } = true;
        private float countdownElapsed, resultElapsed;
        private GameScreen settingsReturn;
        private int settingsMenuIndex;
        private bool resultRecorded;
        public TeamSelection Selection { get; private set; } = new TeamSelection(9);
        public TeamSeries Series { get; private set; }
        public string SelectionStatus { get; private set; } = "";
        public int SelectionPlayer => Selection.ActivePlayer;
        public bool HasNextRound => Series != null && !Series.Complete && Series.AwaitingAdvance;
        private AudioManager audioManager;
        private BattleEffectsView effects;
        private string previousCountdown;
        public AudioManager Audio => audioManager;
        public bool ScreenShakeEnabled => effects && effects.ScreenShakeEnabled;
        public GameScreen Screen { get; private set; } = GameScreen.Title;
        public int MenuIndex { get; private set; }
        private int mainMenuIndex; // cursor is kept when returning to the main menu (e.g. cancel from character select)
        public int Player1Index { get; private set; }
        public int Player2Index { get; private set; } = 1;
        public int StageIndex { get; private set; }
        public bool CpuMode { get; private set; }
        public CpuDifficulty Difficulty { get; private set; } = CpuDifficulty.Normal;
        public BattleSetup Setup => setup;
        public BattleRunner Runner => runner;
        public int CpuRow => SelectionPlayer;
        public bool Ready(int player) => Selection.Picks(player).Count == Selection.Required;
        public string CountdownText => countdownElapsed < settings.CountdownNumberSeconds * 3 ?
            (3 - Mathf.FloorToInt(countdownElapsed / settings.CountdownNumberSeconds)).ToString() : "FIGHT!";
        public void Configure(BattleRunner battle, InputController controller, BattleSetup roster, GameFlowSettings timing,
            MenuScreenView menuView, GameObject battleWorld, GameObject battleHud, StageView stageVisual)
        {
            runner = battle; input = controller; setup = roster; settings = timing; view = menuView;
            world = battleWorld; hud = battleHud; stageView = stageVisual; runner.SetManagedByFlow();
        }
        private void Awake() { if (runner) runner.SetManagedByFlow(); }
        private void Start()
        {
            FramePacing.Load();
            BasicHintsEnabled = PlayerPrefs.GetInt("AniBato.BasicHints", 1) != 0;
            audioManager = runner.GetComponent<AudioManager>(); effects = runner.GetComponent<BattleEffectsView>();
            Player1Index = setup.Player1Index; Player2Index = setup.Player2Index;
            Selection = new TeamSelection(setup.Characters.Length);
            Difficulty = (CpuDifficulty)Mathf.Clamp(PlayerPrefs.GetInt("AniBato.CpuDifficulty", 1), 0, 2);
            Show(GameScreen.Title, false);
        }
        private void Update()
        {
            input.RefreshMenuReleaseGuard();
            var first = input.ReadMenu(0); var second = input.ReadMenu(1);
            bool confirm = first.Confirm || second.Confirm;
            bool cancel = input.EscapePressed || first.Cancel || second.Cancel || Screen == GameScreen.Pause && (first.Start || second.Start);
            int vertical = first.Vertical != 0 ? first.Vertical : second.Vertical;
            int horizontal = first.Horizontal != 0 ? first.Horizontal : second.Horizontal;
            if (Screen == GameScreen.KeyConfig && CapturingBinding)
            {
                if (input.EscapePressed) { CapturingBinding = false; input.GuardScreenTransition(MenuAction.Cancel); }
                else if (input.CaptureBinding(ConfigPlayer, ConfigGamepad, (AniBato.Presentation.ControlAction)(MenuIndex - 2))) CapturingBinding = false;
                view.Render(this); return;
            }
            if (audioManager)
            {
                if (cancel) audioManager.Play(SoundCue.Cancel);
                else if (confirm || first.Start || second.Start) audioManager.Play(SoundCue.Confirm);
                else if (vertical != 0 || horizontal != 0) audioManager.Play(SoundCue.Move);
            }
            if (Screen == GameScreen.Title)
            { if (input.AnyMenuKeyPressed || confirm || first.Start || second.Start) Show(GameScreen.MainMenu, true, MenuAction.AnyKey | MenuAction.Confirm | MenuAction.Start); }
            else if (Screen == GameScreen.Countdown)
            {
                if (first.Start || second.Start) { Show(GameScreen.Pause, true, MenuAction.Start); return; }
                countdownElapsed += Time.unscaledDeltaTime;
                if (CountdownText != previousCountdown) { previousCountdown = CountdownText; if (audioManager) audioManager.Play(CountdownText == "FIGHT!" ? SoundCue.Fight : SoundCue.Countdown); }
                if (countdownElapsed >= settings.CountdownNumberSeconds * 3 + settings.FightBannerSeconds)
                { Show(GameScreen.Battle, false); runner.SetPaused(false); }
            }
            else if (Screen == GameScreen.Battle)
            {
                if (runner.Simulation.Match.IsFinished)
                {
                    if (!resultRecorded) { Series?.Record(runner.Simulation.Match.Result); resultRecorded = true; }
                    resultElapsed += Time.unscaledDeltaTime; if (resultElapsed >= settings.ResultDelaySeconds) Show(GameScreen.Result, false);
                }
                else if (first.Start || second.Start) Show(GameScreen.Pause, true, MenuAction.Start);
            }
            else if (Screen == GameScreen.CharacterSelect)
            { if (input.CommandListPressed) OpenCommands(); else CharacterInput(first, second); }
            else if (Screen == GameScreen.StageSelect)
            {
                StageIndex = Wrap(StageIndex + horizontal + vertical, setup.Stages.Length + 1);
                if (cancel) { Selection.Undo(); Show(GameScreen.CharacterSelect, true, MenuAction.Cancel); }
                else if (confirm) StartMatch();
            }
            else if (Screen == GameScreen.Controls)
            { if (cancel || confirm) Show(controlsReturn, true, cancel ? MenuAction.Cancel : MenuAction.Confirm); }
            else if (Screen == GameScreen.CommandList)
            {
                CommandCharacter = Wrap(CommandCharacter + horizontal, setup.Characters.Length);
                CommandPage = Wrap(CommandPage + vertical, 2);
                if (cancel || confirm) Show(panelReturn, true, cancel ? MenuAction.Cancel : MenuAction.Confirm);
            }
            else if (Screen == GameScreen.KeyConfig)
            {
                MenuIndex = Wrap(MenuIndex + vertical, 17);
                if (MenuIndex == 0 && (horizontal != 0 || confirm)) ConfigPlayer = 1 - ConfigPlayer;
                else if (MenuIndex == 1 && (horizontal != 0 || confirm)) ConfigGamepad = !ConfigGamepad;
                else if (confirm && MenuIndex == 15) input.Configuration.Reset(ConfigPlayer, ConfigGamepad);
                else if (MenuIndex == 16 && (horizontal != 0 || confirm)) input.Configuration.ToggleDoubleTapDash();
                else if (confirm && MenuIndex >= 2 && MenuIndex < 15) { CapturingBinding = true; input.GuardScreenTransition(); }
                if (cancel) Show(panelReturn, true, MenuAction.Cancel);
            }
            else if (Screen == GameScreen.PracticeSettings)
            {
                MenuIndex = Wrap(MenuIndex + vertical, 7);
                if (horizontal != 0 || confirm)
                {
                    if (MenuIndex == 0) Practice.Opponent = (PracticeOpponent)Wrap((int)Practice.Opponent + (horizontal == 0 ? 1 : horizontal), 4);
                    else if (MenuIndex == 1) Practice.FreezeHearts = !Practice.FreezeHearts;
                    else if(MenuIndex==2)Practice.FreezeTimer = !Practice.FreezeTimer;
                    else if(MenuIndex==3){Practice.DebugDefault=!Practice.DebugDefault;runner.SetDebugBoxes(Practice.DebugDefault,false);}
                    else if(MenuIndex==4)runner.CyclePracticeRate();
                    else if(MenuIndex==5)runner.TogglePracticePause();
                    else if(MenuIndex==6){input.Clear();runner.StepPracticeTick();}
                }
                if (cancel) Show(panelReturn, true, MenuAction.Cancel);
            }
            else if (Screen == GameScreen.Settings)
            {
                MenuIndex = Wrap(MenuIndex + vertical, 12);
                if (cancel) Show(settingsReturn, true, MenuAction.Cancel);
                else if (horizontal != 0 || confirm) SettingsAction(horizontal == 0 ? 1 : horizontal, confirm);
            }
            else
            {
                int count = Screen == GameScreen.Pause ? 6 : Screen == GameScreen.MainMenu ? 5 : 3;
                MenuIndex = Wrap(MenuIndex + (Screen == GameScreen.MainMenu ? (horizontal != 0 ? horizontal : vertical) : vertical), count);
                if (cancel)
                {
                    if (Screen == GameScreen.MainMenu) Show(GameScreen.Title, true, MenuAction.Cancel);
                    else if (Screen == GameScreen.Pause) Resume(input.EscapePressed || first.Cancel || second.Cancel ? MenuAction.Cancel : MenuAction.Start);
                }
                else if (confirm) ActivateMenu();
            }
            view.Render(this);
        }
        private void BeginSelection()
        {
            Selection = new TeamSelection(setup.Characters.Length, PracticeMode ? 1 : TeamSeries.TeamSize);
            Series = null; SelectionStatus = ""; Show(GameScreen.CharacterSelect);
        }
        private void CharacterInput(MenuInput first, MenuInput second)
        {
            var current = SelectionPlayer == 0 || CpuMode ? first : second;
            if (input.EscapePressed || first.Cancel || second.Cancel)
            {
                SelectionStatus = "";
                if (!Selection.Undo()) Show(GameScreen.MainMenu, true, MenuAction.Cancel);
                else input.GuardScreenTransition(MenuAction.Cancel);
                return;
            }
            if (current.Horizontal != 0 || current.Vertical != 0)
            {
                int cursor = SelectionPlayer == 0 ? Player1Index : Player2Index;
                cursor = PaperRosterGrid.Move(cursor, current.Horizontal, current.Vertical);
                if (SelectionPlayer == 0) Player1Index = cursor; else Player2Index = cursor;
                SelectionStatus = "";
            }
            if (current.Confirm) ConfirmCharacter();
        }
        private void ConfirmCharacter()
        {
            int cursor = SelectionPlayer == 0 ? Player1Index : Player2Index;
            if (!Selection.Add(cursor)) { SelectionStatus = "同じチームに同じキャラは選べません"; return; }
            SelectionStatus = ""; input.GuardScreenTransition();
            if (Selection.Complete) Show(GameScreen.StageSelect);
        }
        public void SelectRosterCard(int index)
        {
            if (Screen != GameScreen.CharacterSelect || index < 0 || index >= setup.Characters.Length) return;
            if (SelectionPlayer == 0) Player1Index = index; else Player2Index = index;
            ConfirmCharacter();
        }
        private void OpenSettings() { settingsReturn = Screen; settingsMenuIndex = 0; Show(GameScreen.Settings); }
        private void SettingsAction(int direction, bool confirm)
        {
            if (MenuIndex < 2 && audioManager) audioManager.AdjustVolume(MenuIndex == 0, direction * .1f);
            else if (MenuIndex == 2) FramePacing.Cycle(direction);
            else if (MenuIndex == 3 && effects) effects.ToggleShake();
            else if (MenuIndex == 4) { BasicHintsEnabled = !BasicHintsEnabled; PlayerPrefs.SetInt("AniBato.BasicHints", BasicHintsEnabled ? 1 : 0); PlayerPrefs.Save(); }
            else if (MenuIndex == 5) input.Configuration.ToggleDoubleTapDash();
            else if (MenuIndex == 6) runner.SetDebugBoxes(!runner.DebugBoxes);
            else if (MenuIndex == 7)
            { Difficulty = (CpuDifficulty)Wrap((int)Difficulty + direction, 3); PlayerPrefs.SetInt("AniBato.CpuDifficulty", (int)Difficulty); PlayerPrefs.Save(); }
            else if (confirm && MenuIndex == 8) { panelReturn = GameScreen.Settings; Show(GameScreen.KeyConfig); }
            else if (confirm && MenuIndex == 9) Controls();
            else if (confirm && MenuIndex == 10) { CpuMode = PracticeMode = TutorialMode = true; BeginSelection(); }
            else if (confirm && MenuIndex == 11) Show(settingsReturn);
        }
        private void ActivateMenu()
        {
            if (Screen == GameScreen.MainMenu)
            {
                if (MenuIndex < 3)
                { CpuMode = MenuIndex != 1; PracticeMode = MenuIndex == 2; TutorialMode = false; BeginSelection(); }
                else if (MenuIndex == 3) OpenSettings();
                else
                {
                    if(WebInputPolicy.Enabled){Show(GameScreen.Title,true);return;}
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            }
            else if (Screen == GameScreen.Pause)
            {
                if (MenuIndex == 0) Resume();
                else if (MenuIndex == 1) OpenSettings();
                else if (MenuIndex == 2) OpenCommands();
                else if (MenuIndex == 3 && PracticeMode) { panelReturn = Screen; Show(GameScreen.PracticeSettings); }
                else if (MenuIndex == 4) BeginSelection();
                else if (MenuIndex == 5) Show(GameScreen.Title);
            }
            else if (Screen == GameScreen.Result)
            {
                if (MenuIndex == 0)
                {
                    if (HasNextRound) Series.Advance(); else Series?.Reset();
                    StartRound();
                }
                else if (MenuIndex == 1) BeginSelection();
                else Show(GameScreen.Title);
            }
        }
        private void Controls() { controlsReturn = Screen; Show(GameScreen.Controls); }
        private void OpenCommands() { panelReturn = Screen; CommandCharacter = Screen == GameScreen.CharacterSelect && SelectionPlayer == 1 ? Player2Index : Player1Index; CommandPage = 0; Show(GameScreen.CommandList, true, MenuAction.Confirm | MenuAction.Start); }
        private void Resume(MenuAction action = MenuAction.Confirm)
        {
            bool countdown = countdownElapsed < settings.CountdownNumberSeconds * 3 + settings.FightBannerSeconds;
            Show(countdown ? GameScreen.Countdown : GameScreen.Battle, true, action);
            runner.SetPaused(countdown);
        }
        private void StartMatch()
        {
            if (StageIndex == setup.Stages.Length) StageIndex = Random.Range(0, setup.Stages.Length);
            if (!Selection.Complete) throw new System.InvalidOperationException("両チームの選択が未完了です。");
            Series = PracticeMode ? null : new TeamSeries(Selection.Picks(0), Selection.Picks(1));
            if (PracticeMode) { Player1Index = Selection.Picks(0)[0]; Player2Index = Selection.Picks(1)[0]; }
            StartRound();
        }
        private void StartRound()
        {
            if (Series != null) { Player1Index = Series.Entrant(0); Player2Index = Series.Entrant(1); }
            resultRecorded = false;
            world.SetActive(true); hud.SetActive(true);
            runner.Practice = PracticeMode ? Practice : null; runner.Tutorial = TutorialMode ? new TutorialSession() : null;
            runner.BeginMatch(setup.Character(Player1Index), setup.Character(Player2Index), setup.Stages[StageIndex], CpuMode, Difficulty);
            runner.SetDebugBoxes(PracticeMode&&Practice.DebugDefault||PlayerPrefs.GetInt("AniBato.DebugBoxes",0)!=0,false);
            stageView.Rebuild(setup.Stages[StageIndex]); countdownElapsed = resultElapsed = 0; previousCountdown = null;
            Show(GameScreen.Countdown);
        }
        private void Show(GameScreen screen, bool guardInput = true, MenuAction action = MenuAction.Confirm)
        {
            if (screen == GameScreen.Result && Screen != GameScreen.Result && audioManager) audioManager.Play(SoundCue.Result);
            if (Screen == GameScreen.MainMenu) mainMenuIndex = MenuIndex;
            if (Screen == GameScreen.Settings) settingsMenuIndex = MenuIndex;
            Screen = screen; MenuIndex = screen == GameScreen.MainMenu ? mainMenuIndex : screen == GameScreen.Settings ? settingsMenuIndex : 0; if (guardInput) input.GuardScreenTransition(action); else input.Clear();
            bool visible = screen == GameScreen.Battle || screen == GameScreen.Countdown || screen == GameScreen.Pause || screen == GameScreen.Result ||
                screen == GameScreen.Settings && settingsReturn == GameScreen.Pause ||
                screen == GameScreen.Controls && (controlsReturn == GameScreen.Pause || controlsReturn == GameScreen.Settings && settingsReturn == GameScreen.Pause) ||
                (screen == GameScreen.CommandList || screen == GameScreen.KeyConfig || screen == GameScreen.PracticeSettings) && (panelReturn == GameScreen.Pause || panelReturn == GameScreen.Settings && settingsReturn == GameScreen.Pause);
            world.SetActive(visible); hud.SetActive(visible && screen != GameScreen.Result);
            if (audioManager) audioManager.SetMusic(visible && runner.CurrentStage ? (int)runner.CurrentStage.Theme + 1 : 0);
            if (!visible) { runner.Practice = null; runner.Tutorial = null; if (effects) effects.ResetView(); }
            runner.SetPaused(screen != GameScreen.Battle && screen != GameScreen.Result);
            view.Render(this);
        }
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && (Screen == GameScreen.Battle || Screen == GameScreen.Countdown) && runner.Simulation != null && !runner.Simulation.Match.IsFinished)
                Show(GameScreen.Pause, false);
        }
        private static int Wrap(int value, int count) => (value % count + count) % count;
    }
}
