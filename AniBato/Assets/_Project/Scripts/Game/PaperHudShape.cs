using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    // UI cut-outs stay crisp at any canvas scale; they do not need bitmap textures.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PaperHudShape : MaskableGraphic
    {
        public enum Cut { Card, Heart, Strip }
        [SerializeField] private Cut cut;
        public void Configure(Cut value, Color tint) { cut = value; color = tint; raycastTarget = false; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r = rectTransform.rect;
            Vector2[] points;
            if (cut == Cut.Heart)
                points = new[] { new Vector2(0,-.48f), new Vector2(-.48f,.02f), new Vector2(-.49f,.25f),
                    new Vector2(-.32f,.46f), new Vector2(-.14f,.45f), new Vector2(0,.28f),
                    new Vector2(.14f,.45f), new Vector2(.32f,.46f), new Vector2(.49f,.25f), new Vector2(.48f,.02f) };
            else if(cut==Cut.Strip)
                points=new[]{new Vector2(-.5f,-.5f),new Vector2(-.43f,.5f),new Vector2(.5f,.5f),new Vector2(.43f,-.5f)};
            else
                points = new[] { new Vector2(-.5f,-.46f), new Vector2(-.5f,.43f), new Vector2(-.48f,.5f),
                    new Vector2(.47f,.5f), new Vector2(.5f,.44f), new Vector2(.5f,-.43f),
                    new Vector2(.48f,-.5f), new Vector2(-.47f,-.5f) };
            var center = UIVertex.simpleVert; center.color = color; center.position = r.center; mesh.AddVert(center);
            for (int i = 0; i < points.Length; i++)
            {
                var vertex = UIVertex.simpleVert;
                vertex.color=cut==Cut.Strip ? points[i].y>0?Color.Lerp(color,Color.white,.25f):Color.Lerp(color,Color.black,.2f) : color;
                vertex.position = r.center + Vector2.Scale(points[i],r.size); mesh.AddVert(vertex);
            }
            for (int i = 0; i < points.Length; i++) mesh.AddTriangle(0, i+1, (i+1)%points.Length+1);
        }
    }
}
