using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class DirectionalSpecialArtworkBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            const float width = 1426, height = 708;
            var mine = PaperActionArtworkBuilder.Import("directional_minato_mine_v1", "art_source/directional_specials/minato_mine_v1.png",
                "CF4693B31707C0C102978D2E3DC1D6DB6C03765DF870BE1B2ADB8A5AF989D261", 1774, 887, new Vector2(.5f, 1 - 447f / 887));
            var paper = roster[3].PaperMotion;
            paper.Projectiles = (paper.Projectiles ?? System.Array.Empty<PaperProjectileArt>()).Where(p => p.Look != AbilityLook.Mine)
                .Concat(new[] { new PaperProjectileArt { Look = AbilityLook.Mine, Sprite = mine, FitToHitbox = true,
                    VisibleSize = new Vector2(width, height) / PaperActionArtworkBuilder.PixelsPerUnit, Scale = 1 } }).ToArray();
            UnityEditor.EditorUtility.SetDirty(paper);
        }
    }
}
