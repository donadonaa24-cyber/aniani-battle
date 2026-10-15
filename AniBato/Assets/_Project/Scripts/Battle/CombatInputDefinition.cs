using System;

namespace AniBato.Battle
{
    [Serializable]
    public struct CombatInputDefinition
    {
        public bool SingleAerialNormal, FlowingAerialNormal, SingleSpecial, NormalClashes;
        public int ClashStopFrames, ClashRecoveryFrames;
        public static CombatInputDefinition Default => new CombatInputDefinition
        { SingleAerialNormal = true, FlowingAerialNormal = true, SingleSpecial = true, NormalClashes = true, ClashStopFrames = 5, ClashRecoveryFrames = 10 };
        public void Validate()
        {
            if (NormalClashes && (ClashStopFrames < 1 || ClashRecoveryFrames < 1)) throw new ArgumentException("相殺の停止・硬直時間が不正です。");
        }
    }
}
