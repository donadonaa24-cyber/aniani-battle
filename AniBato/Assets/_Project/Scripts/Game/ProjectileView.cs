using System.Collections.Generic;
using UnityEngine;

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
                var character = runner.Character(projectile.Owner.PlayerIndex);
                var animations = character.Animations;
                var effects = character.SpriteEffects != null && character.SpriteEffects.SchemaVersion >= 1 ? character.SpriteEffects : null;
                string clip = effects == null ? "rice_ball" : effects.ProjectileClip;
                bool normal = projectile.Attack.Definition.IsNormal;
                var normalEffects = normal && character.SpriteEffects != null && character.SpriteEffects.NormalProjectileSchemaVersion >= 1 ? character.SpriteEffects : null;
                if (normalEffects != null) clip = normalEffects.NormalProjectileClip;
                Sprite sprite = animations ? animations.Effect(clip, projectile.Attack.Definition.ProjectileLifetimeFrames - projectile.RemainingFrames) : null;
                sprites[i].sprite = sprite ? sprite : ProceduralSprites.Shape(true);
                sprites[i].color = sprite ? Color.white : runner.Character(projectile.Owner.PlayerIndex).Tint;
                sprites[i].flipX = sprite && projectile.Facing < 0;
                var position = runner.RenderPosition(projectile);
                sprites[i].transform.position = new Vector3(position.X, position.Y, 0);
                sprites[i].transform.localScale = sprite ? Vector3.one * (normalEffects != null ? normalEffects.NormalProjectileScale : effects == null ? presentation.RiceBallScale : effects.ProjectileScale) : new Vector3(projectile.Hitbox.Size.X, projectile.Hitbox.Size.Y, 1);
            }
        }
    }
}



