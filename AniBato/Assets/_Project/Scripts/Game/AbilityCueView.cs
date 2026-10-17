using AniBato.Battle;
using UnityEngine;
using AniBato.Presentation;

namespace AniBato.Game
{
    // Temporary cues until the dedicated R4 illustrations replace these readable placeholders.
    public sealed class AbilityCueView
    {
        private readonly TextMesh label;
        private readonly SpriteRenderer artwork;
        public AbilityCueView(Transform parent)
        {
            var go = new GameObject("Ability Cue", typeof(TextMesh)); go.transform.SetParent(parent, false);
            label = go.GetComponent<TextMesh>(); label.font = RuntimeFonts.Japanese; label.fontSize = 64; label.characterSize = .08f;
            label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            go.GetComponent<MeshRenderer>().material = label.font.material;
            go.GetComponent<MeshRenderer>().sortingOrder = 4; go.SetActive(false);
            var effect=new GameObject("Paper Ability",typeof(SpriteRenderer));effect.transform.SetParent(parent,false);
            artwork=effect.GetComponent<SpriteRenderer>();artwork.enabled=false;
        }
        public void Apply(FighterState fighter, SpriteRenderer body, bool finished, PaperCharacterMotion paper,
            in AnimationSample sample, bool generatedTransformation=false)
        {
            var attack = finished ? null : fighter.Attack;
            var art=paper?(paper.AbilityEffect(sample.State)??(attack==null?null:paper.AbilityEffect(attack.Definition.Ability.Look))):null;
            artwork.enabled=!finished&&art!=null&&art.Visible(sample);
            if(art!=null)
            {
                artwork.sprite=art.SpriteFor(sample);artwork.color=Color.white;artwork.flipX=body.flipX;
                artwork.sortingOrder=body.sortingOrder-1;
                artwork.transform.position=body.bounds.center+new Vector3((body.flipX?-1:1)*art.Offset.x,art.Offset.y,0);
                artwork.transform.localScale=Vector3.one*art.ScaleFor(sample);
                float turn=sample.AttackFrame/Mathf.Max(1,art.StepTicks)%2==0?-art.RockDegrees:art.RockDegrees;
                artwork.transform.localRotation=Quaternion.Euler(0,0,turn);
            }
            string text = "";
            bool binary=artwork.enabled&&art.Anticipating(sample)&&!string.IsNullOrEmpty(art.AnticipationText);
            if(binary)text=sample.AttackFrame/Mathf.Max(1,art.StepTicks)%2==0||string.IsNullOrEmpty(art.AlternateAnticipationText)?art.AnticipationText:art.AlternateAnticipationText;
            if (attack != null && art == null)
            {
                switch (attack.Definition.Ability.Look)
                {
                    case AbilityLook.Alma: text = attack.IsDamageActive ? "ALMA!" : attack.Frame / 6 % 2 == 0 ? "0101 1010\n1010 0101" : "1010 0101\n0101 1010"; break;
                    case AbilityLook.Rook: text = generatedTransformation ? "" : "飛"; break;
                    case AbilityLook.Bishop: text = generatedTransformation ? "" : "角"; break;
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
            label.characterSize=binary?art.AnticipationTextSize:.08f;
            label.color = new Color(.55f,.95f,1,.85f);
            label.transform.localPosition = new Vector3(0, fighter.Definition.HurtboxSize.Y * .65f, 0);
            if(binary)label.transform.position=body.bounds.center;
        }
    }
}
