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
            var curry = PaperActionArtworkBuilder.Import("directional_chizuru_curry_residue_v1", "art_source/directional_specials/chizuru_curry_residue_v1.png",
                "275D85C2BFB7B8641750D16CDF342847BAC7A0DBD6C4B3B36FD89F142D22A30A", 2172, 724, new Vector2(1086.5f / 2172, 1 - 389.5f / 724));
            var curryPaper = roster[2].PaperMotion;
            curryPaper.Projectiles = (curryPaper.Projectiles ?? System.Array.Empty<PaperProjectileArt>()).Where(p => p.Look != AbilityLook.CurryResidue)
                .Concat(new[] { new PaperProjectileArt { Look = AbilityLook.CurryResidue, Sprite = curry, FitToHitbox = true,
                    VisibleSize = new Vector2(2081,323) / PaperActionArtworkBuilder.PixelsPerUnit, Scale = 1, SortingOrder = 3 } }).ToArray();
            UnityEditor.EditorUtility.SetDirty(curryPaper);
            Recovery(roster[0], "toma_drone_recovery_v1.png", "1FEAC6F708C94170A3F1A2E89F565FB0E340E025C2D505463A6F531BB9881B3F", 1024, 1536, 570, 1495, 430, 360, 720);
            Recovery(roster[2], "chizuru_balloon_recovery_v2.png", "3D08557C814D5BE2FA36CFD2993DFB3A241067E2B20D050C7C43BAC4C63B5F05", 1024, 1536, 485, 1495, 520, 260, 520);
            Recovery(roster[7], "akatsuki_flap_recovery_v1.png", "02E946160713892FFF62AE999600677C6076BD9F63E6649C978BCA031912314A", 1189, 1323, 620, 1248, 22, 310, 730);
            Recovery(roster[8], "ryuta_book_recovery_v1.png", "27B4AD374A8A4A6BB2E7B4B3644A6E7AC96979AC43CFE36431EDC1185A960D08", 1024, 1536, 560, 1300, 30, 200, 870);
        }
        private static void Recovery(CharacterData data, string file, string sha, int width, int height, int centerX, int feetY, int hairY, int footMinX, int footMaxX)
        {
            const float ppu = PaperActionArtworkBuilder.PixelsPerUnit;
            var sprite = PaperActionArtworkBuilder.Import("directional_" + file.Replace(".png", ""), "art_source/directional_specials/" + file,
                sha, width, height, new Vector2(centerX / (float)width, 1 - feetY / (float)height));
            data.PaperMotion.UpRecovery = new PaperPose { Key = "up_recovery", States = new[] { AniBato.Presentation.AnimationState.SpecialUp },
                Frames = new[] { sprite }, HoldTicks = 8, HeadHeight = (feetY - hairY) / ppu,
                FootMinX = (footMinX - centerX) / ppu, FootMaxX = (footMaxX - centerX) / ppu };
            UnityEditor.EditorUtility.SetDirty(data.PaperMotion);
        }
    }
}
