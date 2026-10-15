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
        public void Configure(BattleRunner value, PresentationSettings settings) { runner = value; presentation = settings; }
        private void LateUpdate()
        {
            if (runner.Simulation == null) return;
            var projectiles = runner.Simulation.Projectiles;
            while (sprites.Count < projectiles.Count)
                sprites.Add(ProceduralSprites.Create(transform, "Projectile", Vector2.one, presentation.ProjectileTint, true, 7));
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].enabled = i < projectiles.Count;
                if (i >= projectiles.Count) continue;
                var projectile = projectiles[i];
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
                sprites[i].transform.position = new Vector3(position.X, position.Y, 0);
                sprites[i].transform.localScale = sprite ? Vector3.one * (normalEffects != null ? normalEffects.NormalProjectileScale : effects == null ? presentation.RiceBallScale : effects.ProjectileScale) : new Vector3(projectile.Hitbox.Size.X, projectile.Hitbox.Size.Y, 1);
            }
        }
    }
}



