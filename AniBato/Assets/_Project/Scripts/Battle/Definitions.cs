using System;

namespace AniBato.Battle
{
    public enum AttackKind { Normal, Special, Ultimate }
    public enum SpecialSlot { Neutral, Side, Up, Down }

    [Serializable]
    public struct AttackDefinition
    {
        public string Name;
        public AttackAbility Ability;
        public AttackKind Kind;
        public bool IsCloseRange;
        public NormalDirection NormalDirection;
        public bool BreaksGuard;
        public int LandingRecoveryFrames;
        public int StartupFrames;
        public int ActiveFrames;
        public int DamageDelayFrames;
        public int RecoveryFrames;
        public BattleRect Hitbox;
        public int HitstopFrames;
        public int HitstunFrames;
        public BattleVector Knockback;
        public bool PiercesNormalDodge;
        public bool SpawnProjectile;
        public float ProjectileSpeed;
        public int ProjectileLifetimeFrames;
        public BattleVector ActiveVelocity;
        public float NormalForwardDistance;
        public float DamageRecoilDistance;
        public int DamageRecoilFrames;
        public float NormalForwardSpeed => IsNormal ? NormalForwardDistance * BattleRuleDefinition.TickRate / (StartupFrames + ActiveFrames) : 0;
        public bool VerticalMotionOnStartOnly;
        public BattleVector ProjectileLaunchVelocity;
        public float ProjectileGravity;
        public bool HasSecondaryHitbox;
        public BattleRect SecondaryHitbox;
        public bool NonDamaging;
        public int ParalysisFrames;
        public int MarkFrames;
        public int SelfHealNormalHits;
        public BuffDefinition SelfBuff;
        public float KnockbackMultiplier;
        public int CooldownFrames;
        public int ProjectileEraseCount; // -1 = every overlapping projectile during the active window.
        public int GuardedRecoveryFrames;
        public bool RequiresFollowup;
        public int StanceFrames;
        public bool IsNormal => Kind == AttackKind.Normal;
        public bool IsSpecial => Kind == AttackKind.Special;
        public bool IsUltimate => Kind == AttackKind.Ultimate;
        public int TotalFrames => StartupFrames + ActiveFrames + RecoveryFrames;

        public void Validate()
        {
            if (LandingRecoveryFrames < 0 || StartupFrames < 0 || ActiveFrames < 1 || DamageDelayFrames < 0 || DamageDelayFrames >= ActiveFrames || RecoveryFrames < 0 || HitstopFrames < 0 ||
                HitstunFrames < 0 || Hitbox.Size.X <= 0 || Hitbox.Size.Y <= 0 ||
                (SpawnProjectile && (ProjectileSpeed < 0 || ProjectileSpeed == 0 && !Ability.AllowZeroProjectileSpeed || ProjectileLifetimeFrames < 1)) ||
                (Ability.IndependentNormalProjectile && (!SpawnProjectile || !IsNormal)) ||
                NormalForwardDistance < 0 || float.IsNaN(NormalForwardDistance) || float.IsInfinity(NormalForwardDistance) ||
                DamageRecoilDistance < 0 || float.IsNaN(DamageRecoilDistance) || float.IsInfinity(DamageRecoilDistance) ||
                DamageRecoilFrames < 0 || DamageRecoilDistance > 0 && DamageRecoilFrames < 1 ||
                ProjectileGravity < 0 || ParalysisFrames < 0 || MarkFrames < 0 || SelfHealNormalHits < 0 ||
                CooldownFrames < 0 || KnockbackMultiplier < 0 || ProjectileEraseCount < -1 || GuardedRecoveryFrames < 0 ||
                (RequiresFollowup && (StartupFrames < 1 || StanceFrames < 1)) ||
                (HasSecondaryHitbox && (SecondaryHitbox.Size.X <= 0 || SecondaryHitbox.Size.Y <= 0)))
                throw new ArgumentException("攻撃のフレーム数・判定サイズが不正です。");
            SelfBuff.Validate();
            Ability.Validate();
        }
    }

    [Serializable]
    public struct CharacterDefinition
    {
        public string Name;
        public BattleVector HurtboxSize;
        public float MoveSpeed;
        public float DashSpeed;
        public int GroundDashFrames;
        public int AirDashFrames;
        public float JumpSpeed;
        public float Gravity;
        public float MaxFallSpeed;
        public float FastFallSpeed;
        public int MaxJumps;
        public int MaxAirDashes;
        public AttackDefinition[] GroundNormals;
        public AttackDefinition[] AirNormals;
        public AttackDefinition[] DirectionalNormals;
        public AttackDefinition UpNormal;
        public StaminaDefinition Stamina;
        public float CrouchHeightRatio, DiagonalThreshold;
        public bool DiagonalFallbackVertical;
        public AttackDefinition[] Specials;
        public AttackDefinition Ultimate;
        public CounterEffectDefinition Counter;
        public int UltimateCutInFrames;
        public AttackDefinition Special(SpecialSlot slot) => Specials[(int)slot];

