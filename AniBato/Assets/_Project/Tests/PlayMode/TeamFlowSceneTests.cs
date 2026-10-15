using System.Collections;
using System.Linq;
using System.Reflection;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AniBato.Tests
{
    public sealed partial class GameFlowSceneTests
    {
        private void FastForwardPanel(string field) => typeof(GameFlowController).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Flow, 1000f);
        private IEnumerator EndTeamRound(bool firstWins)
        {
            var sim = Flow.Runner.Simulation;
            typeof(FighterState).GetProperty("Hearts").SetValue(firstWins ? sim.P2 : sim.P1, 0);
            sim.Tick(default, default);
            Assert.That(sim.Match.Result, Is.EqualTo(firstWins ? MatchResult.Player1Wins : MatchResult.Player2Wins));
            FastForwardPanel("resultElapsed");
            yield return WaitFor(() => Flow.Screen == GameScreen.Result, "チームのラウンド結果へ進みません。");
        }
        private void AssertNewRound(int round, int first, int second)
        {
            var sim = Flow.Runner.Simulation;
            Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Countdown)); Assert.That(Flow.Runner.Paused, Is.True);
            Assert.That(Flow.Series.RoundNumber, Is.EqualTo(round));
            Assert.That(Flow.Player1Index, Is.EqualTo(first)); Assert.That(Flow.Player2Index, Is.EqualTo(second));
            Assert.That(sim.P1.Hearts, Is.EqualTo(3)); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            Assert.That(sim.P1.Gauge + sim.P2.Gauge + sim.P1.NormalHitCount + sim.P2.NormalHitCount, Is.Zero);
            Assert.That(sim.P1.UltimateUses + sim.P2.UltimateUses + sim.Match.TickCount, Is.Zero);
            Assert.That(sim.Match.RemainingTicks, Is.EqualTo(4200)); Assert.That(sim.Shrink.CompletedSteps, Is.Zero);
            Assert.That(sim.Stage[0].Bounds.Size.X, Is.EqualTo(36)); Assert.That(sim.Stage.IsActive(1) && sim.Stage.IsActive(2), Is.True);
            Assert.That(sim.P1.Position, Is.EqualTo(sim.Stage.SpawnP1)); Assert.That(sim.P2.Position, Is.EqualTo(sim.Stage.SpawnP2));
        }
        [UnityTest]
        public IEnumerator TeamSelectionUsesBothAxesSequentialPicksDuplicateRejectionAndUndo()
        {
            yield return ChooseMain(0);
            Assert.That(Flow.SelectionPlayer, Is.Zero); yield return Tap(keyboard.sKey);
            Assert.That(Flow.Player1Index, Is.EqualTo(3)); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.Selection.Picks(0).Count, Is.EqualTo(1));
            yield return Tap(keyboard.enterKey); Assert.That(Flow.Selection.Picks(0).Count, Is.EqualTo(1));
            Assert.That(Flow.SelectionStatus, Does.Contain("同じキャラ"));
            yield return Tap(keyboard.wKey); Assert.That(Flow.Player1Index, Is.Zero); yield return Tap(keyboard.enterKey);
            yield return Tap(keyboard.sKey); yield return Tap(keyboard.sKey); yield return Tap(keyboard.enterKey);
            Assert.That(Flow.Selection.Picks(0), Is.EqualTo(new[] { 3, 0, 4 })); Assert.That(Flow.SelectionPlayer, Is.EqualTo(1));
            CaptureScreen("01_CPUチーム選択",720,"rework_r2");
            yield return Tap(keyboard.sKey); Assert.That(Flow.Player2Index, Is.EqualTo(5));
            Assert.That(Flow.Player1Index, Is.EqualTo(4)); yield return Tap(keyboard.enterKey);
            yield return Tap(keyboard.escapeKey); Assert.That(Flow.Selection.Picks(1).Count, Is.Zero);
            yield return Tap(keyboard.escapeKey); Assert.That(Flow.SelectionPlayer, Is.Zero); Assert.That(Flow.Selection.Picks(0).Count, Is.EqualTo(2));
            yield return Pick(4,0,1,2); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.StageSelect));
            Assert.That(Flow.Selection.Picks(1), Is.EqualTo(new[] { 0, 1, 2 }), "相手チームと同じキャラは選べる");
            yield return Tap(keyboard.escapeKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.CharacterSelect));
            Assert.That(Flow.Selection.Picks(1).Count, Is.EqualTo(2)); CaptureScreen("02_選択の取消",720,"rework_r2");
        }
        [UnityTest]
        public IEnumerator TwoStraightWinsEndAtRoundTwoAndRematchRestartsTheSameTeams()
        {
            yield return SelectLocalBattle(); Flow.Runner.enabled = false;
            var old = Flow.Runner.Simulation; yield return EndTeamRound(true);
            Assert.That(Flow.Series.Score(0), Is.EqualTo(1)); Assert.That(Flow.HasNextRound, Is.True);
            CaptureScreen("03_ラウンド1結果",720,"rework_r2");
            yield return Tap(keyboard.enterKey); AssertNewRound(2,5,3);
            Assert.That(Flow.Runner.Simulation, Is.Not.SameAs(old));
            FastForwardPanel("countdownElapsed"); yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "ラウンド2開始失敗");
            yield return EndTeamRound(true); Assert.That(Flow.Series.Complete, Is.True);
            Assert.That(Flow.Series.RoundNumber, Is.EqualTo(2)); Assert.That(Flow.Series.Score(0), Is.EqualTo(2));
            Assert.That(Flow.HasNextRound, Is.False); CaptureScreen("04_2対0のチーム決着",720,"rework_r2");
            yield return Tap(keyboard.enterKey); AssertNewRound(1,1,0);
            Assert.That(Flow.Series.Score(0) + Flow.Series.Score(1), Is.Zero);
        }
        [UnityTest]
        public IEnumerator SplitWinsUseThirdEntrantsAndSettingsStayInOneSeparatePanel()
        {
            yield return ChooseMain(3); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Settings));
            var choices = Object.FindObjectsByType<Text>().Single(t => t.text.Contains("BGM音量") && t.text.Contains("キーコンフィグ"));
            Assert.That(choices.text, Does.Contain("フレームレート").And.Contain("CPU難易度").And.Contain("SE音量"));
            CaptureScreen("05_設定画面",720,"rework_r2");
            yield return Tap(keyboard.escapeKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.MainMenu));
            choices = Object.FindObjectsByType<Text>().Single(t => t.text.Contains("CPUチーム対戦"));
            Assert.That(choices.text.Split('\n').Length, Is.EqualTo(5)); Assert.That(choices.text, Does.Not.Contain("フレームレート"));
            yield return ChooseMain(1); yield return Pick(1,5,6,0,3,4); yield return Tap(keyboard.enterKey);
            FastForwardPanel("countdownElapsed"); yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "対戦開始失敗");
            Flow.Runner.enabled = false; yield return EndTeamRound(true); yield return Tap(keyboard.enterKey);
            AssertNewRound(2,5,3); FastForwardPanel("countdownElapsed"); yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "ラウンド2開始失敗");
            yield return EndTeamRound(false); yield return Tap(keyboard.enterKey); AssertNewRound(3,6,4);
            Assert.That(Flow.Series.Score(0), Is.EqualTo(1)); Assert.That(Flow.Series.Score(1), Is.EqualTo(1));
            FastForwardPanel("countdownElapsed"); yield return WaitFor(() => Flow.Screen == GameScreen.Battle, "ラウンド3開始失敗");
            yield return EndTeamRound(true); Assert.That(Flow.Series.Winner, Is.Zero);
            Assert.That(Flow.Series.Score(0), Is.EqualTo(2)); Assert.That(Flow.Series.Score(1), Is.EqualTo(1));
            CaptureScreen("06_2対1のチーム決着",720,"rework_r2");
        }
    }
}
