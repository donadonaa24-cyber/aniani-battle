using AniBato.Battle;
using AniBato.Presentation;
using UnityEngine;

namespace AniBato.Game
{
    public enum GameScreen { Title, MainMenu, CharacterSelect, StageSelect, Countdown, Battle, Pause, Result, Controls }
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
                if (input.EscapePressed || first.Start || second.Start) { Show(GameScreen.Pause, true, input.EscapePressed ? MenuAction.Cancel : MenuAction.Start); return; }
                countdownElapsed += Time.unscaledDeltaTime;
                if (CountdownText != previousCountdown) { previousCountdown = CountdownText; if (audioManager) audioManager.Play(SoundCue.Countdown); }
                if (countdownElapsed >= settings.CountdownNumberSeconds * 3 + settings.FightBannerSeconds)
                { Show(GameScreen.Battle, false); runner.SetPaused(false); }
            }
            else if (Screen == GameScreen.Battle)
            {
                if (runner.Simulation.Match.IsFinished)
                { resultElapsed += Time.unscaledDeltaTime; if (resultElapsed >= settings.ResultDelaySeconds) Show(GameScreen.Result, false); }
                else if (input.EscapePressed || first.Start || second.Start) Show(GameScreen.Pause, true, input.EscapePressed ? MenuAction.Cancel : MenuAction.Start);
            }
            else if (Screen == GameScreen.CharacterSelect) CharacterInput(first, second);
            else if (Screen == GameScreen.StageSelect)
            {
                StageIndex = Wrap(StageIndex + horizontal + vertical, setup.Stages.Length + 1);
                if (cancel) Show(GameScreen.CharacterSelect, true, MenuAction.Cancel);
                else if (confirm) StartMatch();
            }
            else if (Screen == GameScreen.Controls)
            { if (cancel || confirm) Show(controlsReturn, true, cancel ? MenuAction.Cancel : MenuAction.Confirm); }
            else
            {
                int count = Screen == GameScreen.Pause ? 8 : Screen == GameScreen.MainMenu ? 5 : 3;
                MenuIndex = Wrap(MenuIndex + vertical, count);
                if (Screen == GameScreen.Pause)
                {
                    if (audioManager && (MenuIndex == 4 || MenuIndex == 5) && horizontal != 0) audioManager.AdjustVolume(MenuIndex == 4, horizontal * .1f);
                    if (effects && MenuIndex == 6 && horizontal != 0) effects.ToggleShake();
                    if (MenuIndex == 7 && horizontal != 0) FramePacing.Cycle(horizontal);
                }
                if (Screen == GameScreen.MainMenu && MenuIndex == 3 && horizontal != 0) FramePacing.Cycle(horizontal);
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
                if (MenuIndex < 2) { CpuMode = MenuIndex == 0; Show(GameScreen.CharacterSelect); }
                else if (MenuIndex == 2) Controls();
                else if (MenuIndex == 3) FramePacing.Cycle(1);
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
            }
            else if (Screen == GameScreen.Result)
            {
                if (MenuIndex == 0) StartMatch();
                else Show(MenuIndex == 1 ? GameScreen.CharacterSelect : GameScreen.Title);
            }
        }
        private void Controls() { controlsReturn = Screen; Show(GameScreen.Controls); }
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
            runner.BeginMatch(setup.Character(Player1Index), setup.Character(Player2Index), setup.Stages[StageIndex], CpuMode, Difficulty);
            stageView.Rebuild(setup.Stages[StageIndex]); countdownElapsed = resultElapsed = 0; previousCountdown = null;
            Show(GameScreen.Countdown);
        }
        private void Show(GameScreen screen, bool guardInput = true, MenuAction action = MenuAction.Confirm)
        {
            if (Screen == GameScreen.MainMenu) mainMenuIndex = MenuIndex;
            Screen = screen; MenuIndex = screen == GameScreen.MainMenu ? mainMenuIndex : 0; if (guardInput) input.GuardScreenTransition(action); else input.Clear();
            if (screen == GameScreen.CharacterSelect) { ready[0] = ready[1] = false; cpuRow = 0; }
            bool visible = screen == GameScreen.Battle || screen == GameScreen.Countdown || screen == GameScreen.Pause || screen == GameScreen.Result ||
                screen == GameScreen.Controls && controlsReturn == GameScreen.Pause;
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
