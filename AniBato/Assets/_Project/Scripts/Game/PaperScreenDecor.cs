using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    // Whole illustrations on sticks. No cut-up joints or bone animation.
    public sealed class PaperScreenDecor : MonoBehaviour
    {
        [SerializeField] private Sprite[] portraits;
        [SerializeField] private PaperEnsembleArt artwork;
        private CanvasGroup ensemble;
        private RectTransform[] figures;
        private GameScreen previous;
        private float fade;
        public Sprite[] Portraits => portraits;
        public PaperEnsembleArt Artwork=>artwork;
        public float EnsembleAlpha=>ensemble?ensemble.alpha:0;
        public void Configure(Sprite[] values,PaperEnsembleArt art=null){portraits=values;artwork=art;}
        private static Image Image(Transform parent,string name,Vector2 p,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchoredPosition=p;r.sizeDelta=size;
            var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return image;
        }
        private void Awake()
        {
            Image(transform,"Paper Stage Header",new Vector2(0,510),new Vector2(1920,60),new Color(.5f,.28f,.23f));
            Image(transform,"Paper Stage Sill",new Vector2(0,-520),new Vector2(1920,40),new Color(.55f,.4f,.27f));
            for(int side=-1;side<=1;side+=2)
            {
                Image(transform,"Paper Side Wing",new Vector2(side*940,0),new Vector2(45,1000),new Color(.42f,.26f,.24f));
                Image(transform,"Paper Tape",new Vector2(side*890,480),new Vector2(100,30),new Color(.86f,.75f,.5f)).rectTransform.localRotation=Quaternion.Euler(0,0,side*25);
            }
            var group=new GameObject("Nine Paper Puppets",typeof(RectTransform),typeof(CanvasGroup));group.transform.SetParent(transform,false);
            ensemble=group.GetComponent<CanvasGroup>();ensemble.blocksRaycasts=false;ensemble.interactable=false;
            if(artwork&&artwork.Artwork)
            {
                ensemble.alpha=0;
                Image(group.transform,"Generated Nine Character Ensemble",PaperEnsembleArt.DisplayPosition,PaperEnsembleArt.DisplaySize,Color.white).sprite=artwork.Artwork;
                figures=System.Array.Empty<RectTransform>();return;
            }
            figures=new RectTransform[portraits.Length];
            float[] xs={0,-660,-480,360,570,760,-280,-820,210};
            float[] ys={-25,-25,-140,-15,-65,-90,-120,-110,5};
            float[] heights={560,430,260,420,400,460,420,440,430};
            string[] names={"透真","湊","シロ","蓮","結衣","黒獅子王","千鶴","暁","龍太"};
            // The lead is drawn last, in front of the supporting row.
            for(int n=portraits.Length-1;n>=0;n--)
            {
                var puppet=new GameObject("Paper Puppet "+names[n],typeof(RectTransform));puppet.transform.SetParent(group.transform,false);
                figures[n]=puppet.GetComponent<RectTransform>();figures[n].anchoredPosition=new Vector2(xs[n],ys[n]);
                Image(puppet.transform,"Puppet Stick",new Vector2(0,-heights[n]*.5f),new Vector2(11,230),new Color(.72f,.56f,.35f));
                var illustration=Image(puppet.transform,"Whole Illustration",Vector2.zero,new Vector2(heights[n]*.9f,heights[n]),Color.white);
                illustration.sprite=portraits[n];illustration.preserveAspect=true;
            }
        }
        public void Render(GameFlowController flow)
        {
            gameObject.SetActive(flow.Screen!=GameScreen.Battle&&flow.Screen!=GameScreen.Countdown&&flow.Screen!=GameScreen.Pause);
            if(!ensemble)return;
            bool title=flow.Screen==GameScreen.Title,top=flow.Screen==GameScreen.MainMenu;
            if(flow.Screen!=previous)
            {if(flow.Screen==GameScreen.CharacterSelect&&previous==GameScreen.MainMenu)fade=.6f;previous=flow.Screen;}
            if(artwork)
            {
                float target=top?1:flow.Screen==GameScreen.CharacterSelect?.66f:0;
                ensemble.alpha=Mathf.MoveTowards(ensemble.alpha,target,Time.unscaledDeltaTime*.85f);
                return;
            }
            if(!title&&!top)fade=Mathf.Max(0,fade-Time.unscaledDeltaTime);
            ensemble.alpha=title?1:top?.22f:fade;
            int pose=Mathf.FloorToInt(Time.unscaledTime*5)%4;
            for(int i=0;i<figures.Length;i++)figures[i].localRotation=Quaternion.Euler(0,0,(pose==0?-1:pose==2?1:0)*(i%2==0?1.5f:-1.5f));
        }
    }
}

