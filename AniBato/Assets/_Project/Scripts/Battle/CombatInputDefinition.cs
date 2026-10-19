using System;

namespace AniBato.Battle
{
    [Serializable]
    public struct CombatInputDefinition
    {
        public bool SingleAerialNormal, FlowingAerialNormal, SingleSpecial, NormalClashes;
        public bool DownSpecialEnabled, UpRecoveryEnabled;
        public SpecialSlot SpecialDirection(BattleVector direction, float threshold) =>
            UpRecoveryEnabled && direction.Y > threshold ? SpecialSlot.Up :
            DownSpecialEnabled && direction.Y < -threshold ? SpecialSlot.Down :
            SingleSpecial ? SpecialSlot.Neutral : direction.Y > threshold ? SpecialSlot.Up :
            direction.Y < -threshold ? SpecialSlot.Down : Math.Abs(direction.X) > threshold ? SpecialSlot.Side : SpecialSlot.Neutral;
        public int RequiredSpecialMask => SingleSpecial ? 1 | (DownSpecialEnabled ? 8 : 0) | (UpRecoveryEnabled ? 4 : 0) : 15;
        public int ClashStopFrames, ClashRecoveryFrames;
        public static CombatInputDefinition Default => new CombatInputDefinition
        { SingleAerialNormal = true, FlowingAerialNormal = true, SingleSpecial = true, NormalClashes = true, ClashStopFrames = 5, ClashRecoveryFrames = 10 };
        public void Validate()
        {
            if (NormalClashes && (ClashStopFrames < 1 || ClashRecoveryFrames < 1)) throw new ArgumentException("相殺の停止・硬直時間が不正です。");
        }
    }
}
