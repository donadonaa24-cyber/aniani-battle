using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    public sealed class TeamSelection
    {
        private readonly List<int>[] picks = { new List<int>(), new List<int>() };
        private readonly IReadOnlyList<int>[] views;
        private readonly int rosterCount;
        public int Required { get; }
        public int ActivePlayer => picks[0].Count < Required ? 0 : 1;
        public bool Complete => picks[0].Count == Required && picks[1].Count == Required;
        public TeamSelection(int rosterCount, int required = TeamSeries.TeamSize)
        {
            if (required < 1 || rosterCount < required) throw new ArgumentException("チーム選択の人数が不正です。");
            this.rosterCount = rosterCount; Required = required;
            views = new IReadOnlyList<int>[] { picks[0].AsReadOnly(), picks[1].AsReadOnly() };
        }
        public IReadOnlyList<int> Picks(int player) => views[player];
        public int Slot(int player, int character) => picks[player].IndexOf(character);
        public bool Add(int character)
        {
            if (Complete || character < 0 || character >= rosterCount || picks[ActivePlayer].Contains(character)) return false;
            picks[ActivePlayer].Add(character); return true;
        }
        public bool Undo()
        {
            int player = picks[1].Count > 0 ? 1 : 0;
            if (picks[player].Count == 0) return false;
            picks[player].RemoveAt(picks[player].Count - 1); return true;
        }
    }

    public sealed class TeamSeries
    {
        public const int TeamSize = 3, WinsRequired = 2;
        private readonly int[][] teams;
        private readonly int[] scores = new int[2];
        private MatchResult lastResult;
        public int RoundIndex { get; private set; }
        public int RoundNumber => RoundIndex + 1;
        public int Score(int player) => scores[player];
        public int Entrant(int player) => teams[player][RoundIndex];
        public bool AwaitingAdvance { get; private set; }
        public bool Complete => scores[0] == WinsRequired || scores[1] == WinsRequired;
        public int Winner => !Complete ? -1 : scores[0] == WinsRequired ? 0 : 1;
        public TeamSeries(IReadOnlyList<int> first, IReadOnlyList<int> second)
        {
            teams = new[] { Copy(first), Copy(second) };
        }
        private static int[] Copy(IReadOnlyList<int> source)
        {
            if (source == null || source.Count != TeamSize) throw new ArgumentException("チームには異なる3人が必要です。");
            var result = new int[TeamSize]; var seen = new HashSet<int>();
            for (int i = 0; i < TeamSize; i++)
            { if (source[i] < 0 || !seen.Add(source[i])) throw new ArgumentException("同チーム内の重複は禁止です。"); result[i] = source[i]; }
            return result;
        }
        public bool Record(MatchResult result)
        {
            if (result == MatchResult.Playing || Complete || AwaitingAdvance) return false;
            if (result != MatchResult.Player1Wins && result != MatchResult.Player2Wins && result != MatchResult.Draw) throw new ArgumentException("試合結果が不正です。");
            if (result != MatchResult.Draw) scores[result == MatchResult.Player1Wins ? 0 : 1]++;
            lastResult = result; AwaitingAdvance = true; return true;
        }
        public bool Advance()
        {
            if (!AwaitingAdvance || Complete) return false;
            if (lastResult != MatchResult.Draw) RoundIndex++;
            AwaitingAdvance = false; lastResult = MatchResult.Playing; return true;
        }
        public void Reset() { RoundIndex = scores[0] = scores[1] = 0; AwaitingAdvance = false; lastResult = MatchResult.Playing; }
    }
}
