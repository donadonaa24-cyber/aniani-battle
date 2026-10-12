using AniBato.Battle;
using UnityEngine;

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
        public void Configure(BattleRunner value, PresentationSettings settings) { runner = value; presentation = settings; }
        private void Awake() { cameraComponent = GetComponent<Camera>(); effects = runner.GetComponent<BattleEffectsView>(); }
        private void OnEnable() { if (runner) runner.MatchRestarted += ResetTarget; }
        private void OnDisable() { if (runner) runner.MatchRestarted -= ResetTarget; }
        private void ResetTarget() => snap = true;
        private void LateUpdate()
        {
            if (runner.Simulation == null) return;
            transform.position -= previousShake; previousShake = Vector3.zero;
            if (runner.Paused) return;
            BattleVector p1 = runner.RenderPosition(0), p2 = runner.RenderPosition(1);
            float centerX = Mathf.Clamp((p1.X + p2.X) * 0.5f, presentation.CameraXLimits.x, presentation.CameraXLimits.y);
            float centerY = Mathf.Clamp((p1.Y + p2.Y) * 0.5f + 1.5f, presentation.CameraYLimits.x, presentation.CameraYLimits.y);
            float horizontal = Mathf.Max(Mathf.Abs(p1.X - centerX), Mathf.Abs(p2.X - centerX));
            float vertical = Mathf.Max(Mathf.Abs(p1.Y - centerY), Mathf.Abs(p2.Y - centerY));
            float size = Mathf.Clamp(Mathf.Max((horizontal + presentation.CameraPadding) / cameraComponent.aspect,
                vertical + presentation.CameraPadding), presentation.CameraMinSize, presentation.CameraMaxSize);
            bool frozen = runner.Simulation.P1.IsFrozenThisTick || runner.Simulation.P2.IsFrozenThisTick || runner.Simulation.CutInRemainingFrames > 0;
            float factor = snap ? 1 : frozen ? 0 : 1 - Mathf.Exp(-presentation.CameraFollowSpeed * Time.unscaledDeltaTime);
            snap = false;
            transform.position = Vector3.Lerp(transform.position, new Vector3(centerX, centerY, -10), factor);
            cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, size, factor);
            if (!runner.Paused && effects && effects.ScreenShakeEnabled && effects.Feedback.ShakeRemaining > 0)
            {
                float strength = presentation.ShakeStrength * Mathf.Clamp01(effects.Feedback.ShakeRemaining / presentation.ShakeSeconds);
                previousShake = new Vector3(Mathf.Sin(Time.unscaledTime * 117), Mathf.Cos(Time.unscaledTime * 93), 0) * strength;
                transform.position += previousShake;
            }
        }
    }
}
