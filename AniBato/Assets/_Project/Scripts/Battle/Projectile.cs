using System.Collections.Generic;

namespace AniBato.Battle
{
    public sealed class Projectile
    {
        public FighterState Owner { get; }
        public AttackInstance Attack { get; }
        public AttackInstance Group { get; }
        public BattleVector Position { get; private set; }
        public int Facing { get; private set; }
        public int VariantIndex { get; }
        public int DelayRemaining { get; private set; }
        public bool Landed { get; private set; }
        public bool Visible => DelayRemaining == 0;
        public bool CanDamage => Visible && !Landed;
        private readonly FighterState target;
        private readonly StageGeometry stage;
        private int wallIndex = -1;
        public int RemainingFrames { get; private set; }
        public BattleVector Velocity { get; private set; }
        internal int ContactCooldown;
        public BattleRect Hitbox => new BattleRect(Position, Attack.Definition.Hitbox.Size);
        internal Projectile(FighterState owner, AttackInstance attack, FighterState target = null, StageGeometry stage = null, float offsetX = 0, int variantIndex = 0, BattleVector? fixedPosition = null, AttackInstance group = null, int delayFrames = 0)
        {
            Owner = owner; Attack = attack; Group=group??attack; this.target = target; this.stage = stage; VariantIndex = variantIndex;
            Facing = attack.Definition.NormalDirection == NormalDirection.Back || attack.Definition.NormalDirection == NormalDirection.AirBack ? -owner.Facing : owner.Facing;
            Position = attack.WorldHitbox(owner).Center;
            if (attack.Definition.Ability.ShotOrigin == ShotOrigin.OpponentAbove && target != null)
            {
                float x = target.Position.X;
                float range = attack.Definition.Ability.TargetRange;
                if (range > 0) x = System.Math.Max(owner.Position.X - range, System.Math.Min(owner.Position.X + range, x));
                Position = new BattleVector(x + attack.Definition.Hitbox.Center.X * owner.Facing, target.Position.Y + attack.Definition.Hitbox.Center.Y);
            }
            else if (attack.Definition.Ability.ShotOrigin == ShotOrigin.StageCenter && stage != null)
                Position = new BattleVector(stage[0].Bounds.Center.X + attack.Definition.Hitbox.Center.X, stage[0].Bounds.Top + attack.Definition.Hitbox.Center.Y);
            Position += new BattleVector(offsetX, 0);
            if (attack.Definition.Ability.PlaceOnGround && stage != null && owner.Grounded)
            {
                var platform = stage[owner.GroundPlatform].Bounds; float half = attack.Definition.Hitbox.Size.X * .5f;
                float x = System.Math.Max(platform.Left + half, System.Math.Min(platform.Right - half, Position.X));
                Position = new BattleVector(x, platform.Top + attack.Definition.Hitbox.Size.Y * .5f);
            }
            RemainingFrames = attack.Definition.ProjectileLifetimeFrames;
            Velocity = new BattleVector(Facing * attack.Definition.ProjectileSpeed + Facing * attack.Definition.ProjectileLaunchVelocity.X,
                attack.Definition.ProjectileLaunchVelocity.Y);
            var behavior=attack.Definition.ProjectileBehavior;
            if(behavior.Formation!=ProjectileFormation.Default&&target!=null)
            {
                Position=ProjectileFormationMotion.Spawn(behavior,target.Position,variantIndex);
                DelayRemaining=variantIndex*behavior.StaggerFrames; RemainingFrames+=DelayRemaining;
                Velocity=behavior.Formation==ProjectileFormation.Converging ? ProjectileFormationMotion.Toward(Position,target.Position,behavior.HomingSpeed) :
                    new BattleVector(variantIndex%2==0?attack.Definition.ProjectileSpeed:-attack.Definition.ProjectileSpeed,0);
                if(Velocity.X!=0)Facing=Velocity.X>0?1:-1;
            }
            if(fixedPosition.HasValue)Position=fixedPosition.Value;
            if(delayFrames>0){DelayRemaining=delayFrames;RemainingFrames+=delayFrames;}
        }
        internal void Step()
        {
            if (ContactCooldown > 0) ContactCooldown--;
            if(DelayRemaining>0)
            {
                if(Attack.Definition.Ability.ProjectileChannel==ProjectileChannel.Aftermath&&Owner.Attack==Group&&Owner.IsFrozenThisTick)return;
                DelayRemaining--;RemainingFrames--;
                if(DelayRemaining==0&&target!=null)Position=ProjectileFormationMotion.Spawn(Attack.Definition.ProjectileBehavior,target.Position,VariantIndex);
                return;
            }
            if(Landed){RemainingFrames--;return;}
            var behavior=Attack.Definition.ProjectileBehavior;
            if(behavior.Homing&&target!=null&&target.Mode!=FighterMode.KO)
                Velocity=ProjectileFormationMotion.Toward(Position,target.Position,behavior.HomingSpeed);
            if(Velocity.X!=0)Facing=Velocity.X>0?1:-1;
            var previous=Position;
            Velocity = new BattleVector(Velocity.X, Velocity.Y - Attack.Definition.ProjectileGravity * BattleRuleDefinition.TickSeconds);
            Position += Velocity * BattleRuleDefinition.TickSeconds;
            if(behavior.WallAfterLanding&&stage!=null&&Velocity.Y<0)
            {
                float halfWidth=Hitbox.Size.X*.5f,halfHeight=Hitbox.Size.Y*.5f,best=float.NegativeInfinity;
                for(int i=0;i<stage.CollisionPlatformCount;i++)
                {
                    if(!stage.IsActive(i))continue;var floor=stage[i].Bounds;
                    if(Position.X+halfWidth>floor.Left&&Position.X-halfWidth<floor.Right&&previous.Y-halfHeight>=floor.Top&&Position.Y-halfHeight<=floor.Top)
                        best=System.Math.Max(best,floor.Top);
                }
                if(!float.IsNegativeInfinity(best))
                {
                    Position=new BattleVector(Position.X,best+halfHeight);Velocity=default;Landed=true;
                    RemainingFrames=behavior.WallFrames;wallIndex=stage.AddSolid(Hitbox);return;
                }
            }
            RemainingFrames--;
        }
        internal void Dispose(){if(wallIndex>=0){stage.DeactivatePlatform(wallIndex);wallIndex=-1;}}
    }

