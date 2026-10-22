using AniBato.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class SupportNoticeHud : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private Text[] labels;
        [SerializeField] private GameObject[] papers;
        private readonly float[] until=new float[2];
        public void Configure(BattleRunner value)
        {
            runner=value;labels=new Text[2];papers=new GameObject[2];
            for(int i=0;i<2;i++)
            {
                var card=PaperFighterHud.Shape(transform,"P"+(i+1)+" Support Explanation",Vector2.zero,new Vector2(1240,70),PaperFighterHud.Paper);
                card.rectTransform.anchorMin=card.rectTransform.anchorMax=card.rectTransform.pivot=new Vector2(.5f,0);
                card.rectTransform.anchoredPosition=new Vector2(0,112+i*76);
                labels[i]=PaperFighterHud.Label(card.transform,"Support Effect Text",Vector2.zero,new Vector2(1200,66),22,"",PaperFighterHud.Ink,TextAnchor.MiddleCenter);
                papers[i]=card.gameObject;papers[i].SetActive(false);
            }
        }
        private void OnEnable(){if(runner){runner.EventProduced+=Notify;runner.MatchRestarted+=Clear;}}
        private void OnDisable(){if(runner){runner.EventProduced-=Notify;runner.MatchRestarted-=Clear;}}
        private void Clear(){for(int i=0;i<2;i++){until[i]=0;if(papers!=null)papers[i].SetActive(false);}}
        private void Notify(BattleEvent value)
        {
            bool wallet=value.Kind==BattleEventKind.SupportWalletCollected;
            if(!wallet&&value.Kind!=BattleEventKind.SupportCollected)return;
            int index=value.SourcePlayer;if(index<0||index>1)return;
            var kind=(SupportKind)value.Value;var fighter=index==0?runner.Simulation.P1:runner.Simulation.P2;
            labels[index].text="P"+(index+1)+" "+fighter.Definition.Name+"｜"+(wallet?"財布":PaperSupportArt.DisplayName(kind))+"\n"+Description(kind,runner.Simulation.Rules.Support,wallet);
            until[index]=Time.unscaledTime+4.5f;papers[index].SetActive(true);
        }
        private void LateUpdate(){for(int i=0;i<2;i++)if(papers!=null)papers[i].SetActive(Time.unscaledTime<until[i]);}
        public static string Description(SupportKind kind,SupportDefinition rules,bool wallet=false)
        {
            if(wallet)return "スタミナを全回復！";
            string seconds=(rules.EffectFrames/60f).ToString("0.#")+"秒";
            switch(kind)
            {
                case SupportKind.Alma:return seconds+"間、相手の移動速度が"+(rules.SlowMultiplier*100).ToString("0")+"％に！ 攻撃速度はそのまま";
                case SupportKind.ShionCar:return "車に当たった相手を"+(rules.CarParalysisFrames/60f).ToString("0.#")+"秒間マヒ！";
                case SupportKind.SakiHana:return seconds+"間、スタミナを消費しない！";
                case SupportKind.Sara:return "ハートを"+rules.HealHearts+"個回復！（最大値まで）";
                case SupportKind.Mai:return "矢が命中すると相手のハートを1個減らす！";
                case SupportKind.Tsuyoshi:return "財布を落とす！ 先に触れた人のスタミナが全回復";
                case SupportKind.Mio:return seconds+"間バリア！ 通常"+rules.BarrierUnits+"回・特殊1回分を防ぐ（超必殺は貫通）";
                case SupportKind.Hota:return "歩が"+rules.PawnCount+"個降る！ 相手に当たるたびスタミナ−"+rules.PawnStaminaDamage;
                default:return "";
            }
        }
    }
}
