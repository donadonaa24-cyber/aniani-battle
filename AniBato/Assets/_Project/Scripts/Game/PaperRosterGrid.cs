using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace AniBato.Game
{
    public sealed class PaperRosterGrid : MonoBehaviour
    {
        private Image[] portraits, backgrounds;
        private Text[] labels;
        private Text status;
        private GameFlowController flow;
        [SerializeField] private PaperEnsembleArt artwork;
        public PaperEnsembleArt Artwork=>artwork;
        public void Configure(PaperEnsembleArt value){artwork=value;}
        public static readonly int[] Layout={6,3,7,5,1,4,0,2,8};
        public static bool UpperRow(int index)=>System.Array.IndexOf(Layout,index)<3;
        public static Vector2 CardPosition(int index)
        {
            int cell=System.Array.IndexOf(Layout,index);
            return cell<3?new Vector2((cell-1)*560,150):new Vector2((cell-3-2.5f)*280,-145);
        }
        public static Vector2 CardSize(int index)=>new Vector2(UpperRow(index)?350:260,250);
        public static int Move(int index,int x,int y)
        {
            int p=System.Array.IndexOf(Layout,index);if(p<0)p=0;
            bool upper=p<3;int start=upper?0:3,count=upper?3:6,col=p-start;
            if(y!=0)
            {
                // Each upper card spans two lower cards. Prefer the right one on a tie.
                return Layout[upper?3+col*2+1:col/2];
            }
            return Layout[start+(col+x%count+count)%count];
        }
        private static Image Picture(Transform parent,string name,Vector2 p,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchoredPosition=p;r.sizeDelta=size;
            var image=go.GetComponent<Image>();image.color=color;image.raycastTarget=false;return image;
        }
        private static Text Label(Transform parent,string name,Vector2 p,Vector2 size,int font)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchoredPosition=p;r.sizeDelta=size;
            var t=go.GetComponent<Text>();t.font=RuntimeFonts.Japanese;t.fontSize=font;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.raycastTarget=false;return t;
        }
        private void Build()
        {
            portraits=new Image[9];backgrounds=new Image[9];labels=new Text[9];
            for(int cell=0;cell<9;cell++)
            {
                int index=Layout[cell];var data=flow.Setup.Character(index);var p=CardPosition(index);var size=CardSize(index);
                backgrounds[index]=Picture(transform,"Roster Card "+index,p,size,new Color(.13f,.17f,.22f));
                var edge=Picture(transform,"Roster Paper Edge "+index,p,size-Vector2.one*8,new Color(.94f,.9f,.8f));
                portraits[index]=Picture(transform,"Roster Illustration "+index,p+new Vector2(0,32),new Vector2(size.x-18,160),Color.white);
                portraits[index].sprite=artwork?artwork.Face(index):data.SelectionSprite;portraits[index].preserveAspect=true;
                Picture(transform,"Roster Name Plate "+index,p+new Vector2(0,-80),new Vector2(size.x-8,82),new Color(.08f,.1f,.15f,.96f));
                labels[index]=Label(transform,"Roster Name "+index,p+new Vector2(0,-80),new Vector2(size.x-12,82),26);
                labels[index].supportRichText=true;
            }
            status=Label(transform,"Roster Selection Status",new Vector2(0,-365),new Vector2(1720,65),25);
            if(artwork)PaperSign.Attach(status,"Team Selection Paper");
        }
        public void Render(GameFlowController value)
        {
            flow = value; if (portraits == null) Build();
            gameObject.SetActive(flow.Screen == GameScreen.CharacterSelect); if (!gameObject.activeSelf) return;
            int cursor = flow.SelectionPlayer == 0 ? flow.Player1Index : flow.Player2Index;
            for (int i = 0; i < portraits.Length; i++)
            {
                bool focus = cursor == i; int first = flow.Selection.Slot(0, i), second = flow.Selection.Slot(1, i);
                backgrounds[i].color = focus ? (flow.SelectionPlayer == 0 ? new Color(.62f,.28f,.22f) : new Color(.15f,.43f,.58f)) : new Color(.13f,.17f,.22f);
                var data = flow.Setup.Character(i);
                string work=i==0||i==3||i==4?"星の終わり":i==1||i==5||i==6?"推し駒Battle":"Battle à la carte";
                labels[i].text = data.Movement.Name + (focus ? " ◀" : "") + "\n<size=16>"+work+"</size>\n<size=18>" +
                    (first >= 0 ? "P1 " + (first + 1) + "番目 " : "") +
                    (second >= 0 ? (flow.CpuMode ? "CPU " : "P2 ") + (second + 1) + "番目" : "") +
                    "</size>";
                if(artwork)portraits[i].color=new Color(1,1,1,focus?1:.7f);
            }
            status.fontSize = 23;
            status.text = Summary(flow, 0) + "　／　" + Summary(flow, 1) + "\n" + flow.SelectionStatus;
        }
        private static string Summary(GameFlowController flow, int player)
        {
            var picks = flow.Selection.Picks(player); string text = player == 0 ? "P1: " : flow.CpuMode ? "CPU: " : "P2: ";
            for (int i = 0; i < flow.Selection.Required; i++) text += (i + 1) + "." + (i < picks.Count ? flow.Setup.Character(picks[i]).Movement.Name : "―") + " ";
            return text;
        }
        private void Update()
        {
            if(!flow||flow.Screen!=GameScreen.CharacterSelect||Mouse.current==null||!Mouse.current.leftButton.wasPressedThisFrame)return;
            var p=Mouse.current.position.ReadValue();
            if(flow.TouchControls&&flow.TouchControls.ContainsButtonPoint(p))return;
            for(int i=0;i<backgrounds.Length;i++)if(RectTransformUtility.RectangleContainsScreenPoint(backgrounds[i].rectTransform,p))
            {flow.SelectRosterCard(i);break;}
        }
    }
}

