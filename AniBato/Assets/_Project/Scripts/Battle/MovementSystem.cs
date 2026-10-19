using System;

namespace AniBato.Battle
{
    public sealed class MovementSystem
    {
        public static void InvalidateGround(FighterState fighter, StageGeometry stage)
        {
            if (!fighter.Grounded) return;
            int index = fighter.GroundPlatform;
            float halfWidth = fighter.Definition.HurtboxSize.X * 0.5f;
            if (!stage.IsActive(index) || fighter.Position.X + halfWidth <= stage[index].Bounds.Left ||
                fighter.Position.X - halfWidth >= stage[index].Bounds.Right)
            {
                fighter.Grounded = false; fighter.GroundPlatform = -1;
                if (fighter.JumpsUsed == 0) fighter.JumpsUsed = 1;
            }
        }
        public void Step(FighterState fighter, in FrameInput input, StageGeometry stage, in BattleRuleDefinition rules, BattleVector externalVelocity = default)
        {
            InvalidateGround(fighter, stage);
            if (fighter.FrozenThisTick || fighter.Mode == FighterMode.KO) return;
            bool canControl = fighter.CanControlMovement;
            bool flowing = rules.CombatInput.FlowingAerialNormal && fighter.Attack != null && fighter.Attack.AirborneAttack &&
                fighter.Attack.Definition.IsNormal && fighter.HitstunRemaining == 0 && fighter.ParalysisRemaining == 0;
            if (flowing) fighter.Velocity = new BattleVector(input.X * fighter.Definition.MoveSpeed * fighter.MoveMultiplier, fighter.Velocity.Y);
            bool droppedThisTick = false;
            if (canControl && fighter.Grounded && fighter.GroundPlatform >= 0 && stage[fighter.GroundPlatform].OneWay && input.Y < -rules.DirectionThreshold &&
                fighter.DownHeldFrames >= rules.DropThroughHoldFrames)
            {
                fighter.DropThroughRemaining = rules.DropThroughFrames;
                fighter.Grounded = false; fighter.GroundPlatform = -1;
                fighter.Velocity = new BattleVector(fighter.Velocity.X, -rules.DropThroughSpeed);
                droppedThisTick = true;
            }
            if ((canControl || flowing) && !droppedThisTick && !fighter.Grounded && input.Y < -rules.DirectionThreshold && fighter.Mode != FighterMode.Dash)
                fighter.FastFalling = true;
            BattleVector oldPosition = fighter.Position;
            float vertical = fighter.Velocity.Y;
            bool recoveryFloat = fighter.Attack != null && fighter.Attack.IsActive && fighter.Attack.Definition.Recovery.IgnoresGravity;
            if (!fighter.Grounded && fighter.Mode != FighterMode.Dash && fighter.Mode != FighterMode.Dodge && !recoveryFloat)
            {
                vertical = Math.Max(vertical - fighter.Definition.Gravity * BattleRuleDefinition.TickSeconds,
                    -(fighter.FastFalling ? fighter.Definition.FastFallSpeed : fighter.Definition.MaxFallSpeed));
                if (fighter.FastFalling) vertical = -fighter.Definition.FastFallSpeed;
            }
            fighter.Velocity = new BattleVector(fighter.Velocity.X, vertical);
            // Persistent fields add displacement through the same swept collisions, not stored momentum.
            vertical += externalVelocity.Y;
            float recoil = fighter.DamageRecoilRemaining > 0 ? fighter.DamageRecoilSpeed : 0;
            if (fighter.DamageRecoilRemaining > 0) fighter.DamageRecoilRemaining--;
            fighter.Position += new BattleVector(fighter.Velocity.X + recoil + externalVelocity.X, vertical) * BattleRuleDefinition.TickSeconds;
            fighter.Grounded = false; fighter.GroundPlatform = -1;

            // Swept AABB against solid platforms; one-way platforms only catch descending feet.
            float halfWidth = fighter.Definition.HurtboxSize.X * 0.5f;
            float halfHeight = fighter.Definition.HurtboxSize.Y * 0.5f;
            float bestTop = float.NegativeInfinity;
            int landing = -1;
            for (int i = 0; i < stage.PlatformCount; i++)
            {
                if (!stage.IsActive(i)) continue;
                StagePlatform platform = stage[i];
                BattleRect bounds = platform.Bounds;
                if (platform.OneWay && fighter.DropThroughRemaining > 0) continue;
                bool horizontalOverlap = fighter.Position.X + halfWidth > bounds.Left && fighter.Position.X - halfWidth < bounds.Right;
                if (horizontalOverlap && vertical <= 0 && oldPosition.Y - halfHeight >= bounds.Top - 0.001f &&
                    fighter.Position.Y - halfHeight <= bounds.Top && bounds.Top > bestTop)
                { bestTop = bounds.Top; landing = i; }
                if (platform.OneWay) continue;
                if (horizontalOverlap && vertical > 0 && oldPosition.Y + halfHeight <= bounds.Bottom && fighter.Position.Y + halfHeight >= bounds.Bottom)
                {
                    fighter.Position = new BattleVector(fighter.Position.X, bounds.Bottom - halfHeight);
                    fighter.Velocity = new BattleVector(fighter.Velocity.X, 0);
                }
                bool verticalOverlap = fighter.Position.Y + halfHeight > bounds.Bottom && fighter.Position.Y - halfHeight < bounds.Top;
                if (!verticalOverlap) continue;
                if (oldPosition.X + halfWidth <= bounds.Left && fighter.Position.X + halfWidth >= bounds.Left)
                    fighter.Position = new BattleVector(bounds.Left - halfWidth, fighter.Position.Y);
                else if (oldPosition.X - halfWidth >= bounds.Right && fighter.Position.X - halfWidth <= bounds.Right)
                    fighter.Position = new BattleVector(bounds.Right + halfWidth, fighter.Position.Y);
            }
            if (landing >= 0)
            {
                bool justLanded = oldPosition.Y - halfHeight > bestTop + .001f;
                fighter.Position = new BattleVector(fighter.Position.X, bestTop + halfHeight);
                fighter.Velocity = new BattleVector(fighter.Velocity.X, 0);
                fighter.Grounded = true; fighter.GroundPlatform = landing;
                fighter.JumpsUsed = 0; fighter.AirDashesUsed = 0; fighter.FastFalling = false;
                if (justLanded && fighter.Attack != null && fighter.Attack.AirborneAttack && fighter.Attack.Definition.LandingRecoveryFrames > 0)
                {
                    if (fighter.Attack.Definition.IsNormal) fighter.Velocity = new BattleVector(0, 0);
                    fighter.ActionFrames = fighter.Attack.Definition.LandingRecoveryFrames; fighter.Attack = null; fighter.Mode = FighterMode.Recovery;
                }
                // An up-special starting from the ground must launch before landing can restore it.
                if (fighter.Attack == null || !fighter.Attack.Definition.IsSpecial || fighter.CurrentSpecial != SpecialSlot.Up)
                    fighter.UpSpecialUsed = false;
            }
            else if (fighter.JumpsUsed == 0) fighter.JumpsUsed = 1; // Walking off a ledge uses the ground jump.
            FighterStateMachine.RefreshLocomotion(fighter);
        }
    }
}
