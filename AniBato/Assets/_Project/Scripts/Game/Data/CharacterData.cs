using System;
using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/キャラクター")]
    public sealed class CharacterData : ScriptableObject
    {
        public CharacterDefinition Movement;
        public int TempoSchemaVersion;
        public int AccessibilitySchemaVersion;
        public int NormalMotionSchemaVersion;
        public int DamageReactionSchemaVersion;
        public int NormalThrowSchemaVersion;
        public int NormalWeaponSchemaVersion;
        public int MoveSetSchemaVersion;
        public int DirectionalSpecialSchemaVersion;
        public int RecoverySchemaVersion;
        public int AbilityTempoSchemaVersion;
        public int RoyalMarchSchemaVersion;
        public int ProjectileEconomySchemaVersion;
        public int AttackStaminaSchemaVersion;
        public int KickSchemaVersion;
        public int LedgeSchemaVersion;
        public int RefinementSchemaVersion;
        public int ActionArtworkSchemaVersion;
        public AttackData[] DirectionalNormals;
        public AttackData UpNormal;
        public AttackData[] GroundNormals;
        public AttackData[] AirNormals;
        public AttackData[] Specials;
        public AttackData Ultimate;
        public Color Tint = Color.cyan;
        public bool RoundShape;
        public int Silhouette; // 0: equipment, 1: dog ears, 2: chef hat (crossover placeholders).
        public Sprite Portrait;
        public Sprite SelectionSprite;
        public bool PrototypeSharedMoves;
        public bool ActionArtworkPending;
        public CharacterAnimationSet Animations;
        public PaperCharacterMotion PaperMotion;
        public CharacterSpriteEffects SpriteEffects;
        public CharacterDefinition ToDefinition()
        {
            var result = Movement;
            result.GroundNormals = Convert(GroundNormals);
            result.AirNormals = Convert(AirNormals);
            result.DirectionalNormals = DirectionalNormals == null ? null : ConvertDirections(DirectionalNormals);
            result.UpNormal = UpNormal ? UpNormal.ToDefinition() : default;
            result.Specials = Convert(Specials);
            result.Ultimate = Ultimate ? Ultimate.ToDefinition() : throw new ArgumentException("超必殺技が未設定です。");
            return result.Snapshot();
        }
        private static AttackDefinition[] Convert(AttackData[] assets)
        {
            if (assets == null) throw new ArgumentException("攻撃アセットが未設定です。");
            var result = new AttackDefinition[assets.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = assets[i] ? assets[i].ToDefinition() : throw new ArgumentException("通常攻撃アセットが欠落しています。");
            return result;
        }
        private static AttackDefinition[] ConvertDirections(AttackData[] assets)
        {
            var result = new AttackDefinition[assets.Length];
            for (int i = 0; i < assets.Length; i++) if (assets[i]) result[i] = assets[i].ToDefinition();
            return result;
        }
    }
    [Serializable]
    public sealed class CharacterSpriteEffects
    {
        // Inline Unity serialization may materialize an empty object: null alone is not
        // an initialization marker. Version0 keeps the existing presentation fallback.
        public int SchemaVersion;
        public string ProjectileClip, DownSpecialClip, UltimateClip;
        public string NormalProjectileClip;
        public int NormalProjectileSchemaVersion;
        public float NormalProjectileScale = 1f;
        public float ProjectileScale = 0.5f, DownSpecialScale = 1f, UltimateScale = 1f;
        public float DownSpecialOpacity = 0.35f;
        public Vector2 DownSpecialOffset, UltimateOffset;
        public Color SideSpecialTint = Color.white;
    }
}

