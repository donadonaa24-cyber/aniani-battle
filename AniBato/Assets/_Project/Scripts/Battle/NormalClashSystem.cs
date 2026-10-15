using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    public sealed class NormalClashSystem
    {
        private readonly List<Part> first = new List<Part>(), second = new List<Part>();
        private readonly struct Part
        {
            public readonly FighterState Owner;
            public readonly AttackInstance Attack;
            public readonly BattleRect Bounds;
            public Part(FighterState owner, AttackInstance attack, BattleRect bounds) { Owner = owner; Attack = attack; Bounds = bounds; }
        }
        public void Resolve(FighterState p1, FighterState p2, IReadOnlyList<Projectile> projectiles, int tick, in CombatInputDefinition rules, List<BattleEvent> events)
        {
            first.Clear(); second.Clear(); Gather(p1, first); Gather(p2, second);
            foreach (var shot in projectiles)
                if (shot.RemainingFrames > 0 && shot.Attack.Definition.IsNormal && !shot.Attack.Clashed && !shot.Attack.HasHit(1 - shot.Owner.PlayerIndex))
                    (shot.Owner == p1 ? first : second).Add(new Part(shot.Owner, shot.Attack, shot.Hitbox));
            foreach (var a in first)
                foreach (var b in second)
                {
                    if (a.Attack.Clashed || b.Attack.Clashed || !a.Bounds.Overlaps(b.Bounds)) continue;
                    Bounce(a, rules); Bounce(b, rules);
                    events.Add(new BattleEvent(BattleEventKind.Clash, tick, a.Owner.PlayerIndex, b.Owner.PlayerIndex, a.Attack.Id));
                }
        }
        private static void Gather(FighterState fighter, List<Part> parts)
        {
            var a = fighter.Attack;
            if (a == null || !a.IsDamageActive || !a.Definition.IsNormal || a.Definition.SpawnProjectile || a.Definition.NonDamaging ||
                a.HasHit(1 - fighter.PlayerIndex) || fighter.IsFrozenThisTick || fighter.Mode == FighterMode.KO) return;
            parts.Add(new Part(fighter, a, a.WorldHitbox(fighter)));
            if (a.Definition.HasSecondaryHitbox) parts.Add(new Part(fighter, a, a.Definition.SecondaryHitbox.At(fighter.Position, fighter.Facing)));
        }
        private static void Bounce(in Part part, in CombatInputDefinition rules)
        {
            part.Attack.Clashed = true; part.Attack.NextQueued = false;
            var owner = part.Owner;
            owner.HitstopRemaining = Math.Max(owner.HitstopRemaining, rules.ClashStopFrames);
            if (owner.Attack != part.Attack) return;
            owner.Attack = null; owner.ChargeFrames = 0; owner.DashRemaining = 0;
            owner.Velocity = new BattleVector(0, owner.Velocity.Y);
            owner.Mode = FighterMode.Recovery; owner.ActionFrames = rules.ClashRecoveryFrames;
            owner.InputBuffer.ClearActions();
        }
    }
}
