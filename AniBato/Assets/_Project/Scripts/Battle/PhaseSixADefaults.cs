namespace AniBato.Battle
{
    public static class PhaseSixADefaults
    {
        public static BattleRuleDefinition Rules()
        { var rules = PhaseFiveBDefaults.Rules(); rules.SchemaVersion = 6; rules.DropThroughHoldFrames = 18; return rules; }
        public static CharacterDefinition Character(int index)
        { var c = PhaseFiveBDefaults.Character(index); AddFeatures(ref c, index); return c; }
        public static void AddFeatures(ref CharacterDefinition c, int index)
        {
            c.Stamina = new StaminaDefinition { Maximum = index == 0 ? 100 : index == 1 ? 85 : 125,
                GroundRecoveryPerSecond = index == 0 ? 32 : index == 1 ? 26 : 22, ActiveRecoveryPerSecond = 6, RecoveryDelayFrames = 30,
                JumpCost = index == 1 ? 6 : 8, AirJumpCost = index == 1 ? 9 : 12,
                GroundDashCost = index == 1 ? 6 : 8, AirDashCost = index == 1 ? 11 : 14, DodgeCost = index == 1 ? 14 : 18,
                GuardDrainPerSecond = 6, NormalGuardCost = 8, SpecialGuardCost = 20, UltimateGuardCost = 65,
                GuardBreakFrames = 18, GuardLockFrames = 36 };
            c.CrouchHeightRatio = .55f; c.DiagonalThreshold = .45f; c.DiagonalFallbackVertical = true;
            c.DirectionalNormals = new AttackDefinition[10];
            for (int slot = 1; slot < 10; slot++)
            {
                var dir = (NormalDirection)slot; var a = c.GroundNormals[0];
                a.Name = c.Name + " " + Names[slot]; a.NormalDirection = dir; a.IsCloseRange = true;
                a.StartupFrames = (index == 0 ? 3 : index == 1 ? 4 : 5) + (dir == NormalDirection.Back || dir == NormalDirection.AirBack ? 3 : 0);
                a.ActiveFrames = 4; a.RecoveryFrames = (index == 0 ? 9 : index == 1 ? 11 : 13);
                float reach = index == 0 ? 1.7f : index == 1 ? 2.3f : 2.6f;
                float x = reach * .65f, y = 0, w = reach, h = index == 2 ? 2.1f : 1.7f;
                if (dir == NormalDirection.Crouch) { x = .85f; y = -.85f; w = reach * .8f; h = .8f; a.BreaksGuard = true; }
                if (dir == NormalDirection.Back || dir == NormalDirection.AirBack) x = -x;
                if (dir == NormalDirection.AirNeutral) { x = 0; w = reach + .8f; }
                if (dir == NormalDirection.AirUp) { x = 0; y = 1.2f; h = 2.2f; }
                if (dir == NormalDirection.AirDown) { x = .2f; y = -1.1f; h = 2; a.LandingRecoveryFrames = 18; a.RecoveryFrames += 8; }
                if (dir == NormalDirection.AirForwardUp) y = .9f;
                if (dir == NormalDirection.AirForwardDown) { y = -.8f; a.LandingRecoveryFrames = 14; }
                if (dir == NormalDirection.AirForward || dir == NormalDirection.AirForwardUp || dir == NormalDirection.AirForwardDown)
                {
                    a.ActiveVelocity = new BattleVector(index == 1 ? 13 : index == 0 ? 9 : 7, dir == NormalDirection.AirForwardUp ? 9 : dir == NormalDirection.AirForwardDown ? -6 : 0);
                    a.VerticalMotionOnStartOnly = true;
                }
                a.Hitbox = new BattleRect(new BattleVector(x, y), new BattleVector(w, h));
                // Downward moves do not meteor: preserve a modest upward knockback to avoid cheap ring-outs.
                a.Knockback = new BattleVector(index == 1 ? 3 : 2, 2);
                c.DirectionalNormals[slot] = a;
            }
        }
        public static readonly string[] Names = { "弱", "足払い", "背面打ち", "空中ニュートラル", "空中前", "空中後ろ", "空中上", "空中下", "空中斜め前上", "空中斜め前下" };
    }
}
