using AniBato.Battle;
using UnityEngine;
using AniBato.Presentation;

namespace AniBato.Game
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(50)]
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private PresentationSettings presentation;
        private Camera cameraComponent;
        private BattleEffectsView effects;
        private Vector3 previousShake;
        private bool snap = true;
        private float previousAspect;
        private RectTransform hudRect;
        public void Configure(BattleRunner value, PresentationSettings settings) { runner = value; presentation = settings; }
        private void Awake()
        {
            cameraComponent = GetComponent<Camera>(); effects = runner.GetComponent<BattleEffectsView>();
            var hud=Object.FindAnyObjectByType<HUDController>(FindObjectsInactive.Include); if(hud)hudRect=hud.GetComponent<RectTransform>();
        }
        private void OnEnable() { snap = true; if (runner) runner.MatchRestarted += ResetTarget; }
        private void OnDisable() { if (runner) runner.MatchRestarted -= ResetTarget; }
        private void ResetTarget() => snap = true;
        private void LateUpdate()
        {
            if (runner.Simulation == null) return;
            transform.position -= previousShake; previousShake = Vector3.zero;
            if (cameraComponent.aspect != previousAspect) { snap = true; previousAspect = cameraComponent.aspect; }
            if (runner.Paused && !snap) return;
            BattleVector p1 = runner.RenderPosition(0), p2 = runner.RenderPosition(1);
            p1=CameraFraming.Subject(runner.Simulation.Stage,p1,runner.Simulation.P1.MaximumChargeLaunch);
            p2=CameraFraming.Subject(runner.Simulation.Stage,p2,runner.Simulation.P2.MaximumChargeLaunch);
            var frame = CameraFraming.Calculate(runner.Simulation.Stage, p1, p2, cameraComponent.aspect,
                runner.Simulation.Match.TickCount == 0, presentation.CameraMinSize, presentation.CameraPadding,
                presentation.CameraBoundaryInset, presentation.CameraHeadroom);
            bool frozen = runner.Simulation.P1.IsFrozenThisTick || runner.Simulation.P2.IsFrozenThisTick || runner.Simulation.CutInRemainingFrames > 0;
            float factor = snap ? 1 : frozen ? 0 : 1 - Mathf.Exp(-presentation.CameraFollowSpeed * Time.unscaledDeltaTime);
            snap = false;
            transform.position = Vector3.Lerp(transform.position, new Vector3(frame.Center.X, frame.Center.Y, -10), factor);
            cameraComponent.orthographicSize = CameraFraming.ContainingSize(frame,
                new BattleVector(transform.position.x, transform.position.y), cameraComponent.aspect,
                Mathf.Lerp(cameraComponent.orthographicSize, frame.Size, factor));
            if(hudRect && hudRect.rect.height>0 && (!frozen || factor==1))
            {
                float highest=Mathf.Max(p1.Y+runner.Simulation.P1.Definition.HurtboxSize.Y*.5f,
                    p2.Y+runner.Simulation.P2.Definition.HurtboxSize.Y*.5f)+presentation.CounterHeadPadding;
                var safe=CameraFraming.BelowHud(new CameraFrame(new BattleVector(transform.position.x,transform.position.y),cameraComponent.orthographicSize,frame.Required),
                    highest,PaperFighterHud.SafeTopHeight/hudRect.rect.height);
                transform.position=new Vector3(safe.Center.X,safe.Center.Y,transform.position.z);cameraComponent.orthographicSize=safe.Size;
            }
            if (!runner.Paused && effects && effects.ScreenShakeEnabled && effects.Feedback.ShakeRemaining > 0)
            {
                float strength = presentation.ShakeStrength * Mathf.Clamp01(effects.Feedback.ShakeRemaining / presentation.ShakeSeconds);
                previousShake = new Vector3(Mathf.Sin(Time.unscaledTime * 117), Mathf.Cos(Time.unscaledTime * 93), 0) * strength;
                transform.position += previousShake;
            }
        }
    }
}
