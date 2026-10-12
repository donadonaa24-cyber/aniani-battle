using System;

namespace AniBato.Battle
{
    [Serializable]
    public struct BattleVector
    {
        public float X;
        public float Y;
        public BattleVector(float x, float y) { X = x; Y = y; }
        public static BattleVector operator +(BattleVector a, BattleVector b) => new BattleVector(a.X + b.X, a.Y + b.Y);
        public static BattleVector operator *(BattleVector a, float scale) => new BattleVector(a.X * scale, a.Y * scale);
    }

    [Serializable]
    public struct BattleRect
    {
        public BattleVector Center;
        public BattleVector Size;
        public BattleRect(BattleVector center, BattleVector size) { Center = center; Size = size; }
        public float Left => Center.X - Size.X * 0.5f;
        public float Right => Center.X + Size.X * 0.5f;
        public float Bottom => Center.Y - Size.Y * 0.5f;
        public float Top => Center.Y + Size.Y * 0.5f;
        public bool Overlaps(in BattleRect other) => Left < other.Right && Right > other.Left && Bottom < other.Top && Top > other.Bottom;
        public BattleRect At(BattleVector position, int facing) => new BattleRect(
            new BattleVector(position.X + Center.X * facing, position.Y + Center.Y), Size);
    }
}
