using UnityEngine;

namespace AniBato.Game
{
    public static class RuntimeFonts
    {
        private static Font japanese;
        public static Font Japanese
        {
            get
            {
                if (!japanese) japanese = Font.CreateDynamicFontFromOSFont(new[] { "Yu Gothic", "Meiryo", "Arial" }, 28);
                return japanese;
            }
        }
    }
}