    public sealed class ProjectileSystem
    {
        private readonly List<Projectile> projectiles = new List<Projectile>();
        public IReadOnlyList<Projectile> Projectiles { get; }
        public ProjectileSystem() => Projectiles = projectiles.AsReadOnly();
        private void RemoveWhere(System.Predicate<Projectile> predicate)
        { for(int i=projectiles.Count-1;i>=0;i--)if(predicate(projectiles[i])){projectiles[i].Dispose();projectiles.RemoveAt(i);} }
        public void RemoveClashed() => RemoveWhere(p => p.Attack.Clashed);
        public int PlacedCount(FighterState owner)
        { int count = 0; foreach (var p in projectiles) if (p.Owner == owner && p.Attack.Definition.Ability.ProjectileChannel == ProjectileChannel.Placed) count++; return count; }
        public void ResolvePlacedContact(in HitContact contact, bool hit)
        {
            var projectile = contact.Projectile;
            if(projectile==null)return;
            var behavior=projectile.Attack.Definition.ProjectileBehavior;
            if(hit&&behavior.ConsumeOnHit){projectile.Dispose();projectiles.Remove(projectile);return;}
            if(behavior.RepeatIntervalFrames>0)
            { projectile.Attack.ForgetHit(contact.Target.PlayerIndex);projectile.ContactCooldown=behavior.RepeatIntervalFrames;return; }
            if(projectile.Attack.Definition.Ability.ProjectileChannel!=ProjectileChannel.Placed)return;
            if (hit && projectile.Attack.Definition.Ability.ConsumePlacementOnHit) { projectile.Dispose();projectiles.Remove(projectile); return; }
            // Guarding does not silently disarm a trap. It can be blocked again after a short respite.
            projectile.Attack.ForgetHit(contact.Target.PlayerIndex); projectile.ContactCooldown = 30;
        }
        public void Step(FighterState p1, FighterState p2, int maximumPerOwner = 0, StageGeometry stage = null)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Step();
                if (projectiles[i].RemainingFrames <= 0 || projectiles[i].Attack.Clashed || projectiles[i].Owner.Mode == FighterMode.KO)
                { projectiles[i].Dispose();projectiles.RemoveAt(i); }
            }
            Spawn(p1, p2, maximumPerOwner, stage); Spawn(p2, p1, maximumPerOwner, stage);
            Aftermath(p1,stage);Aftermath(p2,stage);
            Erase(p1); Erase(p2);
        }
        private void Erase(FighterState fighter)
        {
            var shield = fighter.Attack;
            if (fighter.FrozenThisTick || shield == null || !shield.IsActive || shield.ErasuresRemaining == 0) return;
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                if (projectile.Owner == fighter || !shield.Overlaps(fighter, projectile.Hitbox)) continue;
                projectile.Dispose();projectiles.RemoveAt(i);
                if (shield.ErasuresRemaining > 0 && --shield.ErasuresRemaining == 0) break;
            }
        }
        private void Spawn(FighterState owner, FighterState target, int maximum, StageGeometry stage)
        {
            var attack = owner.Attack;
            if (owner.FrozenThisTick || attack == null || !attack.IsActive || !attack.Definition.SpawnProjectile || attack.ProjectileSpawned ||
                attack.Definition.Ability.ProjectileAtDamageFrame && !attack.IsDamageActive) return;
            attack.ProjectileSpawned = true;
            bool normalSlot = attack.Definition.Ability.IndependentNormalProjectile;
            var channel = attack.Definition.Ability.ProjectileChannel;
            if (attack.Definition.Ability.ReplaceOwnProjectiles)
                RemoveWhere(p => p.Owner == owner && p.Attack.Definition.Ability.ProjectileChannel == channel &&
                    (attack.Definition.IsUltimate || p.Attack.Definition.Ability.IndependentNormalProjectile == normalSlot));
            // A formation is one attack for the owner's limit and shared hit history.
            var instances = new HashSet<AttackInstance>();
            foreach (var shot in projectiles)
                if (shot.Owner == owner && shot.Attack.Definition.Ability.ProjectileChannel == channel && shot.Attack.Definition.Ability.IndependentNormalProjectile == normalSlot) instances.Add(shot.Group);
            int limit = attack.Definition.Ability.PlacementLimit > 0 ? attack.Definition.Ability.PlacementLimit : maximum;
            if (limit > 0 && instances.Count >= limit) return;
            int requested = System.Math.Max(1, attack.Definition.Ability.ProjectileCount);
            for (int i = 0; i < requested; i++)
            {
                float offset = (i - (requested - 1) * .5f) * attack.Definition.Ability.ProjectileSpacing;
                var member=attack;
                if(attack.Definition.ProjectileBehavior.Formation==ProjectileFormation.AlternatingRush)
                {
                    var definition=attack.Definition;
                    if(i<requested-1)
                    {definition.DamageMode=AttackDamageMode.StatusOnly;definition.Knockback=default;definition.HitstunFrames=24;definition.HitstopFrames=3;}
                    member=new AttackInstance(-(attack.Id*32+i+1),definition,0,false);
                }
                projectiles.Add(new Projectile(owner, member, target, stage, offset, i, group:attack));
            }
        }
        private void Aftermath(FighterState owner,StageGeometry stage)
        {
            var source=owner.Attack;
            if(owner.FrozenThisTick||stage==null||source==null||source.AftermathSpawned||!source.Definition.Aftermath.Enabled||!source.IsDamageActive)return;
            source.AftermathSpawned=true;var effect=source.Definition.Aftermath;
            float requested=owner.Position.X+source.Definition.Hitbox.Center.X*owner.Facing;
            float half=effect.Size.X*.5f,top=float.NegativeInfinity,x=requested;
            for(int i=0;i<stage.CollisionPlatformCount;i++)
            {
                if(!stage.IsActive(i))continue;var floor=stage[i].Bounds;
                if(requested+half>floor.Left&&requested-half<floor.Right&&floor.Top<=owner.Position.Y&&floor.Top>top)
                {top=floor.Top;x=System.Math.Max(floor.Left+half,System.Math.Min(floor.Right-half,requested));}
            }
            if(float.IsNegativeInfinity(top))return;
            var definition=source.Definition;definition.Name+="・残留";definition.Kind=AttackKind.Special;definition.DamageMode=AttackDamageMode.NormalAccumulation;
            definition.SpawnProjectile=true;definition.IsCloseRange=false;definition.NonDamaging=false;
            definition.StartupFrames=0;definition.ActiveFrames=1;definition.DamageDelayFrames=0;definition.RecoveryFrames=0;
            definition.ProjectileSpeed=0;definition.ProjectileGravity=0;definition.ProjectileLaunchVelocity=default;definition.ProjectileLifetimeFrames=effect.LifetimeFrames;
            definition.Hitbox=new BattleRect(default,effect.Size);definition.Knockback=new BattleVector(0,1);definition.HitstunFrames=8;definition.HitstopFrames=2;
            definition.Ability=new AttackAbility{AllowZeroProjectileSpeed=true,ProjectileChannel=ProjectileChannel.Aftermath,Look=AbilityLook.CurryResidue};
            definition.Recovery=default;definition.Aftermath=default;definition.SelfHealNormalHits=0;definition.SelfBuff=default;definition.ParalysisFrames=0;
            definition.ProjectileBehavior=new ProjectileBehaviorDefinition{RepeatIntervalFrames=effect.RepeatIntervalFrames};
            RemoveWhere(p=>p.Owner==owner&&p.Attack.Definition.Ability.ProjectileChannel==ProjectileChannel.Aftermath);
            var attack=new AttackInstance(-(source.Id*32+31),definition,0,false);
            int delay=source.Definition.StartupFrames+source.Definition.ActiveFrames-source.Frame;
            projectiles.Add(new Projectile(owner,attack,stage:stage,fixedPosition:new BattleVector(x,top+effect.Size.Y*.5f),group:source,delayFrames:delay));
        }
    }
}