        // The simulation takes an owned snapshot; editing assets/arrays cannot change a running match.
        public CharacterDefinition Snapshot()
        {
            if (HurtboxSize.X <= 0 || HurtboxSize.Y <= 0 || MoveSpeed <= 0 || DashSpeed <= 0 ||
                GroundDashFrames < 1 || AirDashFrames < 1 || JumpSpeed <= 0 || Gravity <= 0 ||
                MaxFallSpeed <= 0 || FastFallSpeed < MaxFallSpeed || MaxJumps < 1 || MaxAirDashes < 0)
                throw new ArgumentException("キャラクターの移動定義が不正です。");
            ValidateNormals(GroundNormals);
            ValidateNormals(AirNormals);
            Stamina.Validate();
            if (CrouchHeightRatio < 0 || CrouchHeightRatio > 1 || DiagonalThreshold < 0 || DiagonalThreshold > 1)
                throw new ArgumentException("Invalid directional input or crouch definition.");
            if (DirectionalNormals != null)
                foreach (var attack in DirectionalNormals)
                    if (attack.ActiveFrames > 0) { attack.Validate(); if (!attack.IsNormal) throw new ArgumentException("Directional attacks must be normal."); }
            if (UpNormal.ActiveFrames > 0) { UpNormal.Validate(); if (!UpNormal.IsNormal) throw new ArgumentException("上攻撃は通常攻撃です。"); }
            if (Specials == null || Specials.Length != 4) throw new ArgumentException("特殊攻撃は4枠必要です。");
            foreach (var attack in Specials)
            { attack.Validate(); if (!attack.IsSpecial) throw new ArgumentException("特殊攻撃の分類が不正です。"); }
            Ultimate.Validate();
            Counter.Validate();
            if (UltimateCutInFrames < 0 || UltimateCutInFrames > 60) throw new ArgumentException("カットインは0〜60Fです。");
            if (!Ultimate.IsUltimate) throw new ArgumentException("超必殺技の分類が不正です。");
            var copy = this;
            copy.GroundNormals = (AttackDefinition[])GroundNormals.Clone();
            copy.AirNormals = (AttackDefinition[])AirNormals.Clone();
            copy.DirectionalNormals = DirectionalNormals == null ? null : (AttackDefinition[])DirectionalNormals.Clone();
            copy.Specials = (AttackDefinition[])Specials.Clone();
            return copy;
        }

        private static void ValidateNormals(AttackDefinition[] attacks)
        {
            if (attacks == null || attacks.Length < 1 || attacks.Length > 3)
                throw new ArgumentException("通常攻撃列は1〜3段必要です。");
            foreach (var attack in attacks)
            {
                attack.Validate();
                if (!attack.IsNormal) throw new ArgumentException("通常攻撃列に特殊攻撃は登録できません。");
            }
        }
    }

    [Serializable]
    public struct BattleRuleDefinition
    {
        public const int TickRate = 60;
        public const float TickSeconds = 1f / TickRate;
        public int MatchSeconds;
        public int InitialHearts;
        public int NormalHitsPerHeart;
        public int InputBufferFrames;
        public int DoubleTapFrames;
        public int ComboWindowFrames;
        public int MaxComboHits;
        public int MaxHeartsLostPerCombo;
        public int DropThroughFrames;
        public int DropThroughHoldFrames;
        public int GroundJumpStartupFrames;
        public ChargeAttackDefinition Charge;
        public CombatInputDefinition CombatInput;
        public float DropThroughSpeed;
        public float DirectionThreshold;
        public bool EnableRingOut;
        public int SchemaVersion;
        public DefenseDefinition Defense;
        public GaugeDefinition Gauge;
        public StageShrinkDefinition Shrink;
        public BattleVector ComboFinishKnockback;
        public int ComboFinishHitstunFrames;
        public int FollowupImmunityFrames;
        public int HeartRecoveryFrames;
        public BattleVector HeartRecoveryKnockback;
        public int ProjectileLimitPerOwner;
        public int DurationTicks => checked(MatchSeconds * TickRate);
        public void Validate()
        {
            if (MatchSeconds < 1 || InitialHearts < 1 || NormalHitsPerHeart < 1 || InputBufferFrames < 1 ||
                DoubleTapFrames < 1 || ComboWindowFrames < 1 || MaxComboHits < 1 || MaxHeartsLostPerCombo < 1 ||
                DropThroughFrames < 1 || GroundJumpStartupFrames < 0 || DropThroughSpeed <= 0 || DirectionThreshold <= 0 || DirectionThreshold > 1)
                throw new ArgumentException("対戦ルールの定義が不正です。");
            _ = DurationTicks;
            Defense.Validate(); Gauge.Validate(); Charge.Validate(); CombatInput.Validate();
            Shrink.Validate();
            if (ComboFinishHitstunFrames < 1 || FollowupImmunityFrames < 1 || HeartRecoveryFrames < 0 || ProjectileLimitPerOwner < 0 || HeartRecoveryKnockback.X < 0 || HeartRecoveryKnockback.Y < 0)
                throw new ArgumentException("フィニッシュの定義が不正です。");
        }
    }
}
