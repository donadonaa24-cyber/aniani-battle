using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;

namespace AniBato.EditorTools
{
    public static class RosterRefinementBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            for (int index = 0; index < roster.Length; index++)
            {
                var data = roster[index];
                if(index==7&&data.RefinementSchemaVersion==1)
                {
                    var pyramid=data.Ultimate.Definition;pyramid.Hitbox=new BattleRect(pyramid.Hitbox.Center,new BattleVector(5,3.4f));
                    data.Ultimate.Definition=pyramid;data.RefinementSchemaVersion=2;EditorUtility.SetDirty(data.Ultimate);EditorUtility.SetDirty(data);
                }
                if (data.RefinementSchemaVersion >= 1) continue;
                var n = data.Specials[0].Definition; var u = data.Ultimate.Definition;
                if (index == 0)
                {
                    n.ProjectileSpeed = 6; n.ProjectileLifetimeFrames = 180;
                    n.CooldownFrames = 180; n.Ability.ReplaceOwnProjectiles = true;
                    n.ProjectileBehavior = new ProjectileBehaviorDefinition { Homing = true, HomingSpeed = 6, ConsumeOnHit = true };
                    u.ProjectileBehavior.VisualScale = 1.6f;
                    var third = data.GroundNormals[2].Definition; third.ProjectileBehavior.VisualOffset = new BattleVector(0,-.55f);
                    data.GroundNormals[2].Definition = third; EditorUtility.SetDirty(data.GroundNormals[2]);
                }
                if (index == 2)
                {
                    u.SelfHealNormalHits = 1;
                    u.Aftermath = new AftermathDefinition { LifetimeFrames = 300, RepeatIntervalFrames = 60, Size = new BattleVector(5.5f,.85f) };
                }
                if (index == 6)
                {
                    n.Ability.ShotOrigin = ShotOrigin.Owner; n.ProjectileGravity = 0; n.ProjectileLaunchVelocity = default;
                    n.ProjectileSpeed = 8; n.ProjectileLifetimeFrames = 180; n.Ability.ProjectileCount = 3;
                    n.CooldownFrames = 240; n.Ability.ReplaceOwnProjectiles = true;
                    n.Hitbox = new BattleRect(default,new BattleVector(1.2f,1.4f)); n.DamageMode = AttackDamageMode.StatusOnly;
                    n.ParalysisFrames = 120; n.Knockback = default;
                    n.ProjectileBehavior = new ProjectileBehaviorDefinition { Formation = ProjectileFormation.Converging, FormationRadius = 6, Homing = true, HomingSpeed = 8 };
                    u.Ability.ShotOrigin = ShotOrigin.Owner; u.ProjectileGravity = 0; u.ProjectileLaunchVelocity = default;
                    u.ProjectileSpeed = 24; u.ProjectileLifetimeFrames = 90; u.Ability.ProjectileCount = 3;
                    u.Hitbox = new BattleRect(default,new BattleVector(2.2f,2.8f));
                    u.ProjectileBehavior = new ProjectileBehaviorDefinition { Formation = ProjectileFormation.AlternatingRush, FormationRadius = 7, StaggerFrames = 18 };
                }
                if (index == 7)
                {
                    u.Hitbox = new BattleRect(u.Hitbox.Center,new BattleVector(5,3.4f));
                    u.ProjectileBehavior = new ProjectileBehaviorDefinition { WallAfterLanding = true, WallFrames = 600 };
                }
                if (index == 8)
                {
                    u.SelfEffectsOnComplete = true; u.SelfBuff = new BuffDefinition { Channel = BuffChannel.MovementBoost, DurationFrames = 1200, MoveMultiplier = 1.5f, DashMultiplier = 1.5f };
                }
                n.Validate(); u.Validate(); data.Specials[0].Definition = n; data.Ultimate.Definition = u;
                EditorUtility.SetDirty(data.Specials[0]); EditorUtility.SetDirty(data.Ultimate);
                var movement = data.Movement; movement.Stamina = Stamina(index, movement.Stamina); data.Movement = movement;
                data.RefinementSchemaVersion = index==7?2:1; EditorUtility.SetDirty(data);
            }
        }
        public static StaminaDefinition Stamina(int i, StaminaDefinition old)
        {
            int[] maximum = { 100,110,105,95,90,100,130,100,120 };
            int[] rest = { 30,28,32,28,34,32,24,30,28 }, active = { 5,4,6,5,7,6,3,5,4 };
            int[] delay = { 30,36,30,30,24,30,42,30,36 }, dash = { 10,8,12,10,7,9,15,11,9 };
            int[] dodge = { 16,18,17,16,12,14,22,16,18 }, specialGuard = { 18,22,20,20,24,18,16,20,22 };
            old.Maximum = maximum[i]; old.GroundRecoveryPerSecond = rest[i]; old.ActiveRecoveryPerSecond = active[i]; old.RecoveryDelayFrames = delay[i];
            old.GroundDashCost = dash[i]; old.AirDashCost = dash[i]+6; old.DodgeCost = dodge[i]; old.SpecialGuardCost = specialGuard[i];
            old.JumpCost = i == 6 ? 12 : i == 4 ? 6 : 8; old.AirJumpCost = old.JumpCost+4;
            old.NormalGuardCost = i == 6 ? 6 : i == 4 ? 10 : 8; old.UltimateGuardCost = i == 6 ? 58 : i == 4 ? 70 : 65;
            old.Validate(); return old;
        }
    }
}
