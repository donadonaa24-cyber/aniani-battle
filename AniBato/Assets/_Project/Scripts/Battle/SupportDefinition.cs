using System;

namespace AniBato.Battle
{
    public enum SupportKind { Alma, ShionCar, SakiHana, Sara, Mai, Tsuyoshi, Mio, Hota }
    [Serializable]
    public struct SupportDefinition
    {
        public bool Enabled;
        public uint Seed;
        public int FirstMinimumFrames,FirstMaximumFrames,IntervalMinimumFrames,IntervalMaximumFrames;
        public int PickupLifetimeFrames,ReuseFrames,EffectFrames,PortalFrames,ActorHoldFrames,ExitFrames;
        public int WalletLifetimeFrames,CarTravelFrames,CarParalysisFrames,ArrowLifetimeFrames;
        public int PawnCount,PawnStaminaDamage,PawnIntervalFrames,PawnLifetimeFrames,BarrierUnits,HealHearts,ProjectileHitstopFrames,ProjectileHitstunFrames;
        public float SlowMultiplier,PickupSize,WalletSize,SpawnMargin,CarWidth,CarHeight,ArrowSpeed,PawnSpeed,PawnSize,PawnSpawnHeight;
        public BattleVector ArrowSize,ArrowKnockback;
        public static SupportDefinition Default=>new SupportDefinition {Enabled=true,Seed=0xA71BA70u,
            FirstMinimumFrames=480,FirstMaximumFrames=720,IntervalMinimumFrames=600,IntervalMaximumFrames=960,
            PickupLifetimeFrames=900,ReuseFrames=600,EffectFrames=300,PortalFrames=45,ActorHoldFrames=90,ExitFrames=24,
            WalletLifetimeFrames=480,CarTravelFrames=180,CarParalysisFrames=120,ArrowLifetimeFrames=180,
            PawnCount=6,PawnStaminaDamage=15,PawnIntervalFrames=16,PawnLifetimeFrames=180,BarrierUnits=3,
            SlowMultiplier=.6f,PickupSize=.7f,WalletSize=.6f,SpawnMargin=1.5f,CarWidth=3.6f,CarHeight=1.45f,
            ArrowSpeed=20,ArrowSize=new BattleVector(1.3f,.28f),ArrowKnockback=new BattleVector(5,4),PawnSpeed=10,PawnSize=.7f,PawnSpawnHeight=10,
            HealHearts=1,ProjectileHitstopFrames=4,ProjectileHitstunFrames=12};
        public void Validate()
        {
            if(!Enabled)return;
            if(FirstMinimumFrames<1 || FirstMaximumFrames<FirstMinimumFrames || IntervalMinimumFrames<1 || IntervalMaximumFrames<IntervalMinimumFrames ||
                PickupLifetimeFrames<1 || ReuseFrames<1 || ReuseFrames>600 || EffectFrames<1 || PortalFrames<1 || ActorHoldFrames<1 || ExitFrames<1 ||
                WalletLifetimeFrames<1 || CarTravelFrames<1 || CarParalysisFrames<1 || ArrowLifetimeFrames<1 || PawnCount<1 || PawnCount>16 ||
                PawnStaminaDamage<1 || PawnIntervalFrames<1 || PawnLifetimeFrames<1 || BarrierUnits<1 || SlowMultiplier<=0 || SlowMultiplier>1 ||
                PickupSize<=0 || WalletSize<=0 || SpawnMargin<0 || CarWidth<=0 || CarHeight<=0 || ArrowSpeed<=0 || ArrowSize.X<=0 || ArrowSize.Y<=0 ||
                PawnSpeed<=0 || PawnSize<=0 || PawnSpawnHeight<=0 || HealHearts<1 || ProjectileHitstopFrames<0 || ProjectileHitstunFrames<0 ||
                ArrowKnockback.X<0 || ArrowKnockback.Y<0)throw new ArgumentException("Invalid support item rules.");
        }
    }
    internal sealed class SupportRandom
    {
        private uint state;
        private readonly SupportKind[] bag=new SupportKind[8];
        private int cursor=8,last=-1;
        public SupportRandom(uint seed){state=seed==0?1:seed;}
        private uint Next(){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
        public int Range(int min,int max)=>min+(int)(Next()%(uint)(max-min+1));
        public float Unit()=>(Next()&0xffffff)/16777216f;
        public SupportKind Draw()
        {
            if(cursor==8)
            {
                for(int i=0;i<8;i++)bag[i]=(SupportKind)i;
                for(int i=7;i>0;i--){int j=Range(0,i);var swap=bag[i];bag[i]=bag[j];bag[j]=swap;}
                if((int)bag[0]==last){var swap=bag[0];bag[0]=bag[1];bag[1]=swap;}cursor=0;
            }
            var kind=bag[cursor++];last=(int)kind;return kind;
        }
    }
}
