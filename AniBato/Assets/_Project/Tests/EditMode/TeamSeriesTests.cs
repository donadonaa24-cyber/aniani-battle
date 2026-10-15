using System;
using AniBato.Battle;
using NUnit.Framework;

namespace AniBato.Tests
{
    public sealed class TeamSeriesTests
    {
        [Test]
        public void SelectionIsSequentialUniqueWithinEachTeamAndUndoCrossesTheBoundary()
        {
            var draft = new TeamSelection(9);
            Assert.That(draft.ActivePlayer, Is.Zero); Assert.That(draft.Undo(), Is.False);
            Assert.That(draft.Add(-1), Is.False); Assert.That(draft.Add(9), Is.False);
            Assert.That(draft.Add(0), Is.True); Assert.That(draft.Add(0), Is.False);
            Assert.That(draft.Picks(0).Count, Is.EqualTo(1));
            Assert.That(draft.Add(3) && draft.Add(4), Is.True);
            Assert.That(draft.ActivePlayer, Is.EqualTo(1)); Assert.That(draft.Picks(1).Count, Is.Zero);
            Assert.That(draft.Undo(), Is.True); Assert.That(draft.ActivePlayer, Is.Zero);
            Assert.That(draft.Picks(0).Count, Is.EqualTo(2)); Assert.That(draft.Add(4), Is.True);
            Assert.That(draft.Add(0) && draft.Add(3) && draft.Add(4), Is.True, "相手チームとの重複は可能");
            Assert.That(draft.Complete, Is.True); Assert.That(draft.Add(8), Is.False);
            Assert.That(draft.Undo(), Is.True); Assert.That(draft.Complete, Is.False);
            Assert.That(draft.Slot(0, 3), Is.EqualTo(1)); Assert.That(draft.Slot(1, 4), Is.EqualTo(-1));
        }
        [TestCase(true)] [TestCase(false)]
        public void TwoStraightWinsFinishWithoutStartingAThirdRound(bool first)
        {
            var series = new TeamSeries(new[] { 0, 3, 4 }, new[] { 1, 5, 6 });
            var result = first ? MatchResult.Player1Wins : MatchResult.Player2Wins;
            Assert.That(series.Advance(), Is.False); Assert.That(series.Record(MatchResult.Playing), Is.False);
            Assert.That(series.Entrant(0), Is.Zero); Assert.That(series.Entrant(1), Is.EqualTo(1));
            Assert.That(series.Record(result), Is.True); Assert.That(series.Record(result), Is.False);
            Assert.That(series.Advance(), Is.True); Assert.That(series.RoundNumber, Is.EqualTo(2));
            Assert.That(series.Entrant(0), Is.EqualTo(3)); Assert.That(series.Entrant(1), Is.EqualTo(5));
            Assert.That(series.Record(result), Is.True); Assert.That(series.Complete, Is.True);
            Assert.That(series.Winner, Is.EqualTo(first ? 0 : 1)); Assert.That(series.Score(first ? 0 : 1), Is.EqualTo(2));
            Assert.That(series.Advance(), Is.False); Assert.That(series.RoundNumber, Is.EqualTo(2));
        }
        [Test]
        public void SplitWinsUseTheThirdSelectedEntrantsAndRematchResetsScores()
        {
            var a = new[] { 0, 3, 4 }; var series = new TeamSeries(a, new[] { 1, 5, 6 }); a[0] = 8;
            Assert.That(series.Entrant(0), Is.Zero, "選択は開始時にコピーする");
            series.Record(MatchResult.Player1Wins); series.Advance(); series.Record(MatchResult.Player2Wins); series.Advance();
            Assert.That(series.RoundNumber, Is.EqualTo(3)); Assert.That(series.Entrant(0), Is.EqualTo(4)); Assert.That(series.Entrant(1), Is.EqualTo(6));
            Assert.That(series.Score(0), Is.EqualTo(1)); Assert.That(series.Score(1), Is.EqualTo(1));
            series.Record(MatchResult.Player2Wins); Assert.That(series.Winner, Is.EqualTo(1));
            series.Reset(); Assert.That(series.RoundNumber, Is.EqualTo(1)); Assert.That(series.Score(0) + series.Score(1), Is.Zero);
            Assert.That(series.Complete || series.AwaitingAdvance, Is.False); Assert.That(series.Entrant(0), Is.Zero);
        }
        [Test]
        public void DrawReplaysTheSameRoundWithoutPointsAndDuplicateTeamsAreRejected()
        {
            var series = new TeamSeries(new[] { 0, 1, 2 }, new[] { 0, 1, 2 });
            for (int i = 0; i < 4; i++)
            { Assert.That(series.Record(MatchResult.Draw), Is.True); Assert.That(series.Advance(), Is.True); }
            Assert.That(series.RoundNumber, Is.EqualTo(1)); Assert.That(series.Score(0) + series.Score(1), Is.Zero);
            Assert.That(series.Winner, Is.EqualTo(-1));
            Assert.Throws<ArgumentException>(() => new TeamSeries(new[] { 0, 0, 1 }, new[] { 1, 2, 3 }));
            Assert.Throws<ArgumentException>(() => new TeamSeries(new[] { 0, 1 }, new[] { 1, 2, 3 }));
        }
        [Test]
        public void PracticeSelectsOnePerSideWithoutBuildingATeamSeries()
        {
            var draft = new TeamSelection(9, 1); draft.Add(2);
            Assert.That(draft.ActivePlayer, Is.EqualTo(1)); draft.Add(2); Assert.That(draft.Complete, Is.True);
            Assert.That(draft.Picks(0).Count, Is.EqualTo(1)); Assert.That(draft.Picks(1).Count, Is.EqualTo(1));
        }
    }
}
