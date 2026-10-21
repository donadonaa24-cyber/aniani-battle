using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class PaperSign : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private RectTransform root;
        [SerializeField] private Vector2 padding = new Vector2(130,45);
        public static PaperSign Attach(Text label, string name, Vector2? textPadding = null)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(label.transform.parent,false);
            go.transform.SetSiblingIndex(label.transform.GetSiblingIndex());
            var sign=go.AddComponent<PaperSign>();sign.text=label;sign.root=go.GetComponent<RectTransform>();
            if(textPadding.HasValue)sign.padding=textPadding.Value;
            Panel(go.transform,"Paper Shadow",new Vector2(8,-10),new Color(.06f,.05f,.08f,.9f),new Vector2(16,16));
            Panel(go.transform,"Paper Cut Edge",Vector2.zero,new Color(.95f,.88f,.68f),Vector2.zero);
            Panel(go.transform,"Paper Ink",Vector2.zero,new Color(.2f,.25f,.32f),new Vector2(-14,-14));
            for(int i=-1;i<=1;i+=2)
            {
                var tape=Panel(go.transform,"Tape",new Vector2(i*170,0),new Color(.9f,.77f,.5f),new Vector2(60,24),false);
                tape.anchorMin=tape.anchorMax=new Vector2(i<0?0:1,.5f);tape.anchoredPosition=Vector2.zero;
                tape.localRotation=Quaternion.Euler(0,0,i*13);
            }
            return sign;
        }
        private static RectTransform Panel(Transform parent,string name,Vector2 p,Color c,Vector2 size,bool stretch=true)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();
            if(stretch){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;}r.anchoredPosition=p;r.sizeDelta=size;
            var image=go.GetComponent<Image>();image.color=c;image.raycastTarget=false;return r;
        }
        private void LateUpdate()
        {
            bool show=text&&text.enabled&&!string.IsNullOrEmpty(text.text);
            foreach(var image in GetComponentsInChildren<Image>())image.enabled=show;
            if(!show)return;
            var source=text.rectTransform;root.anchorMin=source.anchorMin;root.anchorMax=source.anchorMax;root.pivot=source.pivot;root.anchoredPosition=source.anchoredPosition;
            root.sizeDelta=new Vector2(Mathf.Min(source.sizeDelta.x,text.preferredWidth+padding.x),Mathf.Min(source.sizeDelta.y,text.preferredHeight+padding.y));
            root.localRotation=Quaternion.Euler(0,0,-2);root.localScale=source.localScale;
            foreach(var image in GetComponentsInChildren<Image>())if(image.name=="Tape")image.rectTransform.localScale=Vector3.one*Mathf.Clamp(root.sizeDelta.x/400,.25f,1);
        }
    }
}

