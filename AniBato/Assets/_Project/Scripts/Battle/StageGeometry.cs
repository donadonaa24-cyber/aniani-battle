using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    [Serializable]
    public struct StagePlatform
    {
        public BattleRect Bounds;
        public bool OneWay;
        public StagePlatform(BattleRect bounds, bool oneWay) { Bounds = bounds; OneWay = oneWay; }
    }

    public sealed class StageGeometry
    {
        private readonly StagePlatform[] platforms;
        private readonly bool[] active;
        private readonly List<StagePlatform> extra = new List<StagePlatform>();
        private readonly List<bool> extraActive = new List<bool>();
        public int PlatformCount => platforms.Length;
        public int CollisionPlatformCount => platforms.Length + extra.Count;
        public StagePlatform this[int index] => index < platforms.Length ? platforms[index] : extra[index-platforms.Length];
        public float LeftBlastLine { get; }
        public float RightBlastLine { get; }
        public float BottomBlastLine { get; }
        public BattleVector SpawnP1 { get; }
        public BattleVector SpawnP2 { get; }

        public StageGeometry(StagePlatform[] platforms, float left, float right, float bottom,
            BattleVector spawnP1, BattleVector spawnP2)
        {
            if (platforms == null || platforms.Length == 0 || left >= right)
                throw new ArgumentException("ステージ地形が不正です。");
            foreach (var platform in platforms)
                if (platform.Bounds.Size.X <= 0 || platform.Bounds.Size.Y <= 0)
                    throw new ArgumentException("足場サイズが不正です。");
            this.platforms = (StagePlatform[])platforms.Clone();
            active = new bool[platforms.Length];
            for (int i = 0; i < active.Length; i++) active[i] = true;
            LeftBlastLine = left; RightBlastLine = right; BottomBlastLine = bottom;
            SpawnP1 = spawnP1; SpawnP2 = spawnP2;
        }

        public bool IsOutside(BattleVector position) => position.X < LeftBlastLine ||
            position.X > RightBlastLine || position.Y < BottomBlastLine;
        public bool IsActive(int index) => index >= 0 && (index < active.Length ? active[index] : index < CollisionPlatformCount && extraActive[index-platforms.Length]);
        internal int AddSolid(BattleRect bounds)
        { extra.Add(new StagePlatform(bounds,false)); extraActive.Add(true); return CollisionPlatformCount-1; }
        internal void SetPlatform(int index, StagePlatform platform) => platforms[index] = platform;
        internal void DeactivatePlatform(int index) { if(index<active.Length)active[index]=false;else extraActive[index-platforms.Length]=false; }
        public StageGeometry Snapshot()
        {
            var copy = new StageGeometry(platforms, LeftBlastLine, RightBlastLine, BottomBlastLine, SpawnP1, SpawnP2);
            Array.Copy(active, copy.active, active.Length); copy.extra.AddRange(extra);copy.extraActive.AddRange(extraActive); return copy;
        }
    }
}
