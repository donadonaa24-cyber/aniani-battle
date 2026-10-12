using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    [Serializable]
    public struct StageShrinkDefinition
    {
        public bool Enabled;
        public int WarningRemainingSeconds;
        public int StartRemainingSeconds;
        public int Steps;
        public int StepIntervalFrames;
        public int StepWarningFrames;
        public float FinalWidthRatio;
        public int UpperPlatformsDisappearStep;
        public void Validate()
        {
            if (!Enabled) return;
            if (StartRemainingSeconds < 1 || WarningRemainingSeconds <= StartRemainingSeconds || Steps < 1 ||
                StepIntervalFrames < 1 || StepWarningFrames < 1 || StepWarningFrames > StepIntervalFrames ||
                (long)Steps * StepIntervalFrames >= (long)StartRemainingSeconds * BattleRuleDefinition.TickRate ||
                FinalWidthRatio <= 0 || FinalWidthRatio >= 1 || UpperPlatformsDisappearStep < 1 || UpperPlatformsDisappearStep > Steps)
                throw new ArgumentException("ステージ縮小の予告・段数・幅が不正です。");
        }
        public static StageShrinkDefinition Default => new StageShrinkDefinition
        {
            Enabled = true, WarningRemainingSeconds = 15, StartRemainingSeconds = 10,
            Steps = 5, StepIntervalFrames = 108, StepWarningFrames = 60,
            FinalWidthRatio = 0.3f, UpperPlatformsDisappearStep = 2
        };
    }

    // Owns a match-local geometry snapshot. Theme/presentation never changes collision.
    public sealed class StageShrinkController
    {
        private readonly StageShrinkDefinition definition;
        private readonly StageGeometry stage;
        private readonly StagePlatform main;
        public bool WarningIssued { get; private set; }
        public bool Started { get; private set; }
        public int CompletedSteps { get; private set; }
        public int PendingStep { get; private set; }
        public bool UpperPlatformsWarned => PendingStep == definition.UpperPlatformsDisappearStep;
        public BattleRect NextMainBounds => BoundsForStep(Math.Min(CompletedSteps + 1, definition.Steps));

        public StageShrinkController(StageGeometry stage, in StageShrinkDefinition definition)
        { this.stage = stage; this.definition = definition; main = stage[0]; definition.Validate(); }

        public void Tick(MatchState match, List<BattleEvent> events)
        {
            if (!definition.Enabled || match.IsFinished) return;
            int startTick = definition.StartRemainingSeconds * BattleRuleDefinition.TickRate;
            if (!WarningIssued && match.RemainingTicks == definition.WarningRemainingSeconds * BattleRuleDefinition.TickRate)
            {
                WarningIssued = true;
                events.Add(new BattleEvent(BattleEventKind.StageShrinkWarning, match.TickCount));
            }
            if (!Started && match.RemainingTicks == startTick)
            {
                Started = true;
                events.Add(new BattleEvent(BattleEventKind.StageShrinkStarted, match.TickCount));
            }
            if (!Started || CompletedSteps >= definition.Steps) return;
            int elapsed = startTick - match.RemainingTicks;
            int removalTick = (CompletedSteps + 1) * definition.StepIntervalFrames;
            if (PendingStep == 0 && elapsed >= removalTick - definition.StepWarningFrames)
            {
                PendingStep = CompletedSteps + 1;
                events.Add(new BattleEvent(BattleEventKind.StageSegmentWarning, match.TickCount, value: PendingStep));
            }
            if (elapsed < removalTick) return;
            CompletedSteps++;
            stage.SetPlatform(0, new StagePlatform(BoundsForStep(CompletedSteps), main.OneWay));
            if (CompletedSteps == definition.UpperPlatformsDisappearStep)
                for (int i = 1; i < stage.PlatformCount; i++) stage.DeactivatePlatform(i);
            PendingStep = 0;
            events.Add(new BattleEvent(BattleEventKind.StageShrunk, match.TickCount, value: CompletedSteps));
        }
        private BattleRect BoundsForStep(int step)
        {
            float progress = definition.Steps > 0 ? step / (float)definition.Steps : 0;
            return new BattleRect(main.Bounds.Center, new BattleVector(
                main.Bounds.Size.X * (1 - (1 - definition.FinalWidthRatio) * progress), main.Bounds.Size.Y));
        }
    }
}
