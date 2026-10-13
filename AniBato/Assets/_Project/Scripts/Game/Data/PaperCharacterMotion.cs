using AniBato.Presentation;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/紙人形の待機・歩行")]
    public sealed class PaperCharacterMotion : ScriptableObject
    {
        public int SchemaVersion;
        public bool Enabled = true;
        public Sprite Sprite;
        public float StandingHeight = 2.24f;
        public PaperRockingSettings Motion;
        public bool Supports(AnimationState state) => Enabled && Sprite && (state == AnimationState.Idle || state == AnimationState.Move);
    }
}
