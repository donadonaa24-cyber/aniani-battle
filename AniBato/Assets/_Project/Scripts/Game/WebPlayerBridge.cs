using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
namespace AniBato.Game
{
    public sealed class WebPlayerBridge : MonoBehaviour
    {
        private GameFlowController flow;
        private InputController input;
        private float next;
        private int frames;
        private float elapsed;
        private void Start()
        {
            flow=FindAnyObjectByType<GameFlowController>();input=FindAnyObjectByType<InputController>();
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput=false;
#endif
        }
        public void BeginAudio()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            AniBatoResumeAudio();
#endif
        }
        public void BrowserFocus(string value)
        {
            if(!WebInputPolicy.Enabled)return;
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput=value=="1";
#endif
            if(value!="1")BrowserPause();input?.Clear();
        }
        public void BrowserPause()
        {
            if(flow)flow.SendMessage("OnApplicationFocus",false);input?.Clear();
        }
        private void Update()
        {
            if(!WebInputPolicy.Enabled||!flow)return;
            frames++;elapsed+=Time.unscaledDeltaTime;if(Time.unscaledTime<next)return;next=Time.unscaledTime+.3f;
            var sim=flow.Runner.Simulation;
            var status=new Status{
                screen=flow.Screen.ToString(),menu=flow.MenuIndex,player1=flow.Player1Index,player2=flow.Player2Index,selectionPlayer=flow.SelectionPlayer,
                picks1=string.Join(",",flow.Selection.Picks(0)),picks2=string.Join(",",flow.Selection.Picks(1)),stage=flow.StageIndex,
                tick=sim?.Match.TickCount??0,remaining=sim?.Match.RemainingSeconds??0,shrinks=sim?.Shrink.CompletedSteps??0,
                p1x=sim?.P1.Position.X??0,p2x=sim?.P2.Position.X??0,p1mode=sim?.P1.Mode.ToString()??"",p2mode=sim?.P2.Mode.ToString()??"",
                hearts1=sim?.P1.Hearts??0,hearts2=sim?.P2.Hearts??0,result=sim?.Match.Result.ToString()??"",reason=sim?.Match.EndReason.ToString()??"",
                round=flow.Series?.RoundNumber??0,score1=flow.Series?.Score(0)??0,score2=flow.Series?.Score(1)??0,
                bgm=flow.Audio?flow.Audio.BgmVolume:0,se=flow.Audio?flow.Audio.SeVolume:0,fps=frames/Mathf.Max(.001f,elapsed),pads=Gamepad.all.Count,
                attackKey=input.EffectiveKeys(0).Attack.ToString(),practice=flow.PracticeMode,practicePaused=flow.Runner.PracticePlaybackPaused
            };
#if UNITY_WEBGL && !UNITY_EDITOR
            AniBatoStatus(JsonUtility.ToJson(status));
#endif
        }
        [Serializable] private sealed class Status
        {
            public string screen,picks1,picks2,p1mode,p2mode,result,reason,attackKey;
            public int menu,player1,player2,selectionPlayer,stage,tick,shrinks,hearts1,hearts2,round,score1,score2,pads;
            public double remaining;public float p1x,p2x,bgm,se,fps;public bool practice,practicePaused;
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void AniBatoResumeAudio();
        [DllImport("__Internal")] private static extern void AniBatoStatus(string status);
#endif
    }
}
