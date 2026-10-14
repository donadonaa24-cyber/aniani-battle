using AniBato.Battle;
using AniBato.Presentation;
using UnityEngine;

namespace AniBato.Game
{
    public enum GameScreen { Title, MainMenu, CharacterSelect, StageSelect, Countdown, Battle, Pause, Result, Controls, CommandList, KeyConfig, PracticeSettings }
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
        private readonly bool[] ready = new bool[2];
        private int cpuRow;
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
        public int CpuRow => cpuRow;
        public bool Ready(int player) => ready[player];
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
                { resultElapsed += Time.unscaledDeltaTime; if (resultElapsed >= settings.ResultDelaySeconds) Show(GameScreen.Result, false); }
                else if (first.Start || second.Start) Show(GameScreen.Pause, true, MenuAction.Start);
            }
            else if (Screen == GameScreen.CharacterSelect)
            { if (input.CommandListPressed) OpenCommands(); else CharacterInput(first, second); }
            else if (Screen == GameScreen.StageSelect)
            {
                StageIndex = Wrap(StageIndex + horizontal + vertical, setup.Stages.Length + 1);
                if (cancel) Show(GameScreen.CharacterSelect, true, MenuAction.Cancel);
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
                MenuIndex = Wrap(MenuIndex + vertical, 3);
                if (horizontal != 0 || confirm)
                {
                    if (MenuIndex == 0) Practice.Opponent = (PracticeOpponent)Wrap((int)Practice.Opponent + (horizontal == 0 ? 1 : horizontal), 4);
                    else if (MenuIndex == 1) Practice.FreezeHearts = !Practice.FreezeHearts;
                    else Practice.FreezeTimer = !Practice.FreezeTimer;
                }
                if (cancel) Show(panelReturn, true, MenuAction.Cancel);
            }
            else
            {
                int count = Screen == GameScreen.Pause ? 13 : Screen == GameScreen.MainMenu ? 9 : 3;
                MenuIndex = Wrap(MenuIndex + vertical, count);
                if (Screen == GameScreen.Pause)
                {
                    if (audioManager && (MenuIndex == 4 || MenuIndex == 5) && horizontal != 0) audioManager.AdjustVolume(MenuIndex == 4, horizontal * .1f);
                    if (effects && MenuIndex == 6 && horizontal != 0) effects.ToggleShake();
                    if (MenuIndex == 7 && horizontal != 0) FramePacing.Cycle(horizontal);
                    if (MenuIndex == 12 && horizontal != 0) input.Configuration.ToggleDoubleTapDash();
                }
                if (Screen == GameScreen.MainMenu && MenuIndex == 3 && horizontal != 0) FramePacing.Cycle(horizontal);
                if (Screen == GameScreen.MainMenu && MenuIndex == 7 && horizontal != 0) input.Configuration.ToggleDoubleTapDash();
                if (cancel)
                {
                    if (Screen == GameScreen.MainMenu) Show(GameScreen.Title, true, MenuAction.Cancel);
                    else if (Screen == GameScreen.Pause) Resume(input.EscapePressed || first.Cancel || second.Cancel ? MenuAction.Cancel : MenuAction.Start);
                }
                else if (confirm) ActivateMenu();
            }
            view.Render(this);
        }
        private void CharacterInput(MenuInput first, MenuInput second)
        {
            if (input.EscapePressed) { Show(GameScreen.MainMenu, true, MenuAction.Cancel); return; }
            if (CpuMode)
            {
                cpuRow = Wrap(cpuRow + first.Vertical, 3);
                if (cpuRow == 0) Player1Index = Wrap(Player1Index + first.Horizontal, setup.Characters.Length);
                if (cpuRow == 1) Player2Index = Wrap(Player2Index + first.Horizontal, setup.Characters.Length);
                if (cpuRow == 2) Difficulty = (CpuDifficulty)Wrap((int)Difficulty + first.Horizontal, 3);
                Player2Index = Wrap(Player2Index + second.Horizontal, setup.Characters.Length);
                Difficulty = (CpuDifficulty)Wrap((int)Difficulty + second.Vertical, 3);
                if (first.Cancel || second.Cancel) Show(GameScreen.MainMenu, true, MenuAction.Cancel);
                else if (first.Confirm) Show(GameScreen.StageSelect, true, MenuAction.ConfirmP1);
            }
            else
            {
                if (first.Cancel)
                { if (!ready[0]) { Show(GameScreen.MainMenu, true, MenuAction.Cancel); return; } ready[0] = false; }
                if (second.Cancel)
                { if (!ready[1]) { Show(GameScreen.MainMenu, true, MenuAction.Cancel); return; } ready[1] = false; }
                if (!ready[0]) Player1Index = Wrap(Player1Index + first.Horizontal, setup.Characters.Length);
                if (!ready[1]) Player2Index = Wrap(Player2Index + second.Horizontal, setup.Characters.Length);
                if (first.Confirm) ready[0] = true;
                if (second.Confirm) ready[1] = true;
                if (first.Confirm || second.Confirm) input.GuardScreenTransition();
                if (ready[0] && ready[1]) Show(GameScreen.StageSelect);
            }
        }
        private void ActivateMenu()
        {
            if (Screen == GameScreen.MainMenu)
            {
                if (MenuIndex < 2) { CpuMode = MenuIndex == 0; PracticeMode = TutorialMode = false; Show(GameScreen.CharacterSelect); }
                else if (MenuIndex == 2) Controls();
                else if (MenuIndex == 3) FramePacing.Cycle(1);
                else if (MenuIndex == 4 || MenuIndex == 5) { CpuMode = PracticeMode = true; TutorialMode = MenuIndex == 5; Show(GameScreen.CharacterSelect); }
                else if (MenuIndex == 6) { panelReturn = Screen; Show(GameScreen.KeyConfig); }
                else if (MenuIndex == 7) input.Configuration.ToggleDoubleTapDash();
                else
                {
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
                else if (MenuIndex == 1) Controls();
                else if (MenuIndex < 4) Show(MenuIndex == 2 ? GameScreen.CharacterSelect : GameScreen.Title);
                else if (MenuIndex == 6 && effects) effects.ToggleShake();
                else if (MenuIndex == 7) FramePacing.Cycle(1);
                else if (MenuIndex == 8) OpenCommands();
                else if (MenuIndex == 9) { panelReturn = Screen; Show(GameScreen.PracticeSettings); }
                else if (MenuIndex == 10) { panelReturn = Screen; Show(GameScreen.KeyConfig); }
                else if (MenuIndex == 11) { BasicHintsEnabled = !BasicHintsEnabled; PlayerPrefs.SetInt("AniBato.BasicHints", BasicHintsEnabled ? 1 : 0); PlayerPrefs.Save(); }
                else if (MenuIndex == 12) input.Configuration.ToggleDoubleTapDash();
            }
            else if (Screen == GameScreen.Result)
            {
                if (MenuIndex == 0) StartMatch();
                else Show(MenuIndex == 1 ? GameScreen.CharacterSelect : GameScreen.Title);
            }
        }
        private void Controls() { controlsReturn = Screen; Show(GameScreen.Controls); }
        private void OpenCommands() { panelReturn = Screen; CommandCharacter = Player1Index; CommandPage = 0; Show(GameScreen.CommandList, true, MenuAction.Confirm | MenuAction.Start); }
        private void Resume(MenuAction action = MenuAction.Confirm)
        {
            bool countdown = countdownElapsed < settings.CountdownNumberSeconds * 3 + settings.FightBannerSeconds;
            Show(countdown ? GameScreen.Countdown : GameScreen.Battle, true, action);
            runner.SetPaused(countdown);
        }
        private void StartMatch()
        {
            if (StageIndex == setup.Stages.Length) StageIndex = Random.Range(0, setup.Stages.Length);
            world.SetActive(true); hud.SetActive(true);
            runner.Practice = PracticeMode ? Practice : null; runner.Tutorial = TutorialMode ? new TutorialSession() : null;
            runner.BeginMatch(setup.Character(Player1Index), setup.Character(Player2Index), setup.Stages[StageIndex], CpuMode, Difficulty);
            stageView.Rebuild(setup.Stages[StageIndex]); countdownElapsed = resultElapsed = 0; previousCountdown = null;
            Show(GameScreen.Countdown);
        }
        private void Show(GameScreen screen, bool guardInput = true, MenuAction action = MenuAction.Confirm)
        {
            if (screen == GameScreen.Result && Screen != GameScreen.Result && audioManager) audioManager.Play(SoundCue.Result);
            if (Screen == GameScreen.MainMenu) mainMenuIndex = MenuIndex;
            Screen = screen; MenuIndex = screen == GameScreen.MainMenu ? mainMenuIndex : 0; if (guardInput) input.GuardScreenTransition(action); else input.Clear();
            if (screen == GameScreen.CharacterSelect) { ready[0] = ready[1] = false; cpuRow = 0; }
            bool visible = screen == GameScreen.Battle || screen == GameScreen.Countdown || screen == GameScreen.Pause || screen == GameScreen.Result ||
                screen == GameScreen.Controls && controlsReturn == GameScreen.Pause ||
                (screen == GameScreen.CommandList || screen == GameScreen.KeyConfig || screen == GameScreen.PracticeSettings) && panelReturn == GameScreen.Pause;
            world.SetActive(visible); hud.SetActive(visible && screen != GameScreen.Result);
            if (audioManager) audioManager.SetMusic(visible && runner.CurrentStage ? (int)runner.CurrentStage.Theme + 1 : 0);
            if (!visible && effects) effects.ResetView();
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
