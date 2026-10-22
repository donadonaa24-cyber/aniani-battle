using System;
using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    [Serializable] public sealed class SupportSprite
    {
        public string Key;
        public Sprite Sprite;
        public float Height;
        public Vector2 VisibleSize;
    }
    [CreateAssetMenu(menuName="あにバト/サポート原画")]
    public sealed class PaperSupportArt : ScriptableObject
    {
        public SupportSprite[] Entries;
        public Sprite Rift, Card;
        public Shader FieldShader;
        public SupportSprite Find(string key) => Array.Find(Entries,e=>e.Key==key);
        public static string ActorKey(SupportKind kind) => new[]{"alma","shion_car","saki_hana","sara","mai","tsuyoshi","mio","hota"}[(int)kind];
        public static string DisplayName(SupportKind kind) => new[]{"ALMA","志遠","咲と花","紗良","舞依","剛","美桜","歩太"}[(int)kind];
        public static string PickupName(SupportKind kind) => (int)kind<4?"本":(int)kind<6?"カード":kind==SupportKind.Mio?"銀":"歩";
        public static string PickupKey(SupportKind kind) => (int)kind<4?"pickup_book":(int)kind<6?"card":kind==SupportKind.Mio?"pickup_silver":"pickup_pawn";
    }
}
