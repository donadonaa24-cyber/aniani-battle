namespace AniBato.Battle
{
    /// <summary>
    /// Reach adjustment rule (2026-10-15 user request: normal attacks reached too far to fight up close).
    /// Halves a hitbox's horizontal width while keeping the edge nearest the fighter, so only the part that
    /// reaches toward the opponent shrinks. Boxes that extend well behind the body on both sides
    /// (air up/down/neutral) shrink around their own center so the front is not lost.
    /// Rects are in facing-forward local space (+X = toward the opponent when facing right).
    /// </summary>
    public static class HitboxReach
    {
        public const float CenteredBackReach = 0.3f;

        public static BattleRect HalveTowardOpponent(in BattleRect rect)
        {
            float halfWidth = rect.Size.X * 0.5f;
            bool behind = rect.Right <= 0f;
            bool centered = !behind && rect.Left < -CenteredBackReach;
            float centerX = centered ? rect.Center.X
                : behind ? rect.Right - halfWidth * 0.5f
                : rect.Left + halfWidth * 0.5f;
            return new BattleRect(new BattleVector(centerX, rect.Center.Y), new BattleVector(halfWidth, rect.Size.Y));
        }
    }
}
