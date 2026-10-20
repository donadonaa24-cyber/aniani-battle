using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    // Whole-illustration transform only. These paper scraps never enter the simulation.
    public sealed class ChargePoseView
    {
        private readonly SpriteRenderer[] scraps = new SpriteRenderer[4];
        private readonly PresentationSettings settings;
        public ChargePoseView(Transform parent, PresentationSettings settings)
        {
            this.settings = settings;
            for (int i = 0; i < scraps.Length; i++)
            {
                scraps[i] = ProceduralSprites.Create(parent, "Charge Paper " + i, Vector2.one * settings.ChargeSparkSize, settings.ChargeTint, sortingOrder: 4);
                scraps[i].transform.localRotation = Quaternion.Euler(0, 0, 45);
                scraps[i].enabled = false;
            }
        }
        public float Apply(SpriteRenderer body, FighterState fighter, in ChargeAttackDefinition rules)
        {
            bool abilityCharge = AbilityChargeSystem.IsCharging(fighter);
            bool charging = fighter.Mode == FighterMode.Charging && fighter.Grounded || abilityCharge;
            for (int i = 0; i < scraps.Length; i++) scraps[i].enabled = charging;
            if (!charging) return 0;
            int frames = abilityCharge ? fighter.Attack.AbilityChargeFrames : fighter.ChargeFrames;
            float amount = abilityCharge ? Mathf.Clamp01(frames / (float)fighter.Attack.Definition.AbilityCharge.MaximumFrames) :
                Mathf.Clamp01(frames / (float)rules.MaximumFrames);
            int pulse = frames / Mathf.Max(1, settings.ChargePulseTicks) % 2;
            float sink = settings.ChargeSink * (.3f + .7f * amount);
            body.transform.localRotation = Quaternion.Euler(0, 0, -fighter.Facing * settings.ChargeTiltDegrees * (.4f + .6f * amount));
            if (!abilityCharge) body.transform.localPosition = new Vector3(0, -fighter.Definition.HurtboxSize.Y * .5f - sink, 0);
            else if (fighter.Grounded) body.transform.localPosition += Vector3.down * sink;
            body.color = Color.Lerp(Color.white, settings.ChargeTint, (.2f + .55f * amount) * (pulse == 0 ? 1 : .7f));
            for (int i = 0; i < scraps.Length; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * (fighter.Definition.HurtboxSize.X * .7f + pulse * settings.ChargeSparkSize);
                scraps[i].transform.localPosition = new Vector3(x, (i / 2 == 0 ? -.3f : .5f) * fighter.Definition.HurtboxSize.Y, 0);
                scraps[i].color = Color.Lerp(Color.white, settings.ChargeTint, amount);
                scraps[i].transform.localScale = Vector3.one * settings.ChargeSparkSize * (1 + amount);
            }
            return -sink;
        }
    }
}
