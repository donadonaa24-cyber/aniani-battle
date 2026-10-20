using System;

namespace AniBato.Battle
{
    public enum RecoveryStyle { None, VerticalLaunch, DiagonalLaunch, DoubleBurst, DirectionalTeleport, Float }
    [Serializable]
    public struct RecoveryDefinition
    {
        public RecoveryStyle Style;
        public float Speed, Distance, HorizontalSpeed;
        public float MaximumChargeDistance;
        public float MotionTimeScale;
        public int MinimumChargeFrames, MaximumChargeFrames;
        public int BurstFrames, SecondStartFrame;
        public bool ChargedTeleport => Style == RecoveryStyle.DirectionalTeleport && MinimumChargeFrames > 0;
        public float DistanceForCharge(int frames) => !ChargedTeleport ? Distance : Distance + (MaximumChargeDistance - Distance) *
            Math.Max(0f, Math.Min(1f, (frames - MinimumChargeFrames) / (float)(MaximumChargeFrames - MinimumChargeFrames)));
        public bool IgnoresGravity => Style == RecoveryStyle.Float || Style == RecoveryStyle.DoubleBurst;
        public void Validate()
        {
            if (Style == RecoveryStyle.None) return;
            if (Speed < 0 || Distance < 0 || HorizontalSpeed < 0 || !Finite(Speed) || !Finite(Distance) || !Finite(HorizontalSpeed) ||
                !Finite(MotionTimeScale) || MotionTimeScale < 0 || MotionTimeScale > 1 ||
                Style == RecoveryStyle.DirectionalTeleport && Distance <= 0 ||
                Style != RecoveryStyle.DirectionalTeleport && Speed <= 0 ||
                MinimumChargeFrames < 0 || MaximumChargeFrames < 0 ||
                MinimumChargeFrames > 0 && (Style != RecoveryStyle.DirectionalTeleport || MaximumChargeFrames <= MinimumChargeFrames ||
                    !Finite(MaximumChargeDistance) || MaximumChargeDistance < Distance) ||
                Style == RecoveryStyle.DoubleBurst && (BurstFrames < 1 || SecondStartFrame <= BurstFrames))
                throw new ArgumentException("復帰の方向・速さ・持続が不正です。");
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
    public static class RecoveryMotionSystem
    {
        public static bool IsCharging(FighterState self) => self.Attack != null && self.Attack.Definition.Recovery.ChargedTeleport &&
            !self.Attack.RecoveryStarted && self.Attack.Frame < self.Attack.Definition.StartupFrames;

        // Called only on unfrozen action ticks. A quick tap still commits to the minimum charge.
        internal static bool Charge(FighterState self, in FrameInput input)
        {
            if (!IsCharging(self)) return false;
            var attack = self.Attack; var recovery = attack.Definition.Recovery;
            attack.RecoveryChargeFrames++;
            attack.RecoveryDirection = Direction(input.X, input.Y, attack.RecoveryDirection);
            self.Velocity = default;
            attack.Frame = Math.Min(attack.RecoveryChargeFrames, attack.Definition.StartupFrames - 1);
            bool waiting = attack.RecoveryChargeFrames < recovery.MinimumChargeFrames ||
                input.IsHeld(InputButtons.Special) && attack.RecoveryChargeFrames < recovery.MaximumChargeFrames;
            // The caller advances to the first active frame on release.
            if (!waiting) attack.Frame = attack.Definition.StartupFrames - 1;
            return waiting;
        }
        public static BattleVector Direction(float x, float y, BattleVector fallback)
        {
            if (Math.Abs(x) + Math.Abs(y) < .2f) return fallback;
            float length = (float)Math.Sqrt(x * x + y * y);
            return new BattleVector(x / length, y / length);
        }
        public static void Step(FighterState self, in FrameInput input, StageGeometry stage)
        {
            var attack = self.Attack;
            if (self.IsFrozenThisTick || attack == null || !attack.IsActive) return;
            var recovery = attack.Definition.Recovery;
            if (recovery.Style == RecoveryStyle.None) return;
            int age = attack.Frame - attack.Definition.StartupFrames;
            if (!attack.RecoveryStarted)
            {
                attack.RecoveryStarted = true;
                if (recovery.Style == RecoveryStyle.DirectionalTeleport)
                {
                    var direction = Direction(input.X, input.Y, attack.RecoveryDirection);
                    attack.RecoveryOrigin = self.Position;
                    self.Position = Destination(self, direction * recovery.DistanceForCharge(attack.RecoveryChargeFrames), stage);
                    attack.RecoveryDestination = self.Position; self.Velocity = default;
                }
                else if (recovery.Style == RecoveryStyle.VerticalLaunch)
                    self.Velocity = new BattleVector(0, recovery.Speed);
                else if (recovery.Style == RecoveryStyle.DiagonalLaunch)
                {
                    float sign = attack.RecoveryDirection.X == 0 ? self.Facing : Math.Sign(attack.RecoveryDirection.X);
                    self.Velocity = new BattleVector(sign * recovery.HorizontalSpeed, recovery.Speed);
                }
                self.Grounded = false; self.GroundPlatform = -1; self.FastFalling = false;
            }
            if (recovery.Style == RecoveryStyle.Float)
            {
                var direction = Direction(input.X, input.Y, new BattleVector(0, 1));
                self.Velocity = new BattleVector(direction.X * recovery.HorizontalSpeed, direction.Y * recovery.Speed);
            }
            if (recovery.Style == RecoveryStyle.DoubleBurst)
            {
                if (age == recovery.SecondStartFrame)
                    attack.RecoveryDirection = Direction(input.X, input.Y, attack.RecoveryDirection);
                bool moving = age < recovery.BurstFrames || age >= recovery.SecondStartFrame && age < recovery.SecondStartFrame + recovery.BurstFrames;
                self.Velocity = moving ? attack.RecoveryDirection * recovery.Speed : default;
            }
        }
        // A teleport crosses intervening space; only its destination must fit the whole body.
        // If occupied, choose the furthest free point on the requested ray. Never clamp to a blast line.
        private static BattleVector Destination(FighterState self, BattleVector delta, StageGeometry stage)
        {
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y) / .2));
            for (int step = steps; step >= 1; step--)
            {
                var position = self.Position + delta * (step / (float)steps);
                var body = new BattleRect(position, self.Definition.HurtboxSize);
                bool blocked = false;
                for (int i = 0; i < stage.CollisionPlatformCount; i++)
                    if (stage.IsActive(i) && !stage[i].OneWay && body.Overlaps(stage[i].Bounds)) { blocked = true; break; }
                if (!blocked) return position;
            }
            return self.Position;
        }
    }
}
