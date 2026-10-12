using System;
using System.Collections.Generic;
using AniBato.Battle;

namespace AniBato.Presentation
{
    // Owned value snapshots. Rendering never writes to the battle simulation.
    public sealed class RenderSnapshots
    {
        private struct Pose
        {
            public BattleVector Position;
            public int Facing;
            public bool Frozen, KnockedOut;
            public Pose(FighterState fighter)
            { Position = fighter.Position; Facing = fighter.Facing; Frozen = fighter.IsFrozenThisTick || fighter.HitstopRemaining > 0; KnockedOut = fighter.Mode == FighterMode.KO; }
        }
        private readonly Pose[] previous = new Pose[2], current = new Pose[2];
        private readonly Dictionary<long, BattleVector> previousShots = new Dictionary<long, BattleVector>();
        private readonly Dictionary<long, BattleVector> currentShots = new Dictionary<long, BattleVector>();
        private BattleSimulation owner;
        public bool Cinematic { get; private set; }
        public float DiscontinuityDistance { get; set; } = 5;
        public void Reset(BattleSimulation simulation)
        {
            owner = simulation; Capture(simulation); Array.Copy(current, previous, 2);
            previousShots.Clear(); foreach (var pair in currentShots) previousShots.Add(pair.Key, pair.Value);
        }
        public void BeforeTick(BattleSimulation simulation)
        {
            if (owner != simulation) Reset(simulation);
            // Also handles externally driven tests and discontinuous state changes.
            Capture(simulation); Array.Copy(current, previous, 2);
            previousShots.Clear(); foreach (var pair in currentShots) previousShots.Add(pair.Key, pair.Value);
        }
        public void AfterTick(BattleSimulation simulation) => Capture(simulation);
        private void Capture(BattleSimulation simulation)
        {
            current[0] = new Pose(simulation.P1); current[1] = new Pose(simulation.P2);
            Cinematic = simulation.CutInRemainingFrames > 0;
            currentShots.Clear(); foreach (var shot in simulation.Projectiles) currentShots[shot.Attack.Id] = shot.Position;
        }
        public bool CanInterpolate(int player, bool paused)
        {
            var a = previous[player]; var b = current[player];
            return !paused && !Cinematic && !a.Frozen && !b.Frozen && !a.KnockedOut && !b.KnockedOut && a.Facing == b.Facing &&
                Math.Abs(b.Position.X - a.Position.X) <= DiscontinuityDistance && Math.Abs(b.Position.Y - a.Position.Y) <= DiscontinuityDistance;
        }
        public BattleVector FighterPosition(int player, double fraction, bool paused = false) =>
            CanInterpolate(player, paused) ? Blend(previous[player].Position, current[player].Position, fraction) : current[player].Position;
        public BattleVector ProjectilePosition(Projectile shot, double fraction, bool paused = false) =>
            !paused && !Cinematic && !current[shot.Owner.PlayerIndex].Frozen && previousShots.TryGetValue(shot.Attack.Id, out var position) && currentShots.TryGetValue(shot.Attack.Id, out var now)
                ? Blend(position, now, fraction) : shot.Position;
        private static BattleVector Blend(BattleVector a, BattleVector b, double fraction)
        { float alpha = (float)Math.Max(0, Math.Min(1, fraction)); return new BattleVector(a.X + (b.X - a.X) * alpha, a.Y + (b.Y - a.Y) * alpha); }
    }
}
