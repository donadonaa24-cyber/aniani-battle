using UnityEngine;

namespace AniBato.Game
{
    // The camera world is warped before the overlay canvas draws the paper HUD.
    public sealed class SupportFieldFilter : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private SupportView view;
        [SerializeField] private Shader shader;
        private Material material;
        public void Configure(BattleRunner value,SupportView support,Shader effect){runner=value;view=support;shader=effect;}
        private void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            var sim=runner?runner.Simulation:null;
            bool active=sim!=null&&!sim.Match.IsFinished&&view&&view.isActiveAndEnabled&&
                (sim.P1.SupportSlowRemaining>0||sim.P2.SupportSlowRemaining>0);
            if(!active||!shader){Graphics.Blit(source,destination);return;}
            if(!material)material=new Material(shader);
            material.SetFloat("_Phase",sim.Match.TickCount/60f);
            Graphics.Blit(source,destination,material);
        }
        private void OnDestroy(){if(material)Destroy(material);}
    }
}
