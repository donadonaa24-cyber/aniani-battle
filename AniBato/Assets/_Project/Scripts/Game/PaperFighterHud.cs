using AniBato.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class PaperFighterHud : MonoBehaviour
    {
        public const float SafeTopHeight = 420;
        public static readonly Color Ink = new Color(.16f,.13f,.16f,1);
        public static readonly Color Paper = new Color(.98f,.93f,.80f,1);
        public static readonly Color Red = new Color(.74f,.19f,.19f,1);
        public static readonly Color Teal = new Color(.08f,.42f,.39f,1);
        public static readonly Color Link = new Color(.29f,.29f,.59f,1);
        [SerializeField] private Text nameLabel, english, stamina, link, costs, accumulation;
        [SerializeField] private PaperHudShape staminaFill, linkFill;
        [SerializeField] private PaperHudShape[] hearts;
        public Text NameLabel => nameLabel;
        public float StaminaFraction { get; private set; }
        public float LinkFraction { get; private set; }
        public static string EnglishName(string japanese)
        {
            switch(japanese)
            {
                case "透真": return "TOMA"; case "蓮": return "REN"; case "千鶴": return "CHIZURU";
                case "湊": return "MINATO"; case "シロ": return "SHIRO"; case "結衣": case "結一": return "YUI";
                case "黒獅子王": return "KUROSHISHIO"; case "暁": return "AKATSUKI"; case "龍太": return "RYUTA";
                default: return japanese.ToUpperInvariant();
            }
        }
        public static PaperFighterHud Create(Transform parent, int player, int heartCount, Font font)
        {
            var root = new GameObject("P"+(player+1)+" Paper HUD",typeof(RectTransform),typeof(PaperFighterHud));
            root.transform.SetParent(parent,false); var rect = root.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(player,1);
            rect.anchoredPosition=new Vector2(player==0?28:-28,-18); rect.sizeDelta=new Vector2(680,224);
            var hud = root.GetComponent<PaperFighterHud>();
            Shape(root.transform,"Paper Shadow",new Vector2(5,-5),new Vector2(680,224),Ink);
            Shape(root.transform,"Opaque Paper Base",Vector2.zero,new Vector2(680,224),Ink);
            Shape(root.transform,"Cream Card",new Vector2(0,0),new Vector2(672,216),Paper);
            var tape=Shape(root.transform,"Washi Tape",new Vector2(-274,106),new Vector2(94,18),new Color(.76f,.59f,.36f,1));
            tape.rectTransform.localRotation=Quaternion.Euler(0,0,-7);
            var badge=Shape(root.transform,"Player Stamp",new Vector2(-300,62),new Vector2(46,44),player==0?Red:Link);
            Label(badge.transform,"Player",Vector2.zero,new Vector2(46,44),24,"P"+(player+1),Color.white,TextAnchor.MiddleCenter);
            hud.english=Label(root.transform,"English Ruby",new Vector2(-70,91),new Vector2(394,34),20,"",Ink);
            hud.nameLabel=Label(root.transform,"Player "+(player+1)+" HUD",new Vector2(-70,52),new Vector2(394,54),32,"",Ink);
            hud.hearts=new PaperHudShape[heartCount];
            for(int i=0;i<heartCount;i++)
            {
                var center=new Vector2(170+i*52,62);
                Shape(root.transform,"Heart Cut Edge "+i,center+new Vector2(2,-3),new Vector2(47,47),Ink,PaperHudShape.Cut.Heart);
                hud.hearts[i]=Shape(root.transform,"Paper Heart "+i,center,new Vector2(40,40),Red,PaperHudShape.Cut.Heart);
            }
            hud.accumulation=Label(root.transform,"Normal Accumulation",new Vector2(206,22),new Vector2(220,34),22,"",Ink,TextAnchor.MiddleRight);
            Label(root.transform,"ST Label",new Vector2(-292,-16),new Vector2(60,36),25,"ST",Teal);
            Shape(root.transform,"ST Track",new Vector2(-103,-16),new Vector2(274,24),Ink);
            hud.staminaFill=Shape(root.transform,"ST Paper Fill",new Vector2(-103,-16),new Vector2(266,16),Teal,PaperHudShape.Cut.Strip);
            hud.staminaFill.rectTransform.pivot=new Vector2(0,.5f); hud.staminaFill.rectTransform.anchoredPosition=new Vector2(-236,-16);
            hud.stamina=Label(root.transform,"Stamina Value",new Vector2(108,-16),new Vector2(130,40),26,"",Ink,TextAnchor.MiddleRight);
            hud.costs=Label(root.transform,"Normal Costs",new Vector2(-90,-49),new Vector2(460,32),21,"",Ink);
            Label(root.transform,"LINK Label",new Vector2(-282,-80),new Vector2(82,34),23,"LINK",Link);
            Shape(root.transform,"LINK Track",new Vector2(-103,-80),new Vector2(274,24),Ink);
            hud.linkFill=Shape(root.transform,"LINK Paper Fill",new Vector2(-103,-80),new Vector2(266,16),Link,PaperHudShape.Cut.Strip);
            hud.linkFill.rectTransform.pivot=new Vector2(0,.5f); hud.linkFill.rectTransform.anchoredPosition=new Vector2(-236,-80);
            hud.link=Label(root.transform,"Link Value",new Vector2(206,-80),new Vector2(220,38),24,"",Ink,TextAnchor.MiddleRight);
            return hud;
        }
        public void Refresh(FighterState fighter, BattleRuleDefinition rules)
        {
            nameLabel.text=fighter.Definition.Name; english.text=EnglishName(fighter.Definition.Name);
            for(int i=0;i<hearts.Length;i++) hearts[i].color=i<fighter.Hearts?Red:new Color(.76f,.71f,.61f,1);
            accumulation.text="被弾 "+fighter.NormalHitCount+"/"+rules.NormalHitsPerHeart;
            accumulation.color=fighter.NormalHitCount>=rules.NormalHitsPerHeart-1?Red:Ink;
            stamina.text=Mathf.FloorToInt(fighter.Stamina)+"/"+fighter.Definition.Stamina.Maximum;
            costs.text="通常 "+fighter.Definition.Stamina.NormalFirstCost+"/"+fighter.Definition.Stamina.NormalSecondCost+"/"+fighter.Definition.Stamina.NormalThirdCost;
            StaminaFraction=fighter.Definition.Stamina.Maximum>0?Mathf.Clamp01(fighter.Stamina/fighter.Definition.Stamina.Maximum):1;
            staminaFill.rectTransform.sizeDelta=new Vector2(266*StaminaFraction,16);
            staminaFill.color=fighter.StaminaRejectedRemaining>0?Red:Teal;
            link.text="LINK "+fighter.Gauge+"/"+rules.Gauge.Maximum;
            LinkFraction=rules.Gauge.Maximum>0?Mathf.Clamp01(fighter.Gauge/(float)rules.Gauge.Maximum):0;
            linkFill.rectTransform.sizeDelta=new Vector2(266*LinkFraction,16);
        }
        public static PaperHudShape Shape(Transform parent,string name,Vector2 p,Vector2 size,Color tint,PaperHudShape.Cut cut=PaperHudShape.Cut.Card)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(PaperHudShape)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchoredPosition=p; rect.sizeDelta=size;
            var graphic=go.GetComponent<PaperHudShape>(); graphic.Configure(cut,tint); return graphic;
        }
        public static Text Label(Transform parent,string name,Vector2 p,Vector2 size,int fontSize,string value,Color tint,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            var text=go.GetComponent<Text>(); text.rectTransform.anchoredPosition=p; text.rectTransform.sizeDelta=size;
            text.font=RuntimeFonts.Japanese; text.fontSize=fontSize; text.text=value; text.color=tint;
            text.alignment=align; text.raycastTarget=false; text.supportRichText=false; return text;
        }
    }
}
