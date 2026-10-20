using System.Collections.Generic;
using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    public sealed class SummonCommandView
    {
        private readonly SpriteRenderer bubble,tail;
        private readonly TextMesh words;
        public SummonCommandView(Transform parent)
        {
            bubble=ProceduralSprites.Create(parent,"Royal Command Bubble",new Vector2(3.2f,.7f),new Color(1,.97f,.87f),true,12);
            tail=ProceduralSprites.Create(parent,"Royal Command Tail",Vector2.one*.22f,new Color(1,.97f,.87f),sortingOrder:12);
            var go=new GameObject("Royal Command Words",typeof(TextMesh));go.transform.SetParent(parent,false);
            words=go.GetComponent<TextMesh>();words.text="突撃しろ！！";words.font=RuntimeFonts.Japanese;words.fontSize=64;words.characterSize=.075f;
            words.anchor=TextAnchor.MiddleCenter;words.alignment=TextAlignment.Center;words.color=new Color(.16f,.07f,.2f);
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=words.font.material;renderer.sortingOrder=13;
            bubble.enabled=tail.enabled=false;go.SetActive(false);
        }
        public void Apply(FighterState owner,IReadOnlyList<Projectile> shots,float headY,bool finished)
        {
            bool command=false;
            if(!finished)foreach(var shot in shots)if(shot.Owner==owner&&shot.MarchCommand){command=true;break;}
            bubble.enabled=tail.enabled=command;words.gameObject.SetActive(command);if(!command)return;
            var center=new Vector3(owner.Position.X,headY+.65f,0);bubble.transform.position=center;words.transform.position=center;
            tail.transform.position=center+new Vector3(-.5f,-.3f,0);tail.transform.rotation=Quaternion.Euler(0,0,45);
        }
    }
}
