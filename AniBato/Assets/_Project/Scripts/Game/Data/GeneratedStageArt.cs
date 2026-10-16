using UnityEngine;
namespace AniBato.Game
{
    [CreateAssetMenu(menuName="あにバト/生成した紙舞台")]
    public sealed class GeneratedStageArt : ScriptableObject
    {
        public Sprite Far, Mid, Frame, Main, Upper, Warning, Removal;
        public Rect MainRegion, UpperRegion, WarningRegion, RemovalRegion, BoardPanel;
        public Material Cutout;
        public bool Complete => Far && Mid && Frame && Main && Upper && Warning && Removal && Cutout;
        public static Sprite Region(Sprite source,Rect rect,Vector2 pivot)
        {
            var result=Sprite.Create(source.texture,rect,pivot,100,0,SpriteMeshType.FullRect);
            result.name=source.name+" region";return result;
        }
    }
}
