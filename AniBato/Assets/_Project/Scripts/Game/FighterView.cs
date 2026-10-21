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
        private ChargePoseView chargePose;
        private readonly AniBato.Presentation.PaperUltimatePoseHold ultimatePoseHold=new AniBato.Presentation.PaperUltimatePoseHold();
        private AbilityCueView abilityCue;
        private TeleportPhaseView teleportPhase;
        private SummonCommandView summonCommand;
        private float flashUntil, headY;
        private int curryLastActiveTick=-1;
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
            chargePose = new ChargePoseView(transform, presentation);
            abilityCue = new AbilityCueView(transform);
            teleportPhase = new TeleportPhaseView(transform, presentation);
            summonCommand = new SummonCommandView(transform);
            arm.enabled = secondArm.enabled = food.enabled = mogu.enabled = wave.enabled = field.enabled = false;
            ApplyCharacter();
        }
        private void OnEnable() { if (runner) { runner.EventProduced += OnBattleEvent; runner.MatchRestarted += OnRestart; } }
        private void OnDisable() { if (runner) { runner.EventProduced -= OnBattleEvent; runner.MatchRestarted -= OnRestart; } }
        private void OnRestart() { flashUntil = 0; curryLastActiveTick=-1; ultimatePoseHold.Reset(); teleportPhase?.Reset(); if (body) ApplyCharacter(); }
        private void OnDestroy() => teleportPhase?.Dispose();
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
            if(fighter.ChargeFrames>0)ultimatePoseHold.Reset();
            if(character.PaperMotion&&character.PaperMotion.Enabled)
                sample=ultimatePoseHold.Evaluate(sample,runner.Simulation.Match.TickCount,character.PaperMotion.UltimateImpactMinimumTicks);
            // Reuse an existing whole key illustration; keep the public motion sample truthful.
            if (fighter.Attack != null && sample.State == AniBato.Presentation.AnimationState.SpecialDown)
            {
                var ability = fighter.Attack.Definition.Ability;
                if (ability.SuperGuard) sample.State = fighter.Attack.IsActive ? AniBato.Presentation.AnimationState.SpecialNeutral : AniBato.Presentation.AnimationState.Guard;
                else if (ability.Look == AbilityLook.Reading) { sample.State = AniBato.Presentation.AnimationState.Ultimate; sample.Startup = sample.DamageDelay = 0; }
                else if (ability.Conscious) sample.State = AniBato.Presentation.AnimationState.Dodge;
                if (ability.ProjectileChannel == ProjectileChannel.Placed && ability.PlaceOnGround)
                    sample.State = AniBato.Presentation.AnimationState.Crouch;
            }
            bool recovering = fighter.Attack != null && fighter.Attack.Definition.Recovery.Style != RecoveryStyle.None;
            if (recovering && (fighter.Attack.Definition.Ability.Look == AbilityLook.Rook || fighter.Attack.Definition.Ability.Look == AbilityLook.Bishop || fighter.Attack.Definition.Ability.Look == AbilityLook.King))
            {
                sample.State = AniBato.Presentation.AnimationState.SpecialNeutral;
                if (fighter.Attack.IsActive) sample.AttackFrame = Mathf.Max(sample.AttackFrame, sample.Startup + sample.DamageDelay);
            }
            else if (recovering && fighter.Attack.Definition.Ability.Conscious) sample.State = AniBato.Presentation.AnimationState.Dodge;
            UsesAnimationSprites = character.Animations && character.Animations.TrySprite(sample, out sprite);
            PaperPose paperPose = null; Sprite paperSprite = null;
            UsesPaperSprite = character.PaperMotion && character.PaperMotion.TrySprite(sample, fighter.LastDamageLostHeart, out paperSprite, out paperPose);
            bool hanging=fighter.Mode==FighterMode.LedgeHang && !runner.Simulation.Match.IsFinished && character.PaperMotion && character.PaperMotion.LedgeHang;
            if(hanging) { paperSprite=character.PaperMotion.LedgeHang; paperPose=null; UsesPaperSprite=true; }
            if (recovering && character.PaperMotion && character.PaperMotion.UpRecovery != null && character.PaperMotion.UpRecovery.Valid)
            { paperPose = character.PaperMotion.UpRecovery; paperSprite = paperPose.Frames[0]; UsesPaperSprite = true; }
            if (UsesPaperSprite) { sprite = paperSprite; UsesAnimationSprites = true; }
            transform.localScale = UsesAnimationSprites ? Vector3.one : new Vector3(fighter.Facing, 1, 1);
            eye.enabled = !UsesAnimationSprites;
            foreach (var ear in ears) ear.enabled = !UsesAnimationSprites && character.Silhouette == 1;
            if (UsesAnimationSprites)
            {
                var visualSample = sample;
                if (paperPose != null && paperPose.ReverseFacing) visualSample.FlipX = !visualSample.FlipX;
                bool turnWithRush=UsesPaperSprite&&paperPose!=null&&paperPose.RotateWithRush&&paperPose.Frames.Length==2&&paperPose.FrameIndex(sample)==1;
                bool unmirroredImpact=UsesPaperSprite&&paperPose!=null&&paperPose.UnmirroredImpact&&paperPose.Frames.Length==2&&paperPose.FrameIndex(sample)==1;
                body.sprite = sprite; body.flipX = !turnWithRush&&!unmirroredImpact&&visualSample.FlipX;
                body.transform.localScale = Vector3.one;
                var rock = UsesPaperSprite ? character.PaperMotion.Transform(visualSample, paperPose) : default;
                var performance = UsesPaperSprite && !visualSample.IsCharging ? AniBato.Presentation.PaperPerformance.Evaluate(character.PaperMotion.Performance, visualSample, fighter.LastDamageLostHeart) : default;
                float angle = rock.Degrees + performance.Degrees + (paperPose==null?0:paperPose.TiltDegrees*(visualSample.FlipX?-1:1));
                if(turnWithRush&&fighter.Attack!=null&&fighter.Attack.IsActive&&
                    fighter.Attack.Definition.Ability.Motion==AniBato.Battle.AbilityMotion.TwoPhaseRush&&
                    Mathf.Abs(fighter.Velocity.X)+Mathf.Abs(fighter.Velocity.Y)>.001f)
                    angle=Mathf.Atan2(fighter.Velocity.Y,fighter.Velocity.X)*Mathf.Rad2Deg-90;
                body.transform.localRotation = Quaternion.Euler(0, 0, angle);
                float baseY = paperPose != null && paperPose.Airborne ? 0 : -fighter.Definition.HurtboxSize.Y * .5f;
                if (paperPose != null && paperPose.Airborne && fighter.Grounded)
                    baseY = -fighter.Definition.HurtboxSize.Y * .5f - paperPose.Foot(sample);
                if (UsesPaperSprite && paperPose != null) baseY += paperPose.OriginY(sample);
                float paperHeight = paperPose == null ? character.PaperMotion ? character.PaperMotion.StandingHeight : 0 : paperPose.Head(sample);
                Vector3 offset = new Vector3(performance.X, performance.Y, 0);
                if (performance.RotateAboutCenter||turnWithRush)
                {
                    Vector3 center = new Vector3(0, paperHeight * .5f, 0);
                    offset += center - body.transform.localRotation * center;
                }
                else if (UsesPaperSprite && paperPose != null && !paperPose.Airborne && performance.Degrees != 0)
                {
                    float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
                    float left = visualSample.FlipX ? -paperPose.FootMax(sample) : paperPose.FootMin(sample);
                    float right = visualSample.FlipX ? -paperPose.FootMin(sample) : paperPose.FootMax(sample);
                    offset.y += Mathf.Max(0, -Mathf.Min(left * sin, right * sin));
                }
                body.transform.localPosition = new Vector3(0, baseY + rock.Lift, 0) + offset;
                float spriteHeight = UsesPaperSprite ? paperHeight * Mathf.Cos(angle * Mathf.Deg2Rad) + rock.Lift + offset.y :
                    character.Animations.HeadHeight(sample) / character.Animations.PixelsPerUnit;
                if (performance.RotateAboutCenter||turnWithRush) spriteHeight = paperHeight + rock.Lift;
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
            if (fighter.Attack != null && fighter.Attack.IsActive && fighter.Attack.Definition.Ability.SuperGuard) body.color = presentation.GuardTint;
            if (fighter.Attack != null && fighter.Attack.Definition.Ability.Conscious) body.color = new Color(.65f, .9f, 1, .7f);
            if (fighter.Mode == FighterMode.Dodge) body.color = presentation.DodgeTint;
            if (fighter.Mode == FighterMode.Counter) body.color = presentation.CounterTint;
            if (fighter.ParalysisRemaining > 0) body.color = Color.Lerp(baseColor, Color.yellow, 0.6f);
            if (fighter.BuffRemaining > 0) body.color = Color.Lerp(body.color, Color.white, 0.3f);
            if (fighter.Mode == FighterMode.KO) body.color = Color.Lerp(baseColor, Color.black, 0.65f);
            if (fighter.HeartRecoveryRemaining > 0 && fighter.HeartRecoveryRemaining / 5 % 2 == 0)
            { var color = body.color; color.a = .35f; body.color = color; }
            if (Time.unscaledTime < flashUntil && Mathf.FloorToInt(Time.unscaledTime * 40) % 2 == 0)
                body.color = UsesAnimationSprites ? new Color(1, 1, 1, presentation.SpriteHitFlashOpacity) : Color.white;
            headY += chargePose.Apply(body, fighter, runner.Simulation.Rules.Charge);
            if(hanging)
            {
                body.transform.localRotation=Quaternion.identity; body.transform.localScale=Vector3.one;
                body.flipX=fighter.Facing<0;
                body.transform.localPosition=new Vector3(fighter.LedgePoint.X-position.X-fighter.LedgeSide*.06f,fighter.LedgePoint.Y-position.Y,0);
                headY=body.bounds.max.y;
            }
            bool generatedTransformation=UsesPaperSprite&&paperPose!=null&&paperPose.Frames.Length==2&&
                paperPose.Frames[1]==body.sprite&&(paperPose.Key=="special"||paperPose.Key=="super");
            abilityCue.Apply(fighter, body, runner.Simulation.Match.IsFinished,UsesPaperSprite?character.PaperMotion:null,sample,generatedTransformation);
            teleportPhase.Apply(fighter, body, runner.Simulation.Match.TickCount, runner.Simulation.Match.IsFinished);
            summonCommand.Apply(fighter,runner.Simulation.Projectiles,headY,runner.Simulation.Match.IsFinished);
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
            bool curryActive=UsesAnimationSprites && fighter.Attack != null && fighter.Attack.Definition.IsUltimate && fighter.Attack.IsActive && wave.sprite &&
                (fighter.Attack.Definition.Ability.Look == AbilityLook.Default || fighter.Attack.Definition.Ability.Look == AbilityLook.Curry);
            int tick=runner.Simulation.Match.TickCount;
            if(curryActive)curryLastActiveTick=tick;
            int trail=character.PaperMotion?character.PaperMotion.CurryAfterglowTicks:0;
            bool curryTrail=wave.sprite&&curryLastActiveTick>=0&&tick>=curryLastActiveTick&&tick-curryLastActiveTick<trail&&
                sample.State==AniBato.Presentation.AnimationState.Ultimate&&!runner.Simulation.Match.IsFinished;
            wave.enabled=curryActive||curryTrail;
            wave.color=new Color(1,1,1,curryActive?1:curryTrail?1-(tick-curryLastActiveTick)/(float)trail:0);
            float waveScale = effects == null ? presentation.CurryWaveScale : effects.UltimateScale;
            Vector2 waveOffset = effects == null ? presentation.CurryWaveOffset : effects.UltimateOffset;
            wave.flipX = sample.FlipX; wave.transform.localScale = new Vector3(waveScale, waveScale, 1);
            wave.transform.localPosition = new Vector3((sample.FlipX ? -1 : 1) * waveOffset.x, waveOffset.y, 0);
            if (wave.enabled && fighter.Attack != null && fighter.Attack.AbilityChargeReleased)
            {
                var box = fighter.Attack.WorldHitbox(fighter); var size = wave.sprite.bounds.size;
                wave.transform.position = new Vector3(box.Center.X, box.Center.Y, 0);
                wave.transform.localScale = new Vector3(box.Size.X/size.x,box.Size.Y/size.y,1);
            }
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


