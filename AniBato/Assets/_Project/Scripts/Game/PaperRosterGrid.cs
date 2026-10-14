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
        public static readonly int[] Layout={0,1,2,3,5,7,4,6,8};
        public static int Move(int index,int x,int y)
        {
            int p=System.Array.IndexOf(Layout,index);if(p<0)p=0;
            int col=(p%3+x+3)%3,row=(p/3+y+3)%3;return Layout[row*3+col];
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
            string[] works={"星の終わりに君は生きる","推し駒Battle","Battle à la carte"};
            Color[] colors={new Color(.24f,.39f,.5f),new Color(.48f,.32f,.34f),new Color(.45f,.4f,.27f)};
            for(int c=0;c<3;c++)
            {
                var panel=Picture(transform,"Work Paper "+c,new Vector2((c-1)*565,0),new Vector2(535,670),colors[c]);
                panel.rectTransform.localRotation=Quaternion.Euler(0,0,c==1?-.5f:.5f);
                Label(transform,"Work Title "+c,new Vector2((c-1)*565,300),new Vector2(530,65),26).text=works[c];
            }
            for(int cell=0;cell<9;cell++)
            {
                int index=Layout[cell];var data=flow.Setup.Character(index);float x=(cell%3-1)*565,y=167-cell/3*200;
                backgrounds[index]=Picture(transform,"Roster Card "+index,new Vector2(x,y),new Vector2(500,190),new Color(.13f,.17f,.22f));
                portraits[index]=Picture(transform,"Roster Illustration "+index,new Vector2(x-130,y+5),new Vector2(175,180),Color.white);
                portraits[index].sprite=data.SelectionSprite;portraits[index].preserveAspect=true;
                labels[index]=Label(transform,"Roster Name "+index,new Vector2(x+93,y),new Vector2(270,160),26);
            }
            status=Label(transform,"Roster Selection Status",new Vector2(0,-365),new Vector2(1720,65),25);
        }
        public void Render(GameFlowController value)
        {
            flow=value;if(portraits==null)Build();
            gameObject.SetActive(flow.Screen==GameScreen.CharacterSelect);if(!gameObject.activeSelf)return;
            for(int i=0;i<portraits.Length;i++)
            {
                bool p1=flow.Player1Index==i,p2=flow.Player2Index==i;
                backgrounds[i].color=p1&&p2?new Color(.45f,.3f,.52f):p1?new Color(.62f,.28f,.22f):p2?new Color(.15f,.43f,.58f):new Color(.13f,.17f,.22f);
                var data=flow.Setup.Character(i);
                labels[i].text=data.Movement.Name+"\n"+(p1?"P1"+(flow.Ready(0)?" 決定":" 選択"):"")+(p1&&p2?" / ":"")+
                    (p2?(flow.CpuMode?"CPU":"P2")+(flow.Ready(1)?" 決定":" 選択"):"")+(data.PrototypeSharedMoves?"\n基本技を共有・試作":"");
            }
            status.text=flow.CpuMode?"編集中： "+(flow.CpuRow==0?"P1":flow.CpuRow==1?"CPU":"難易度")+"　／　上下で編集先　左右または絵をクリックで選択　／　決定でステージへ":
                "赤：P1　青：P2　重なると紫　／　上下左右で選択、各自の決定ボタンで準備完了";
        }
        private void Update()
        {
            if(!flow||flow.Screen!=GameScreen.CharacterSelect||Mouse.current==null||!Mouse.current.leftButton.wasPressedThisFrame)return;
            var p=Mouse.current.position.ReadValue();
            for(int i=0;i<backgrounds.Length;i++)if(RectTransformUtility.RectangleContainsScreenPoint(backgrounds[i].rectTransform,p))
            {flow.SelectRosterCard(i);break;}
        }
    }
}

