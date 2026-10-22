using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    public static class SupportStatus
    {
        public static bool Infinite(FighterState f)=>f.SupportInfiniteRemaining>0;
        public static void Step(FighterState f)
        {
            if(f.SupportReuseRemaining>0)f.SupportReuseRemaining--;
            if(f.SupportSlowRemaining>0)f.SupportSlowRemaining--;
            if(f.SupportInfiniteRemaining>0)f.SupportInfiniteRemaining--;
            if(f.SupportBarrierRemaining>0 && --f.SupportBarrierRemaining==0)f.SupportBarrierUnits=0;
        }
        public static bool Absorb(in HitContact contact,int tick,in BattleRuleDefinition rules,List<BattleEvent> events)
        {
            var f=contact.Target;var a=contact.Attack.Definition;
            if(f.SupportBarrierRemaining==0 || f.SupportBarrierUnits==0 || a.IsUltimate || a.DamageMode==AttackDamageMode.StatusOnly)return false;
            int cost=a.IsNormal || a.DamageMode==AttackDamageMode.NormalAccumulation?1:rules.Support.BarrierUnits;
            f.SupportBarrierUnits=Math.Max(0,f.SupportBarrierUnits-cost);
            if(f.SupportBarrierUnits==0)f.SupportBarrierRemaining=0;
            f.HitstopRemaining=Math.Max(f.HitstopRemaining,a.HitstopFrames);
            contact.Attacker.HitstopRemaining=Math.Max(contact.Attacker.HitstopRemaining,a.HitstopFrames);
            events.Add(new BattleEvent(BattleEventKind.BarrierAbsorbed,tick,contact.Attacker.PlayerIndex,f.PlayerIndex,contact.Attack.Id,f.SupportBarrierUnits,a.Kind));
            return true;
        }
    }
    public sealed class SupportPickup
    {
        public long Id {get;}
        public SupportKind Kind {get;}
        public bool IsWallet {get;}
        public BattleVector Position {get;internal set;}
        public int RemainingFrames {get;internal set;}
        internal SupportPickup(long id,SupportKind kind,BattleVector position,int lifetime,bool wallet=false)
        {Id=id;Kind=kind;Position=position;RemainingFrames=lifetime;IsWallet=wallet;}
    }
    public sealed class SupportActor
    {
        public long Id {get;}
        public SupportKind Kind {get;}
        public FighterState Owner {get;}
        public BattleVector Position {get;}
        public int Facing {get;}
        public int AttackFacing {get;internal set;}
        public int Age {get;internal set;}
        internal int PawnsReleased;
        internal bool Applied;
        internal SupportActor(long id,SupportKind kind,FighterState owner,BattleVector position,int facing)
        {Id=id;Kind=kind;Owner=owner;Position=position;Facing=facing;}
    }
}
