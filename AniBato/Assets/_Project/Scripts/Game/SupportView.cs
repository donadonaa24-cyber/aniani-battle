using System.Collections.Generic;
using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    // Whole illustrations appear from a rift; no joints or generated intermediate poses.
    public sealed class SupportView : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        private readonly List<SpriteRenderer> actors=new List<SpriteRenderer>(),rifts=new List<SpriteRenderer>();
        private SpriteRenderer pickup,wallet;
        private readonly SpriteRenderer[] barriers=new SpriteRenderer[2];
        public void Configure(BattleRunner value) { runner=value; }
        private SpriteRenderer Make(string label,int order) => ProceduralSprites.Create(transform,label,Vector2.one,Color.white,sortingOrder:order);
        private static float Scale(SupportSprite art) => art.Height/art.VisibleSize.y;
        private void DrawPickup(ref SpriteRenderer renderer,SupportPickup item,PaperSupportArt art)
        {
            if(!renderer)renderer=Make(item!=null&&item.IsWallet?"Support Wallet":"Support Pickup",5);
            renderer.enabled=item!=null;
            if(item==null)return;
            string key=item.IsWallet?"wallet":PaperSupportArt.PickupKey(item.Kind);
            var entry=key=="card"?null:art.Find(key);
            renderer.sprite=entry==null?art.Card:entry.Sprite;
            renderer.color=Color.white; renderer.flipX=false;
            // Pickup hitboxes and generated objects have a common readable height.
            renderer.transform.localScale=Vector3.one*(entry==null ? .8f/renderer.sprite.bounds.size.y : Scale(entry));
            float half=(item.IsWallet?runner.Simulation.Rules.Support.WalletSize:runner.Simulation.Rules.Support.PickupSize)*.5f;
            renderer.transform.position=new Vector3(item.Position.X,item.Position.Y-(key=="card"?0:half),0);
            renderer.transform.localRotation=Quaternion.Euler(0,0,runner.Simulation.Match.TickCount/10%2==0?-4:4);
        }
        private void LateUpdate()
        {
            var sim=runner.Simulation; var art=runner.Setup.SupportArt;
            if(sim==null || !art)return;
            bool live=!sim.Match.IsFinished && sim.Rules.Support.Enabled;
            DrawPickup(ref pickup,live?sim.Supports.Pickup:null,art); DrawPickup(ref wallet,live?sim.Supports.Wallet:null,art);
            var people=sim.Supports.Actors;var rules=sim.Rules.Support;
            while(actors.Count<people.Count){actors.Add(Make("Support Character",3));rifts.Add(Make("Support Rift",2));}
            for(int i=0;i<actors.Count;i++)
            {
                var sprite=actors[i]; var rift=rifts[i];sprite.enabled=rift.enabled=live&&i<people.Count;
                if(!sprite.enabled)continue;
                var actor=people[i]; var entry=art.Find(PaperSupportArt.ActorKey(actor.Kind));
                float enter=Mathf.Clamp01(actor.Age/(float)rules.PortalFrames);
                float exit=Mathf.Clamp01((actor.Age-rules.PortalFrames-rules.ActorHoldFrames)/(float)rules.ExitFrames);
                bool car=actor.Kind==SupportKind.ShionCar;
                sprite.enabled=!car || actor.Age<rules.PortalFrames;
                sprite.sprite=actor.Kind==SupportKind.Hota&&actor.Age>=rules.PortalFrames?art.Find("pickup_pawn").Sprite:entry.Sprite;
                var enemy=actor.Owner==sim.P1?sim.P2:sim.P1;
                int aim=actor.AttackFacing!=0?actor.AttackFacing:enemy.Position.X>=actor.Position.X?1:-1;
                sprite.color=new Color(1,1,1,enter*(1-exit));sprite.flipX=(actor.Kind==SupportKind.Mai?aim:actor.Facing)<0;
                sprite.transform.position=new Vector3(actor.Position.X-actor.Facing*(1-enter)*.45f,actor.Position.Y,0);
                sprite.transform.localRotation=Quaternion.Euler(0,0,actor.Age/8%2==0?-2:2);
                float height=actor.Kind==SupportKind.Hota&&actor.Age>=rules.PortalFrames?1.2f:entry.Height;
                float visibleHeight=actor.Kind==SupportKind.Hota&&actor.Age>=rules.PortalFrames?art.Find("pickup_pawn").VisibleSize.y:entry.VisibleSize.y;
                sprite.transform.localScale=Vector3.one*(height/visibleHeight);
                rift.sprite=art.Rift;rift.color=new Color(1,1,1,.85f);
                rift.transform.position=new Vector3(actor.Position.X,actor.Position.Y+entry.Height*.5f,0);
                float opening=car?Mathf.Clamp01((rules.PortalFrames+rules.ExitFrames-actor.Age)/(float)rules.ExitFrames):Mathf.Min(1,enter*3)*(1-exit);
                rift.enabled=opening>0&&(enter<1||exit>0||car);
                float scale=entry.Height*1.15f/art.Rift.bounds.size.y;
                rift.transform.localScale=new Vector3(scale*.6f*opening,scale,1);
            }
            for(int i=0;i<2;i++)
            {
                if(!barriers[i])barriers[i]=Make("Support Barrier P"+(i+1),6);
                var f=i==0?sim.P1:sim.P2;var sprite=barriers[i];sprite.enabled=live&&f.SupportBarrierRemaining>0&&f.SupportBarrierUnits>0;
                if(!sprite.enabled)continue;
                sprite.sprite=art.Find("mio_barrier").Sprite;sprite.color=new Color(1,1,1,.38f);
                sprite.transform.position=new Vector3(f.Position.X,f.Position.Y-f.Definition.HurtboxSize.Y*.65f,0);
                sprite.transform.localScale=Vector3.one*(f.Definition.HurtboxSize.Y*1.35f/art.Find("mio_barrier").VisibleSize.y);
            }
        }
    }
}
