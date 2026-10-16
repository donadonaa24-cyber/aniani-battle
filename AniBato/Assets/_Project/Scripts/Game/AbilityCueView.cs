using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    // Temporary cues until the dedicated R4 illustrations replace these readable placeholders.
    public sealed class AbilityCueView
    {
        private readonly TextMesh label;
        public AbilityCueView(Transform parent)
        {
            var go = new GameObject("Ability Cue", typeof(TextMesh)); go.transform.SetParent(parent, false);
            label = go.GetComponent<TextMesh>(); label.font = RuntimeFonts.Japanese; label.fontSize = 64; label.characterSize = .08f;
            label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            go.GetComponent<MeshRenderer>().material = label.font.material;
            go.GetComponent<MeshRenderer>().sortingOrder = 4; go.SetActive(false);
        }
        public void Apply(FighterState fighter, SpriteRenderer body, bool finished, bool generatedTransformation=false)
        {
            var attack = finished ? null : fighter.Attack; string text = "";
            if (attack != null)
            {
                switch (attack.Definition.Ability.Look)
                {
                    case AbilityLook.Alma: text = attack.IsDamageActive ? "ALMA!" : attack.Frame / 6 % 2 == 0 ? "0101 1010\n1010 0101" : "1010 0101\n0101 1010"; break;
                    case AbilityLook.Rook: text = generatedTransformation ? "" : "飛"; break;
                    case AbilityLook.Bishop: text = "角"; break;
                    case AbilityLook.King: text = "王"; break;
                    case AbilityLook.Emp: text = "電磁パルス"; break;
                    case AbilityLook.Teleport: text = attack.IsActive ? "瞬移" : "接続…"; break;
                    case AbilityLook.Spell: text = "ABC…?!"; break;
                    case AbilityLook.Bat: text = "バット！"; break;
                    case AbilityLook.BlackHole:
                        text = "意識空間"; var color = body.color; color.a *= .6f; body.color = color; break;
                }
            }
            label.gameObject.SetActive(text.Length > 0); label.text = text;
            label.color = new Color(.55f,.95f,1,.85f);
            label.transform.localPosition = new Vector3(0, fighter.Definition.HurtboxSize.Y * .65f, 0);
        }
    }
}
