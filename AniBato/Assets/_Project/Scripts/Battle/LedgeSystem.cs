using System;

namespace AniBato.Battle
{
    [Serializable]
    public struct LedgeDefinition
    {
        public bool Enabled;
        public int MaximumHoldFrames, StaminaPerSecond, ClimbFrames, JumpCarryFrames;
        public float CatchReach, CatchAbove, CatchBelow, BodyGap, HangHeadBelow, JumpInwardMultiplier, JumpVerticalMultiplier;
        public static LedgeDefinition Default => new LedgeDefinition { Enabled=true, MaximumHoldFrames=300,
            StaminaPerSecond=25, ClimbFrames=18, JumpCarryFrames=24, CatchReach=.65f, CatchAbove=.3f,
            CatchBelow=.65f, BodyGap=.08f, HangHeadBelow=.15f, JumpInwardMultiplier=.7f, JumpVerticalMultiplier=1.1f };
        public void Validate()
        {
            if (!Enabled) return;
            if(MaximumHoldFrames<1 || StaminaPerSecond<1 || ClimbFrames<2 || JumpCarryFrames<1 ||
                CatchReach<=0 || CatchAbove<0 || CatchBelow<=0 || BodyGap<0 || HangHeadBelow<0 || JumpInwardMultiplier<=0 || JumpVerticalMultiplier<=0)
                throw new ArgumentException("Invalid ledge recovery definition.");
        }
    }
    // The main stage is the only grabbable surface. Paper artwork never determines collision.
    public static class LedgeSystem
    {
        public static bool Attached(FighterState f) => f.Mode==FighterMode.LedgeHang || f.Mode==FighterMode.LedgeClimb;
        public static bool CanHit(FighterState target, in AttackDefinition attack) => target.Mode!=FighterMode.LedgeHang || attack.CanHitLedge;
        public static int HoldFramesLeft(FighterState f,in LedgeDefinition d)
        {
            int remaining=Math.Max(0,d.MaximumHoldFrames-f.LedgeAge);
            return !f.Definition.Stamina.Enabled || SupportStatus.Infinite(f)?remaining:Math.Min(remaining,(f.StaminaUnits+d.StaminaPerSecond-1)/d.StaminaPerSecond);
        }
        public static void Interrupt(FighterState f)
        {
            if(f.LedgeSide!=0) f.LedgeRegrabBlocked=true;
            f.LedgeSide=0; f.LedgeAge=0; f.LedgeJumpCarryRemaining=0;
        }
        private static BattleVector Point(StageGeometry stage,int side) =>
            new BattleVector(side<0?stage[0].Bounds.Left:stage[0].Bounds.Right,stage[0].Bounds.Top);
        private static BattleVector HangPosition(FighterState f, BattleVector point, in LedgeDefinition d) =>
            new BattleVector(point.X+f.LedgeSide*(f.Definition.HurtboxSize.X*.5f+d.BodyGap),
                point.Y-f.Definition.HurtboxSize.Y*.5f-d.HangHeadBelow);
        private static void Drop(FighterState f)
        {
            Interrupt(f); f.Mode=FighterMode.Fall; f.Velocity=new BattleVector(0,-1);
            f.Grounded=false; f.GroundPlatform=-1; f.InputBuffer.ClearActions();
        }
        public static bool StepAttached(FighterState f,in FrameInput input, StageGeometry stage,in BattleRuleDefinition rules)
        {
            if(!Attached(f)) return false;
            f.InputBuffer.ClearActions(); f.Velocity=default;
            var d=rules.Ledge;
            if(!d.Enabled || stage==null || !stage.IsActive(0) || stage.IsOutside(f.Position)) { Drop(f); return true; }
            var point=Point(stage,f.LedgeSide);
            if(Math.Abs(point.X-f.LedgePoint.X)>d.CatchReach || Math.Abs(point.Y-f.LedgePoint.Y)>d.CatchBelow)
            { Drop(f); return true; }
            f.LedgePoint=point;
            if(f.Mode==FighterMode.LedgeClimb)
            {
                float t=++f.LedgeAge/(float)d.ClimbFrames;
                float y=Math.Min(1,t*2),x=Math.Max(0,(t-.5f)*2);
                var end=new BattleVector(point.X-f.LedgeSide*(f.Definition.HurtboxSize.X*.5f+d.BodyGap),
                    point.Y+f.Definition.HurtboxSize.Y*.5f+.001f);
                f.Position=new BattleVector(f.LedgeStart.X+(end.X-f.LedgeStart.X)*x,
                    f.LedgeStart.Y+(end.Y-f.LedgeStart.Y)*y);
                if(t>=1)
                {
                    f.Mode=FighterMode.Idle; f.Grounded=true; f.GroundPlatform=0;
                    f.JumpsUsed=f.AirDashesUsed=0; f.UpSpecialUsed=false; f.LedgeRegrabBlocked=false;
                    f.LedgeSide=f.LedgeAge=0;
                }
                return true;
            }
            f.Position=HangPosition(f,point,d);
            if(input.Y < -rules.DirectionThreshold) { Drop(f); return true; }
            if(input.IsPressed(InputButtons.Jump))
            {
                // A ledge jump is an exit even if the previous air jumps have been spent.
                StaminaSystem.Spend(f,f.Definition.Stamina.JumpCost,true);
                f.LedgeJumpDirection=-f.LedgeSide; f.LedgeJumpCarryRemaining=d.JumpCarryFrames;
                f.LedgeRegrabBlocked=true; f.LedgeSide=f.LedgeAge=0;
                f.Mode=FighterMode.Jump; f.FastFalling=false;
                f.Velocity=new BattleVector(f.LedgeJumpDirection*f.Definition.MoveSpeed*d.JumpInwardMultiplier,f.Definition.JumpSpeed*d.JumpVerticalMultiplier);
                return true;
            }
            if(input.X*f.LedgeSide < -rules.DirectionThreshold)
            { f.Mode=FighterMode.LedgeClimb; f.LedgeStart=f.Position; f.LedgeAge=0; return true; }
            if(f.Definition.Stamina.Enabled && !SupportStatus.Infinite(f))
            {
                f.StaminaUnits=Math.Max(0,f.StaminaUnits-d.StaminaPerSecond);
                f.StaminaDelayRemaining=f.Definition.Stamina.RecoveryDelayFrames;
            }
            if(++f.LedgeAge>=d.MaximumHoldFrames || f.Definition.Stamina.Enabled && !SupportStatus.Infinite(f) && f.StaminaUnits==0) Drop(f);
            return true;
        }
        public static void JumpCarry(FighterState f,in FrameInput input,in LedgeDefinition d)
        {
            if(f.LedgeJumpCarryRemaining<=0 || f.IsFrozenThisTick) return;
            if(f.Grounded || f.HitstunRemaining>0 || f.Attack!=null || input.X*f.LedgeJumpDirection<-.1f)
            { f.LedgeJumpCarryRemaining=0; return; }
            f.LedgeJumpCarryRemaining--;
            if(f.CanControlMovement && Math.Abs(input.X)<.1f)
                f.Velocity=new BattleVector(f.LedgeJumpDirection*f.Definition.MoveSpeed*d.JumpInwardMultiplier,f.Velocity.Y);
        }
        public static void TryCatch(FighterState f,in FrameInput input, StageGeometry stage,in BattleRuleDefinition rules)
        {
            var d=rules.Ledge;
            if(f.Grounded) { f.LedgeRegrabBlocked=false; f.LedgeJumpCarryRemaining=0; return; }
            if(!d.Enabled || !stage.IsActive(0) || stage.IsOutside(f.Position) || f.LedgeRegrabBlocked || Attached(f) ||
                f.IsFrozenThisTick || f.Mode==FighterMode.KO || f.HitstunRemaining>0 || f.ParalysisRemaining>0 ||
                f.Mode==FighterMode.Dodge || f.Mode==FighterMode.Counter || f.Mode==FighterMode.Guard ||
                input.Y < -rules.DirectionThreshold || f.Definition.Stamina.Enabled && !SupportStatus.Infinite(f) && f.StaminaUnits==0 ||
                f.Attack!=null && f.Attack.Definition.Recovery.Style==RecoveryStyle.None) return;
            var floor=stage[0].Bounds; float half=f.Definition.HurtboxSize.X*.5f;
            int side=f.Position.X<=floor.Left?-1:f.Position.X>=floor.Right?1:0;
            if(side==0) return;
            var point=Point(stage,side); float distance=Math.Abs(f.Position.X-point.X)-half;
            float head=f.Position.Y+f.Definition.HurtboxSize.Y*.5f;
            if(distance>d.CatchReach || distance<-.001f || head<point.Y-d.CatchBelow || head>point.Y+d.CatchAbove) return;
            f.LedgeSide=side; f.LedgePoint=point; f.LedgeAge=0; f.Facing=-side;
            f.Mode=FighterMode.LedgeHang; f.Position=HangPosition(f,point,d); f.Velocity=default;
            f.Attack=null; f.ChargeFrames=0; f.ActionFrames=0; f.DashRemaining=0; f.FastFalling=false;
            f.MaximumChargeLaunch=false;
            f.DamageRecoilRemaining=0; f.InputBuffer.ClearActions();
        }
    }
}
