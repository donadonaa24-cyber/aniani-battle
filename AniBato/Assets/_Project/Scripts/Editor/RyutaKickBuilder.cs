using AniBato.Game;
using UnityEditor;

namespace AniBato.EditorTools
{
    public static class RyutaKickBuilder
    {
        public static void Apply(CharacterData ryuta)
        {
            ryuta.PaperMotion.SuppressNormalAttackOverlays=true;EditorUtility.SetDirty(ryuta.PaperMotion);
            if(ryuta.KickSchemaVersion>=1)return;
            var asset=ryuta.GroundNormals[2];var definition=asset.Definition;definition.Name="前蹴り・三段目";
            asset.Definition=definition;EditorUtility.SetDirty(asset);ryuta.KickSchemaVersion=1;EditorUtility.SetDirty(ryuta);
        }
    }
}
