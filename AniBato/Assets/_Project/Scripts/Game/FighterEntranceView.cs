using UnityEngine;

namespace AniBato.Game
{
    // The same generated rift as the support cast, applied to one whole standing illustration.
    internal sealed class FighterEntranceView
    {
        private readonly BattleRunner runner;
        private readonly GameFlowController flow;
        private readonly SpriteRenderer rift;
        public FighterEntranceView(Transform parent,BattleRunner runner)
        {
            this.runner=runner;flow=Object.FindAnyObjectByType<GameFlowController>();
            rift=ProceduralSprites.Create(parent,"Fighter Entrance Rift",Vector2.one,Color.white,sortingOrder:4);
            rift.enabled=false;
        }
        public void Apply(SpriteRenderer body,int facing)
        {
            float progress=flow?flow.FighterEntranceProgress:1;
            var art=runner.Setup?runner.Setup.SupportArt:null;
            rift.enabled=progress<1&&art&&art.Rift;
            if(!rift.enabled)return;
            var bounds=body.bounds;rift.sprite=art.Rift;
            rift.transform.position=bounds.center;
            float scale=bounds.size.y*1.2f/art.Rift.bounds.size.y;
            float opening=Mathf.Min(1,progress*5)*Mathf.Clamp01((1-progress)*4);
            rift.transform.localScale=new Vector3(scale*.65f*opening,scale,1);
            rift.color=new Color(1,1,1,.85f);
            // Deliberate stepped whole-card movement, without modifying simulation positions.
            float appear=Mathf.Floor(Mathf.Clamp01((progress-.15f)/.65f)*6)/6;
            var color=body.color;color.a*=appear;body.color=color;
            body.transform.localPosition+=new Vector3(-facing*(1-appear)*.45f,0,0);
        }
    }
}
