using System.Collections.Generic;

namespace AniBato.Battle
{
    public sealed class AttackInstance
    {
        private readonly HashSet<int> hitTargets = new HashSet<int>();
        public long Id { get; }
        public AttackDefinition Definition { get; }
        public int ChainIndex { get; }
        public bool AirborneAttack { get; }
        public int ChargeFrames { get; internal set; }
        public bool Clashed { get; internal set; }
        public int Frame { get; internal set; }
        public bool NextQueued { get; internal set; }
        internal bool ProjectileSpawned;
        internal bool SelfEffectApplied;
        internal int ErasuresRemaining;
        internal int StanceRemaining;
        public bool IsActive => !Clashed && Frame >= Definition.StartupFrames && Frame < Definition.StartupFrames + Definition.ActiveFrames;
        public bool IsDamageActive => IsActive && Frame >= Definition.StartupFrames + Definition.DamageDelayFrames;
        public bool IsComplete => Frame >= Definition.TotalFrames;
        public bool HasHit(int playerIndex) => hitTargets.Contains(playerIndex);
        internal bool MarkHit(int playerIndex) => hitTargets.Add(playerIndex);
        public BattleRect WorldHitbox(FighterState owner) => Definition.Hitbox.At(owner.Position, owner.Facing);
        public bool Overlaps(FighterState owner, in BattleRect target) => WorldHitbox(owner).Overlaps(target) ||
            (Definition.HasSecondaryHitbox && Definition.SecondaryHitbox.At(owner.Position, owner.Facing).Overlaps(target));
        internal AttackInstance(long id, in AttackDefinition definition, int chainIndex, bool air)
        {
            Id = id; Definition = definition; ChainIndex = chainIndex; AirborneAttack = air;
            ErasuresRemaining = definition.ProjectileEraseCount; StanceRemaining = definition.StanceFrames;
        }
    }
}
