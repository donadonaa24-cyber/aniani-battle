using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/表示設定")]
    public sealed class PresentationSettings : ScriptableObject
    {
        public Color Background = new Color(0.025f, 0.04f, 0.09f);
        public Color MainPlatform = new Color(0.17f, 0.24f, 0.35f);
        public Color UpperPlatform = new Color(0.3f, 0.46f, 0.58f);
        public Color Warning = new Color(1, 0.65f, 0.12f);
        public int CameraSchemaVersion;
        public int TheatreSchemaVersion;
        [Min(0)] public int FinishHoldTicks = 6;
        public float CameraMinSize = 5.2f;
        public float CameraMaxSize = 19;
        public float CameraPadding = 1.2f;
        public float CameraBoundaryInset = 2;
        public float CameraHeadroom = 2.5f;
        public float CameraFollowSpeed = 8;
        public Vector2 CameraXLimits = new Vector2(-8, 8);
        public Vector2 CameraYLimits = new Vector2(-1, 7);
        public float CounterHeight = 1.7f;
        public float CounterHeadPadding = 0.45f;
        public float CounterBounceSeconds = 0.2f;
        public float BreakSeconds = 0.55f;
        public float HitFlashSeconds = 0.18f;
        public Color GuardTint = new Color(0.25f, 0.65f, 1);
        public Color DodgeTint = new Color(0.65f, 1, 0.8f, 0.45f);
        public Color CounterTint = new Color(1, 0.85f, 0.15f);
        public Color ProjectileTint = new Color(1, 0.65f, 0.2f);
        public float PopupSeconds = 0.7f;
        public float PopupHeight = 2.6f;
        public float SpriteHitFlashOpacity = 0.3f;
        public float RiceBallScale = 0.5f;
        public float StatusIconScale = 0.35f;
        public Vector2 StatusIconOffset = new Vector2(0.7f, 0.2f);
        public float CurryWaveScale = 2.5f;
        public Vector2 CurryWaveOffset = new Vector2(1.7f, -0.2f);
        public float DebugLineWidth = 0.035f;
        public bool ScreenShake = true;
        public int MaximumCatchupTicks = 5;
        public float InterpolationDiscontinuityDistance = 5;
        public float ShakeSeconds = .16f;
        public float ShakeStrength = .12f;
        public float SparkSeconds = .22f;
        public float JustFlashSeconds = .25f;
        public float FinishSeconds = 1.1f;
    }
}


