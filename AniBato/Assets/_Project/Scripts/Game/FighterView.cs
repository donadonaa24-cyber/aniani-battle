using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    public sealed class FighterView : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private CharacterData character;
        [SerializeField] private PresentationSettings presentation;
        [SerializeField] private int playerIndex;
        private SpriteRenderer body, eye, arm, secondArm, hat, food, mogu, wave, field;
        private SpriteRenderer[] ears;
        private float flashUntil, headY;
        public int PlayerIndex => playerIndex;
        public bool UsesAnimationSprites { get; private set; }
        public bool UsesPaperSprite { get; private set; }
        public float HeadY => headY;
        public AniBato.Presentation.AnimationSample RenderedSample { get; private set; }
        public void Configure(BattleRunner battleRunner, CharacterData data, PresentationSettings settings, int player)
        { runner = battleRunner; character = data; presentation = settings; playerIndex = player; }
        private void Start()
        {
            body = ProceduralSprites.Create(transform, "Body", new Vector2(character.Movement.HurtboxSize.X, character.Movement.HurtboxSize.Y), character.Tint, character.RoundShape, 5);
            eye = ProceduralSprites.Create(transform, "Facing Eye", new Vector2(0.17f, 0.2f), Color.white, sortingOrder: 6);
            eye.transform.localPosition = new Vector3(0.3f, 0.4f, 0);
            arm = ProceduralSprites.Create(transform, "Attack Arm", Vector2.one, character.Tint, sortingOrder: 4);
            secondArm = ProceduralSprites.Create(transform, "Cross Arm", Vector2.one, character.Tint, sortingOrder: 4);
            ears = new[] { ProceduralSprites.Create(transform, "Left Ear", new Vector2(0.35f, 0.5f), character.Tint, true, 6),
                ProceduralSprites.Create(transform, "Right Ear", new Vector2(0.35f, 0.5f), character.Tint, true, 6) };
            ears[0].transform.localPosition = new Vector3(-0.45f, 0.85f, 0); ears[1].transform.localPosition = new Vector3(0.45f, 0.85f, 0);
            // Reserved for a future awakening appearance; casual Chizuru never enables it.
            hat = ProceduralSprites.Create(transform, "Chef Hat", new Vector2(1, 0.45f), Color.white, sortingOrder: 6); hat.enabled = false;
            food = ProceduralSprites.Create(transform, "Meal", new Vector2(0.45f, 0.4f), Color.white, true, 7);
            mogu = ProceduralSprites.Create(transform, "Mogu Status", Vector2.one, Color.white, sortingOrder: 8);
            wave = ProceduralSprites.Create(transform, "Curry Wave", Vector2.one, Color.white, sortingOrder: 7);
            field = ProceduralSprites.Create(transform, "Blocking Field", Vector2.one, Color.white, sortingOrder: 7);
            arm.enabled = secondArm.enabled = food.enabled = mogu.enabled = wave.enabled = field.enabled = false;
            ApplyCharacter();
        }
        private void OnEnable() { if (runner) { runner.EventProduced += OnBattleEvent; runner.MatchRestarted += OnRestart; } }
        private void OnDisable() { if (runner) { runner.EventProduced -= OnBattleEvent; runner.MatchRestarted -= OnRestart; } }
        private void OnRestart() { flashUntil = 0; if (body) ApplyCharacter(); }
        private void ApplyCharacter()
        {
            character = runner.Character(playerIndex);
            body.sprite = ProceduralSprites.Shape(character.RoundShape); body.flipX = false;
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(character.Movement.HurtboxSize.X, character.Movement.HurtboxSize.Y, 1);
            foreach (var ear in ears) { ear.enabled = character.Silhouette == 1; ear.color = character.Tint; }
            hat.enabled = false;
        }
        private void OnBattleEvent(BattleEvent battleEvent)
        { if (battleEvent.Kind == BattleEventKind.Hit && battleEvent.TargetPlayer == playerIndex) flashUntil = Time.unscaledTime + presentation.HitFlashSeconds; }
        private void LateUpdate() => RenderFrame();
        public void RenderFrame()
        {
            if (runner.Simulation == null || !body) return;
            var fighter = playerIndex == 0 ? runner.Simulation.P1 : runner.Simulation.P2;
            var position = runner.RenderPosition(playerIndex);
            transform.position = new Vector3(position.X, position.Y, 0);
            var sample = runner.Animation(playerIndex); Sprite sprite = null;
            RenderedSample = sample;
            UsesAnimationSprites = character.Animations && character.Animations.TrySprite(sample, out sprite);
            PaperPose paperPose = null; Sprite paperSprite = null;
            UsesPaperSprite = character.PaperMotion && character.PaperMotion.TrySprite(sample, fighter.LastDamageLostHeart, out paperSprite, out paperPose);
            if (UsesPaperSprite) { sprite = paperSprite; UsesAnimationSprites = true; }
            transform.localScale = UsesAnimationSprites ? Vector3.one : new Vector3(fighter.Facing, 1, 1);
            eye.enabled = !UsesAnimationSprites;
            foreach (var ear in ears) ear.enabled = !UsesAnimationSprites && character.Silhouette == 1;
            if (UsesAnimationSprites)
            {
                body.sprite = sprite; body.flipX = sample.FlipX;
                body.transform.localScale = Vector3.one;
                var rock = UsesPaperSprite ? character.PaperMotion.Transform(sample, paperPose) : default;
                body.transform.localRotation = Quaternion.Euler(0, 0, rock.Degrees);
                float baseY = paperPose != null && paperPose.Airborne ? 0 : -fighter.Definition.HurtboxSize.Y * .5f;
                if (paperPose != null && paperPose.Airborne && fighter.Grounded)
                    baseY = -fighter.Definition.HurtboxSize.Y * .5f - paperPose.FootBaseY;
                body.transform.localPosition = new Vector3(0, baseY + rock.Lift, 0);
                float paperHeight = paperPose == null ? character.PaperMotion ? character.PaperMotion.StandingHeight : 0 : paperPose.HeadHeight;
                float spriteHeight = UsesPaperSprite ? paperHeight * Mathf.Cos(rock.Degrees * Mathf.Deg2Rad) + rock.Lift :
                    character.Animations.HeadHeight(sample) / character.Animations.PixelsPerUnit;
                headY = position.Y + baseY + spriteHeight;
            }
            else
            {
                body.sprite = ProceduralSprites.Shape(character.RoundShape); body.flipX = false;
                body.transform.localPosition = Vector3.zero;
                body.transform.localRotation = Quaternion.identity;
                body.transform.localScale = new Vector3(fighter.Definition.HurtboxSize.X, fighter.Definition.HurtboxSize.Y, 1);
                headY = position.Y + fighter.Definition.HurtboxSize.Y * 0.5f;
            }
            Color baseColor = UsesAnimationSprites ? Color.white : character.Tint;
            var effects = character.SpriteEffects != null && character.SpriteEffects.SchemaVersion >= 1 ? character.SpriteEffects : null;
            body.color = baseColor;
            if (UsesAnimationSprites && effects != null && sample.State == AniBato.Presentation.AnimationState.SpecialSide)
                body.color = effects.SideSpecialTint;
            if (fighter.Mode == FighterMode.Guard) body.color = presentation.GuardTint;
            if (fighter.Mode == FighterMode.Dodge) body.color = presentation.DodgeTint;
            if (fighter.Mode == FighterMode.Counter) body.color = presentation.CounterTint;
            if (fighter.ParalysisRemaining > 0) body.color = Color.Lerp(baseColor, Color.yellow, 0.6f);
            if (fighter.BuffRemaining > 0) body.color = Color.Lerp(body.color, Color.white, 0.3f);
            if (fighter.Mode == FighterMode.KO) body.color = Color.Lerp(baseColor, Color.black, 0.65f);
            if (fighter.HeartRecoveryRemaining > 0 && fighter.HeartRecoveryRemaining / 5 % 2 == 0)
            { var color = body.color; color.a = .35f; body.color = color; }
            if (Time.unscaledTime < flashUntil && Mathf.FloorToInt(Time.unscaledTime * 40) % 2 == 0)
                body.color = UsesAnimationSprites ? new Color(1, 1, 1, presentation.SpriteHitFlashOpacity) : Color.white;
            food.enabled = !UsesAnimationSprites && fighter.Attack != null && fighter.Attack.Definition.NonDamaging && fighter.Attack.Definition.SelfHealNormalHits > 0;
            if (food.enabled) food.transform.localPosition = new Vector3(0.45f, 0.35f + Mathf.Sin(fighter.Attack.Frame * 0.5f) * 0.08f, 0);
            arm.enabled = !UsesAnimationSprites && fighter.Attack != null && fighter.Attack.IsActive && !fighter.Attack.Definition.SpawnProjectile;
            secondArm.enabled = arm.enabled && fighter.Attack.Definition.HasSecondaryHitbox;
            if (arm.enabled)
            {
                var hitbox = fighter.Attack.Definition.Hitbox;
                arm.transform.localPosition = new Vector3(hitbox.Center.X, hitbox.Center.Y, 0); arm.transform.localScale = new Vector3(hitbox.Size.X, hitbox.Size.Y * 0.25f, 1); arm.color = character.Tint;
                if (secondArm.enabled) { var secondary = fighter.Attack.Definition.SecondaryHitbox; secondArm.transform.localPosition = new Vector3(secondary.Center.X, secondary.Center.Y, 0); secondArm.transform.localScale = new Vector3(secondary.Size.X, secondary.Size.Y, 1); secondArm.color = character.Tint; }
            }
            mogu.sprite = runner.EffectSprite("mogu", sample.Age);
            mogu.enabled = fighter.ParalysisRemaining > 0 && mogu.sprite;
            mogu.transform.position = new Vector3(position.X + presentation.StatusIconOffset.x, headY + presentation.StatusIconOffset.y, 0); mogu.transform.localScale = Vector3.one * presentation.StatusIconScale;
            int effectAge = sample.IsAttack ? Mathf.Max(0, sample.AttackFrame - sample.Startup) : 0;
            wave.sprite = character.Animations ? character.Animations.Effect(effects == null ? "curry_wave" : effects.UltimateClip, effectAge) : null;
            if (character.PaperMotion && character.PaperMotion.Enabled && character.PaperMotion.CurryWave) wave.sprite = character.PaperMotion.CurryWave;
            wave.enabled = UsesAnimationSprites && fighter.Attack != null && fighter.Attack.Definition.IsUltimate && fighter.Attack.IsActive && wave.sprite;
            float waveScale = effects == null ? presentation.CurryWaveScale : effects.UltimateScale;
            Vector2 waveOffset = effects == null ? presentation.CurryWaveOffset : effects.UltimateOffset;
            wave.flipX = sample.FlipX; wave.transform.localScale = new Vector3(waveScale, waveScale, 1);
            wave.transform.localPosition = new Vector3((sample.FlipX ? -1 : 1) * waveOffset.x, waveOffset.y, 0);
            field.sprite = character.Animations && effects != null ? character.Animations.Effect(effects.DownSpecialClip, effectAge) : null;
            field.enabled = UsesAnimationSprites && sample.State == AniBato.Presentation.AnimationState.SpecialDown && fighter.Attack != null && fighter.Attack.IsActive && field.sprite;
            field.flipX = sample.FlipX;
            if (effects != null)
            {
                field.transform.localScale = Vector3.one * effects.DownSpecialScale;
                field.color = new Color(1, 1, 1, effects.DownSpecialOpacity);
                field.transform.localPosition = new Vector3((sample.FlipX ? -1 : 1) * effects.DownSpecialOffset.x, effects.DownSpecialOffset.y, 0);
            }
        }
    }
}


