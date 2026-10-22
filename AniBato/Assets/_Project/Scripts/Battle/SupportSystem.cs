using System;
using System.Collections.Generic;

namespace AniBato.Battle
{
    public sealed class SupportSystem
    {
        private readonly SupportDefinition rules;
        private readonly SupportRandom random;
        private readonly List<SupportActor> actors=new List<SupportActor>();
        public IReadOnlyList<SupportActor> Actors {get;}
        public SupportPickup Pickup {get;private set;}
        public SupportPickup Wallet {get;private set;}
        public int SpawnCountdown {get;private set;}
        private long nextId=1,nextAttack=1L<<60;
        private int tiePriority;
        public SupportSystem(in SupportDefinition definition)
        {
            definition.Validate();rules=definition;random=new SupportRandom(definition.Seed);Actors=actors.AsReadOnly();
            if(rules.Enabled)SpawnCountdown=random.Range(rules.FirstMinimumFrames,rules.FirstMaximumFrames);
        }
        private float RandomX(StageGeometry stage,float radius)
        {
            var floor=stage[0].Bounds;float inset=Math.Min(floor.Size.X*.45f,Math.Max(radius,rules.SpawnMargin));
            return floor.Left+inset+random.Unit()*Math.Max(0,floor.Size.X-2*inset);
        }
        private bool Position(StageGeometry stage,float size,out BattleVector position,float? preferred=null)
        {
            var floor=stage[0].Bounds;float half=size*.5f;
            for(int attempt=0;attempt<65;attempt++)
            {
                float x=attempt==0&&preferred.HasValue?preferred.Value:attempt<17?RandomX(stage,half):
                    floor.Left+half+(floor.Size.X-size)*(attempt-17)/47f;
                x=Math.Max(floor.Left+half,Math.Min(floor.Right-half,x));
                position=new BattleVector(x,floor.Top+half);
                var body=new BattleRect(position,new BattleVector(size,size));bool clear=true;
                for(int i=1;i<stage.CollisionPlatformCount;i++)
                    if(stage.IsActive(i)&&!stage[i].OneWay&&body.Overlaps(stage[i].Bounds)){clear=false;break;}
                if(clear)return true;
            }
            position=default;return false;
        }
        private static bool Ready(FighterState f)=>f.Mode!=FighterMode.KO && f.SupportReuseRemaining==0;
        private static bool Touch(SupportPickup pickup,FighterState f,float size)=>
            f.Hurtbox.Overlaps(new BattleRect(pickup.Position,new BattleVector(size,size)));
        private FighterState Collector(SupportPickup pickup,FighterState p1,FighterState p2,float size,bool reuse)
        {
            bool a=(reuse?Ready(p1):p1.Mode!=FighterMode.KO)&&Touch(pickup,p1,size);
            bool b=(reuse?Ready(p2):p2.Mode!=FighterMode.KO)&&Touch(pickup,p2,size);
            if(a&&b){var chosen=tiePriority==0?p1:p2;tiePriority=1-tiePriority;return chosen;}
            return a?p1:b?p2:null;
        }
        public void Step(FighterState p1,FighterState p2,StageGeometry stage,in BattleRuleDefinition battle,int tick,
            ProjectileSystem projectiles,List<BattleEvent> events)
        {
            if(!rules.Enabled || p1.FrozenThisTick || p2.FrozenThisTick)return;
            SupportStatus.Step(p1);SupportStatus.Step(p2);
            if(SpawnCountdown>0)SpawnCountdown--;
            if(Pickup!=null)
            {
                bool placed=Reposition(Pickup,stage,rules.PickupSize);
                if(--Pickup.RemainingFrames<=0 || !stage.IsActive(0) || !placed)Pickup=null;
                else
                {
                    var f=Collector(Pickup,p1,p2,rules.PickupSize,true);
                    if(f!=null)
                    {
                        Activate(Pickup.Kind,f,f==p1?p2:p1,stage,battle,tick,events);Pickup=null;
                        SpawnCountdown=random.Range(rules.IntervalMinimumFrames,rules.IntervalMaximumFrames);
                    }
                }
            }
            if(Pickup==null && SpawnCountdown==0 && stage.IsActive(0) && Position(stage,rules.PickupSize,out var location))
            {
                Pickup=new SupportPickup(nextId++,random.Draw(),location,rules.PickupLifetimeFrames);
                SpawnCountdown=random.Range(rules.IntervalMinimumFrames,rules.IntervalMaximumFrames);
            }
            if(Wallet!=null)
            {
                bool placed=Reposition(Wallet,stage,rules.WalletSize);
                if(--Wallet.RemainingFrames<=0 || !stage.IsActive(0) || !placed)Wallet=null;
                else
                {
                    var f=Collector(Wallet,p1,p2,rules.WalletSize,false);
                    if(f!=null)
                    {
                        f.StaminaUnits=f.Definition.Stamina.Maximum*StaminaSystem.UnitsPerPoint;
                        f.StaminaDelayRemaining=f.StaminaRejectedRemaining=0;Wallet=null;
                        events.Add(new BattleEvent(BattleEventKind.SupportWalletCollected,tick,f.PlayerIndex,f.PlayerIndex,value:(int)SupportKind.Tsuyoshi));
                    }
                }
            }
            for(int i=actors.Count-1;i>=0;i--)
            {
                var actor=actors[i];var target=actor.Owner==p1?p2:p1;actor.Age++;
                if(actor.Age>=rules.PortalFrames && !actor.Applied)
                {
                    actor.Applied=true;
                    if(actor.Kind==SupportKind.ShionCar)
                    {
                        var floor=stage[0].Bounds;
                        var position=new BattleVector(actor.Facing>0?floor.Left-rules.CarWidth*.5f:floor.Right+rules.CarWidth*.5f,
                            floor.Top+rules.CarHeight*.5f);
                        var a=Attack(actor.Kind,new BattleVector(rules.CarWidth,rules.CarHeight));a.DamageMode=AttackDamageMode.StatusOnly;
                        a.ParalysisFrames=rules.CarParalysisFrames;a.HitstunFrames=rules.CarParalysisFrames;
                        a.ProjectileSpeed=(floor.Size.X+rules.CarWidth)/ (rules.CarTravelFrames/(float)BattleRuleDefinition.TickRate);
                        a.ProjectileLifetimeFrames=rules.CarTravelFrames;Spawn(projectiles,actor,target,stage,a,position,actor.Facing);
                    }
                    if(actor.Kind==SupportKind.Mai)
                    {
                        var a=Attack(actor.Kind,rules.ArrowSize);a.ProjectileSpeed=rules.ArrowSpeed;a.ProjectileLifetimeFrames=rules.ArrowLifetimeFrames;
                        a.Knockback=rules.ArrowKnockback;
                        int direction=target.Position.X>=actor.Position.X?1:-1;
                        actor.AttackFacing=direction;
                        Spawn(projectiles,actor,target,stage,a,actor.Position+new BattleVector(direction*.6f,1.3f),direction);
                    }
                    if(actor.Kind==SupportKind.Tsuyoshi && Position(stage,rules.WalletSize,out var walletPosition))
                        Wallet=new SupportPickup(nextId++,SupportKind.Tsuyoshi,walletPosition,rules.WalletLifetimeFrames,true);
                }
                if(actor.Kind==SupportKind.Hota && actor.Age>=rules.PortalFrames && actor.PawnsReleased<rules.PawnCount &&
                    actor.Age>=rules.PortalFrames+actor.PawnsReleased*rules.PawnIntervalFrames)
                {
                    var a=Attack(actor.Kind,new BattleVector(rules.PawnSize,rules.PawnSize));a.DamageMode=AttackDamageMode.StatusOnly;
                    a.ProjectileSpeed=0;a.Ability.AllowZeroProjectileSpeed=true;a.ProjectileLaunchVelocity=new BattleVector(0,-rules.PawnSpeed);
                    a.StaminaDamage=rules.PawnStaminaDamage;a.CanHitLedge=true;a.HitstopFrames=0;a.HitstunFrames=0;
                    a.ProjectileLifetimeFrames=rules.PawnLifetimeFrames;
                    Spawn(projectiles,actor,target,stage,a,new BattleVector(RandomX(stage,rules.PawnSize*.5f),stage[0].Bounds.Top+rules.PawnSpawnHeight),actor.Facing);
                    actor.PawnsReleased++;
                }
                int lifetime=rules.PortalFrames+rules.ActorHoldFrames+rules.ExitFrames;
                if(actor.Age>=lifetime || actor.Owner.Mode==FighterMode.KO)actors.RemoveAt(i);
            }
        }
        private bool Reposition(SupportPickup pickup,StageGeometry stage,float size)
        {
            bool found=Position(stage,size,out var point,pickup.Position.X);if(found)pickup.Position=point;return found;
        }
        private void Activate(SupportKind kind,FighterState owner,FighterState enemy,StageGeometry stage,in BattleRuleDefinition battle,int tick,List<BattleEvent> events)
        {
            owner.SupportReuseRemaining=rules.ReuseFrames;
            var floor=stage[0].Bounds;
            float x=Math.Max(floor.Left+1,Math.Min(floor.Right-1,owner.Position.X-owner.Facing*1.7f));
            if(kind==SupportKind.ShionCar)x=owner.Facing>0?floor.Left-rules.CarWidth*.5f:floor.Right+rules.CarWidth*.5f;
            actors.Add(new SupportActor(nextId++,kind,owner,new BattleVector(x,floor.Top),owner.Facing));
            switch(kind)
            {
                case SupportKind.Alma:enemy.SupportSlowRemaining=rules.EffectFrames;enemy.SupportSlowMultiplier=rules.SlowMultiplier;break;
                case SupportKind.SakiHana:owner.SupportInfiniteRemaining=rules.EffectFrames;break;
                case SupportKind.Sara:owner.Hearts=Math.Min(battle.InitialHearts,owner.Hearts+rules.HealHearts);break;
                case SupportKind.Mio:owner.SupportBarrierRemaining=rules.EffectFrames;owner.SupportBarrierUnits=rules.BarrierUnits;break;
            }
            events.Add(new BattleEvent(BattleEventKind.SupportCollected,tick,owner.PlayerIndex,owner.PlayerIndex,value:(int)kind));
        }
        private AttackDefinition Attack(SupportKind kind,BattleVector size)=>new AttackDefinition {Name="Support "+kind,
            Kind=AttackKind.Special,ActiveFrames=1,Hitbox=new BattleRect(default,size),SpawnProjectile=true,
            ProjectileSpeed=1,ProjectileLifetimeFrames=rules.ArrowLifetimeFrames,HitstopFrames=rules.ProjectileHitstopFrames,HitstunFrames=rules.ProjectileHitstunFrames,KnockbackMultiplier=1,
            Ability=new AttackAbility {ProjectileChannel=ProjectileChannel.Support,Look=kind==SupportKind.ShionCar?AbilityLook.SupportCar:kind==SupportKind.Mai?AbilityLook.SupportArrow:AbilityLook.SupportPawn},
            ProjectileBehavior=new ProjectileBehaviorDefinition {ConsumeOnHit=true}};
        private void Spawn(ProjectileSystem system,SupportActor actor,FighterState target,StageGeometry stage,AttackDefinition attack,BattleVector position,int direction)
        {
            attack.Validate();system.AddSupport(actor.Owner,new AttackInstance(nextAttack++,attack,0,true),target,stage,position,direction);
        }
    }
}
