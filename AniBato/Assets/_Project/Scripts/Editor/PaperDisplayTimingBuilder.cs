using AniBato.Game;
using UnityEditor;
namespace AniBato.EditorTools
{
    public static class PaperDisplayTimingBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            foreach(var character in roster)
            {
                var paper=character.PaperMotion;if(!paper||paper.DisplayTimingSchemaVersion>=1)continue;
                paper.UltimateImpactMinimumTicks=96;paper.CurryAfterglowTicks=24;
                paper.DisplayTimingSchemaVersion=1;EditorUtility.SetDirty(paper);
            }
        }
    }
}
