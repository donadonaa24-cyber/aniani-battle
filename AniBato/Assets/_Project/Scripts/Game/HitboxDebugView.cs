using System.Collections.Generic;
using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    public sealed class HitboxDebugView : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private PresentationSettings presentation;
        private readonly List<LineRenderer> lines = new List<LineRenderer>();
        private Material material;
        private int used;
        public void Configure(BattleRunner value, PresentationSettings settings) { runner = value; presentation = settings; }
        private void LateUpdate()
        {
            used = 0;
            if (runner && runner.DebugBoxes && runner.Simulation != null)
            {
                DrawFighter(runner.Simulation.P1); DrawFighter(runner.Simulation.P2);
                foreach (var projectile in runner.Simulation.Projectiles) Draw(projectile.Hitbox, Color.yellow);
            }
            for (int i = used; i < lines.Count; i++) lines[i].enabled = false;
        }
        private void DrawFighter(FighterState fighter)
        {
            Draw(fighter.Hurtbox, Color.green);
            if (fighter.Attack == null || !fighter.Attack.IsDamageActive || fighter.Attack.Definition.SpawnProjectile || fighter.Attack.Definition.NonDamaging) return;
            Draw(fighter.Attack.WorldHitbox(fighter), Color.red);
            if (fighter.Attack.Definition.HasSecondaryHitbox) Draw(fighter.Attack.Definition.SecondaryHitbox.At(fighter.Position, fighter.Facing), Color.red);
        }
        private void Draw(in BattleRect rect, Color color)
        {
            if (!material) material = new Material(Shader.Find("Sprites/Default"));
            if (used == lines.Count)
            {
                var go = new GameObject("Debug Box " + used); go.transform.SetParent(transform, false);
                var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = material; line.useWorldSpace = true;
                line.positionCount = 5; line.startWidth = line.endWidth = presentation.DebugLineWidth; line.sortingOrder = 50; lines.Add(line);
            }
            var current = lines[used++]; current.enabled = true; current.startColor = current.endColor = color;
            current.SetPosition(0, new Vector3(rect.Left, rect.Bottom, 0)); current.SetPosition(1, new Vector3(rect.Left, rect.Top, 0));
            current.SetPosition(2, new Vector3(rect.Right, rect.Top, 0)); current.SetPosition(3, new Vector3(rect.Right, rect.Bottom, 0)); current.SetPosition(4, new Vector3(rect.Left, rect.Bottom, 0));
        }
        private void OnDestroy() { if (material) Destroy(material); }
    }
}

