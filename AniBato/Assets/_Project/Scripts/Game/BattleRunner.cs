using System;
using AniBato.Battle;
using UnityEngine;
using AniBato.Presentation;

namespace AniBato.Game
{
    public sealed class BattleRunner : MonoBehaviour
    {
        [SerializeField] private CharacterData p1;
        [SerializeField] private CharacterData p2;
        [SerializeField] private BattleRules rules;
        [SerializeField] private StageData stage;
        [SerializeField] private InputController input;
        [SerializeField] private BattleSetup setup;
        [SerializeField] private PresentationSettings presentation;
        private readonly int[] selected = new int[2];
        private readonly FixedTickClock clock = new FixedTickClock();
        public RenderSnapshots RenderState { get; } = new RenderSnapshots();
        public double RenderFraction => clock.Fraction;
        public BattleVector RenderPosition(int player) => RenderState.FighterPosition(player, clock.Fraction, Paused);
        public BattleVector RenderPosition(Projectile shot) => RenderState.ProjectilePosition(shot, clock.Fraction, Paused);
        private int suppressInputFrame = -1;
        private int finishHoldRemaining = -1;
        private uint supportMatchSequence;
        private ICpuBrain cpuBrain;
        public CpuDifficulty Difficulty { get; private set; }
        public ICpuBrain CpuBrain => cpuBrain;
        private PracticeOptions practice;
        public PracticeOptions Practice
        {
            get => practice;
            set { practice = value; if (Simulation != null) Simulation.Practice = value; }
        }
        public TutorialSession Tutorial { get; set; }
        public FrameInput LastPlayerInput { get; private set; }
        public string LastMoveName { get; private set; } = "";
        private readonly FighterAnimationPlayer[] animations = { new FighterAnimationPlayer(), new FighterAnimationPlayer() };
        public AnimationSample Animation(int player) => animations[player].Sample;
        public bool DebugBoxes { get; private set; }
        public int PracticeRateIndex { get; private set; }
        public bool PracticePlaybackPaused { get; private set; }
        public string PracticeRateLabel => PracticeRateIndex==0?"通常":PracticeRateIndex==1?"1/2":"1/4";
        public void SetDebugBoxes(bool value,bool save=true){DebugBoxes=value;if(save)PlayerPrefs.SetInt("AniBato.DebugBoxes",value?1:0);}
        public void CyclePracticeRate(){PracticeRateIndex=(PracticeRateIndex+1)%presentation.PracticeRates.Length;clock.Reset();}
        public void TogglePracticePause(){PracticePlaybackPaused=!PracticePlaybackPaused;clock.Reset();if(Simulation!=null)RenderState.Reset(Simulation);}
        public bool StepPracticeTick()
        {
            if(Practice==null||!PracticePlaybackPaused||Simulation==null||Simulation.Match.IsFinished)return false;
            AdvanceTick();clock.Reset();RenderState.Reset(Simulation);return true;
        }
        public bool ManagedByFlow { get; private set; }
        public bool Paused { get; private set; }
        public bool CpuOpponent { get; private set; }
        public StageData CurrentStage => stage;
        public BattleSimulation Simulation { get; private set; }
        public event Action<BattleEvent> EventProduced;
        public event Action MatchRestarted;
        public void Configure(CharacterData player1, CharacterData player2, BattleRules battleRules, StageData stageData, InputController controller)
        { p1 = player1; p2 = player2; rules = battleRules; stage = stageData; input = controller; }
        public BattleSetup Setup => setup;
        public CharacterData Character(int player) => player == 0 ? p1 : p2;
        public void ConfigureSetup(BattleSetup value) => setup = value;
        public void ConfigurePresentation(PresentationSettings value)
        { presentation = value; RenderState.DiscontinuityDistance = value.InterpolationDiscontinuityDistance; }
        private int MaximumCatchupTicks => presentation ? System.Math.Max(1, presentation.MaximumCatchupTicks) : 5;
        public void SetManagedByFlow() { ManagedByFlow = true; SetPaused(true); }
        private void Awake() { DebugBoxes=PlayerPrefs.GetInt("AniBato.DebugBoxes",0)!=0;if (presentation) RenderState.DiscontinuityDistance = presentation.InterpolationDiscontinuityDistance; }
        public void SetPaused(bool value)
        { Paused = value; suppressInputFrame = Time.frameCount; clock.Reset(); if (Simulation != null) RenderState.Reset(Simulation); if (input) input.Clear(); }
        public void BeginMatch(CharacterData first, CharacterData second, StageData selectedStage, bool cpu, CpuDifficulty difficulty = CpuDifficulty.Normal)
        { p1 = first; p2 = second; stage = selectedStage; CpuOpponent = cpu; Difficulty = difficulty; RestartMatch(); }
        private void Start()
        {
            if (setup)
            {
                selected[0] = setup.Player1Index; selected[1] = setup.Player2Index;
                p1 = setup.Character(selected[0]); p2 = setup.Character(selected[1]);
            }
            if (!ManagedByFlow) { FramePacing.Load(); RestartMatch(); }
        }
        public void SelectCharacter(int player, int index)
        {
            if (player < 0 || player > 1) throw new ArgumentOutOfRangeException(nameof(player));
            if (!setup) throw new InvalidOperationException("キャラ切替用のBattleSetupが未設定です。BuildAllを実行してください。");
            if (Simulation != null && !Simulation.Match.IsFinished) throw new InvalidOperationException("キャラ変更は試合終了後です。");
            var data = setup.Character(index); selected[player] = index;
            if (player == 0) p1 = data; else p2 = data;
        }
        public void RestartMatch()
        {
            if (!p1 || !p2 || !rules || !stage || !input)
                throw new InvalidOperationException("BattleRunnerの設定が欠落しています。BuildAllを実行してください。");
            var definition=rules.ToDefinition();
            uint supportSeed=unchecked(definition.Support.Seed+(supportMatchSequence++)*0x9e3779b9u);
            Simulation = new BattleSimulation(p1.ToDefinition(), p2.ToDefinition(), definition, stage.ToGeometry(),supportSeed);
            Simulation.Practice = Practice; Tutorial?.Reset(); LastPlayerInput = default; LastMoveName = "";
            if (CpuOpponent)
            {
                if (!setup || setup.CpuDifficulties == null || setup.CpuDifficulties.Length != 3 || !setup.CpuDifficulties[(int)Difficulty])
                    throw new InvalidOperationException("CPU難易度データが未生成です。BuildAllを実行してください。");
                cpuBrain = new CpuBrain(setup.CpuDifficulties[(int)Difficulty].ToDefinition(), setup.CpuSeed);
            }
            else cpuBrain = null;
            animations[0].Configure(p1.Animations ? p1.Animations.Timing : AnimationTiming.Default);
            animations[1].Configure(p2.Animations ? p2.Animations.Timing : AnimationTiming.Default);
            finishHoldRemaining = -1;
            PracticeRateIndex=0;PracticePlaybackPaused=false;
            clock.Reset(); RenderState.Reset(Simulation); input.Clear(); animations[0].Reset(Simulation.P1, Simulation.Match); animations[1].Reset(Simulation.P2, Simulation.Match); MatchRestarted?.Invoke();
        }
        private void Update()
        {
            if (Time.frameCount == suppressInputFrame) { input.Clear(); return; }
            input.Capture();
            if (input.DebugBoxesPressed) SetDebugBoxes(!DebugBoxes);
            if(Practice!=null)
            {
                if(input.PracticeRatePressed)CyclePracticeRate();
                if(input.PracticePausePressed)TogglePracticePause();
                if(input.PracticeStepPressed&&PracticePlaybackPaused&&!Paused)StepPracticeTick();
            }
            if (input.FullscreenPressed) Screen.fullScreen = !Screen.fullScreen;
            if (Paused) { input.Clear(); return; }
            if (Simulation == null) return;
            if (Simulation.Match.IsFinished)
            {
                if (setup && !ManagedByFlow)
                    for (int player = 0; player < 2; player++)
                    {
                        int delta = input.CharacterSelectionDelta(player);
                        if (delta != 0) SelectCharacter(player, (selected[player] + delta + setup.Characters.Length) % setup.Characters.Length);
                    }
                if (!ManagedByFlow && input.RematchPressed) RestartMatch(); else clock.Advance(Time.unscaledDeltaTime, AdvanceTick, MaximumCatchupTicks); return;
            }
            AdvanceRenderFrame(Time.unscaledDeltaTime);
        }
        public void AdvanceRenderFrame(double elapsedSeconds)
        { if (!Paused && Simulation != null) clock.Advance(Practice==null?elapsedSeconds:PracticePlayback.Scale(elapsedSeconds,PracticeRateIndex,PracticePlaybackPaused,presentation.PracticeRates), AdvanceTick, MaximumCatchupTicks); }
        private void AdvanceTick()
        {
            if (Simulation.Match.IsFinished)
            {
                RenderState.Reset(Simulation);
                if (finishHoldRemaining < 0) finishHoldRemaining = presentation ? presentation.FinishHoldTicks : 0;
                if (finishHoldRemaining > 0) { finishHoldRemaining--; return; }
                animations[0].Advance(Simulation.P1, Simulation.Match, false, false, false);
                animations[1].Advance(Simulation.P2, Simulation.Match, false, false, false); return;
            }
            bool cinematic = Simulation.CutInRemainingFrames > 0;
            var first = input.Consume(0); var second = input.Consume(1);
            LastPlayerInput = first; Tutorial?.Prepare(Simulation);
            if (Tutorial != null) second = Tutorial.CoachInput(Simulation);
            else if (Practice != null) second = Practice.Opponent == PracticeOpponent.Cpu ? cpuBrain.NextInput(Simulation, 1) : Practice.DummyInput(Simulation);
            else if (CpuOpponent) second = cpuBrain.NextInput(Simulation, 1);
            RenderState.BeforeTick(Simulation);
            Simulation.Tick(first, second);
            Tutorial?.Observe(Simulation, first);
            RenderState.AfterTick(Simulation);
            var success = new bool[2];
            foreach (BattleEvent battleEvent in Simulation.Events)
            {
                if (battleEvent.Kind == BattleEventKind.CounterSuccess) success[battleEvent.SourcePlayer] = true;
                if (battleEvent.Kind == BattleEventKind.AttackStarted && battleEvent.SourcePlayer == 0 && Simulation.P1.Attack != null) LastMoveName = Simulation.P1.Attack.Definition.Name;
                EventProduced?.Invoke(battleEvent);
            }
            cinematic |= Simulation.CutInRemainingFrames > 0;
            // Preserve the final action briefly before changing to the winner/loser poses.
            if (Simulation.Match.IsFinished) { finishHoldRemaining = presentation ? presentation.FinishHoldTicks : 0; return; }
            animations[0].Advance(Simulation.P1, Simulation.Match, Simulation.P1.IsFrozenThisTick, cinematic, success[0]);
            animations[1].Advance(Simulation.P2, Simulation.Match, Simulation.P2.IsFrozenThisTick, cinematic, success[1]);
        }
        public Sprite EffectSprite(string name, int age)
        {
            if (name == "mogu" && setup && setup.Characters != null)
                foreach (var data in setup.Characters)
                    if (data && data.PaperMotion && data.PaperMotion.Enabled && data.PaperMotion.Mogu) return data.PaperMotion.Mogu;
            if (setup && setup.Characters != null)
                foreach (var data in setup.Characters)
                    if (data && data.Animations) { var sprite = data.Animations.Effect(name, age); if (sprite) return sprite; }
            for (int i = 0; i < 2; i++) { var data = Character(i); if (data && data.Animations) { var sprite = data.Animations.Effect(name, age); if (sprite) return sprite; } }
            return null;
        }
        private void OnApplicationFocus(bool focus) { if (!focus) clock.Reset(); }
    }
}



