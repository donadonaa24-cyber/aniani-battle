using System.Collections.Generic;
using UnityEngine;
using AniBato.Battle;

namespace AniBato.Game
{
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private PresentationSettings presentation;
        private readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> rifts = new List<SpriteRenderer>();
        private readonly ProjectileArtworkSlots artworkSlots=new ProjectileArtworkSlots();
        public void Configure(BattleRunner value, PresentationSettings settings) { runner = value; presentation = settings; }
        private void LateUpdate()
        {
            if (runner.Simulation == null) return;
            var projectiles = runner.Simulation.Projectiles;
            artworkSlots.Refresh(projectiles);
            while (sprites.Count < projectiles.Count)
            {
                sprites.Add(ProceduralSprites.Create(transform, "Projectile", Vector2.one, presentation.ProjectileTint, true, 7));
                rifts.Add(ProceduralSprites.Create(transform,"Royal Summon Rift",Vector2.one,Color.white,sortingOrder:6));
            }
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].enabled = i < projectiles.Count;
                rifts[i].enabled = false;
                if (i >= projectiles.Count) continue;
                var projectile = projectiles[i];
                if (!projectile.Visible) { sprites[i].enabled = false; continue; }
                var behavior = projectile.Attack.Definition.ProjectileBehavior;
                sprites[i].sortingOrder = 7;
                var look = projectile.Attack.Definition.Ability.Look;
                bool placeholder = look != AbilityLook.Default && look != AbilityLook.RiceBall;
                var character = runner.Character(projectile.Owner.PlayerIndex);
                var animations = character.Animations;
                var effects = character.SpriteEffects != null && character.SpriteEffects.SchemaVersion >= 1 ? character.SpriteEffects : null;
                string clip = effects == null ? "rice_ball" : effects.ProjectileClip;
                bool normal = projectile.Attack.Definition.IsNormal;
                var normalEffects = normal && character.SpriteEffects != null && character.SpriteEffects.NormalProjectileSchemaVersion >= 1 ? character.SpriteEffects : null;
                if (normalEffects != null) clip = normalEffects.NormalProjectileClip;
                Sprite sprite = !placeholder && animations ? animations.Effect(clip, projectile.Attack.Definition.ProjectileLifetimeFrames - projectile.RemainingFrames) : null;
                var paper = character.PaperMotion;
                var art = paper ? paper.Projectile(look) : null;
                if (look == AbilityLook.TrapCard && paper) art = paper.Projectile(AbilityLook.Card);
                if (!placeholder && paper && paper.Enabled && (normal ? paper.Bag : paper.RiceBall)) sprite = normal ? paper.Bag : paper.RiceBall;
                if (placeholder) sprite = null;
                sprites[i].sprite = sprite ? sprite : ProceduralSprites.Shape(true);
                sprites[i].color = sprite ? Color.white : runner.Character(projectile.Owner.PlayerIndex).Tint;
                if (placeholder)
                {
                    sprites[i].sprite = ProceduralSprites.Shape(look == AbilityLook.Drone || look == AbilityLook.BlackHole);
                    sprites[i].color = look == AbilityLook.BlackHole ? new Color(.35f,.12f,.65f,.8f) :
                        look == AbilityLook.Drone ? Color.cyan : look == AbilityLook.Rocket ? new Color(1,.35f,.15f) : new Color(1,.85f,.35f);
                }
                sprites[i].flipX = sprite && projectile.Facing < 0;
                int age = projectile.Attack.Definition.ProjectileLifetimeFrames - projectile.RemainingFrames;
                sprites[i].transform.localRotation = Quaternion.Euler(0, 0, !placeholder && normal && paper && paper.Enabled && paper.Bag ? age / 8 * 45 * projectile.Facing : look == AbilityLook.Pyramid ? 45 : 0);
                var position = runner.RenderPosition(projectile);
                position += new BattleVector(behavior.VisualOffset.X * projectile.Facing, behavior.VisualOffset.Y);
                sprites[i].transform.position = new Vector3(position.X, position.Y, 0);
                sprites[i].transform.localScale = sprite ? Vector3.one * (normalEffects != null ? normalEffects.NormalProjectileScale : effects == null ? presentation.RiceBallScale : effects.ProjectileScale) : new Vector3(projectile.Hitbox.Size.X, projectile.Hitbox.Size.Y, 1);
                if (art != null)
                {
                    int variant=artworkSlots.Index(projectile);
                    sprites[i].sprite = art.SpriteFor(variant); sprites[i].color = Color.white;
                    sprites[i].sortingOrder = art.SortingOrder;
                    sprites[i].flipX = (art.FlipWithFacing || look == AbilityLook.King) && projectile.Facing < 0;
                    float angle = art.RotateWithVelocity ? Mathf.Atan2(projectile.Velocity.Y, projectile.Velocity.X) * Mathf.Rad2Deg : 0;
                    sprites[i].transform.localRotation = Quaternion.Euler(0, 0, angle + art.RotationOffsetDegrees);
                    sprites[i].transform.localScale = Vector3.one * art.ScaleFor(projectile.Hitbox.Size,variant);
                }
                if (behavior.VisualScale > 0) sprites[i].transform.localScale *= behavior.VisualScale;
                if (art != null && projectile.Attack.AbilityChargeReleased && !projectile.Attack.Definition.AbilityCharge.FormationOnly && !behavior.ExplodeOnLanding)
                {
                    var size = art.VariantVisibleSizes != null && art.VariantVisibleSizes.Length > 0 ?
                        art.VariantVisibleSizes[artworkSlots.Index(projectile)%art.VariantVisibleSizes.Length] : art.VisibleSize;
                    if (size.x > 0 && size.y > 0)
                        sprites[i].transform.localScale = new Vector3(projectile.Hitbox.Size.X/size.x,projectile.Hitbox.Size.Y/size.y,1);
                }
                if (projectile.Exploded)
                {
                    sprites[i].sprite = PaperBurstSprite.Sprite; sprites[i].color = Color.white;
                    sprites[i].transform.localRotation = Quaternion.identity;
                    sprites[i].transform.localScale = new Vector3(projectile.Hitbox.Size.X,projectile.Hitbox.Size.Y,1);
                }
                if(look==AbilityLook.SupportCar || look==AbilityLook.SupportArrow || look==AbilityLook.SupportPawn)
                {
                    var support=runner.Setup.SupportArt;var key=look==AbilityLook.SupportCar?"shion_car":look==AbilityLook.SupportArrow?"mai_arrow":"pickup_pawn";
                    var entry=support.Find(key);sprites[i].sprite=entry.Sprite;sprites[i].color=Color.white;sprites[i].sortingOrder=7;
                    sprites[i].flipX=look!=AbilityLook.SupportPawn&&projectile.Facing<0;
                    sprites[i].transform.localRotation=Quaternion.identity;
                    float height=look==AbilityLook.SupportCar?projectile.Hitbox.Size.Y*1.45f:look==AbilityLook.SupportArrow ? .45f : projectile.Hitbox.Size.Y;
                    float fit=look==AbilityLook.SupportArrow?projectile.Hitbox.Size.X/entry.VisibleSize.x:height/entry.VisibleSize.y;
                    sprites[i].transform.localScale=Vector3.one*fit;
                    sprites[i].transform.position=new Vector3(position.X,position.Y-projectile.Hitbox.Size.Y*.5f,0);
                }
                if (projectile.RoyalMarch)
                {
                    float emergence=Mathf.Clamp01(projectile.MarchAge/(float)behavior.MarchSpawnFrames);
                    float exit=projectile.MarchExitAge<0?0:Mathf.Clamp01(projectile.MarchExitAge/(float)behavior.MarchExitFrames);
                    sprites[i].color=new Color(1,1,1,emergence*(1-exit));
                    float step=projectile.MarchAge/8%2==0?-3:3;
                    sprites[i].transform.localRotation=Quaternion.Euler(0,0,projectile.CanDamage?step:0);
                    sprites[i].transform.position+=Vector3.right*projectile.Facing*(-.5f*(1-emergence)+exit*.3f);
                    if (paper && paper.SummonRift && (emergence<1||exit>0))
                    {
                        var rift=rifts[i];rift.enabled=true;rift.sprite=paper.SummonRift;rift.color=Color.white;
                        rift.transform.position=new Vector3(position.X,position.Y,0);
                        float opening=exit>0?1-exit:Mathf.Min(1,emergence*3);
                        rift.transform.localScale=new Vector3(opening*.75f,1,1)*(projectile.Hitbox.Size.Y*1.2f/paper.SummonRift.bounds.size.y);
                    }
                }
            }
        }
    }
}



