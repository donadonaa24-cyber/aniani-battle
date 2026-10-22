using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    public sealed class OnlineBattleBridge : MonoBehaviour
    {
        [Serializable] private sealed class StartData { public int player, stage; public int[] first, second; public uint seed; }
        private GameFlowController flow;
        private int round;
        private float waitingSince;
        private readonly Dictionary<int,string> localHashes = new Dictionary<int,string>();
        private readonly Dictionary<int,string> remoteHashes = new Dictionary<int,string>();
        public OnlineInputBuffer Inputs { get; private set; }
        public bool Active { get; private set; }
        public int LocalPlayer { get; private set; }
        public uint Seed { get; private set; }
        public bool Waiting { get; private set; }
        public string Notice { get; private set; } = "";
        public void Configure(GameFlowController controller) { flow = controller; gameObject.name = "OnlineBridge"; }
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void AniBatoOnlineSend(string value);
        [DllImport("__Internal")] private static extern void AniBatoOnlineUi(string value);
#endif
        public static void Ui(string value)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            AniBatoOnlineUi(value);
#endif
        }
        private static void Send(string value)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            AniBatoOnlineSend(value);
#endif
        }
        public void Begin(string json)
        {
            try
            {
                if (Active || json == null || json.Length > 600) throw new FormatException();
                var data = JsonUtility.FromJson<StartData>(json);
                ValidateTeam(data.first); ValidateTeam(data.second);
                if (data.player < 0 || data.player > 1 || data.stage < 0 || data.stage >= flow.Setup.Stages.Length) throw new FormatException();
                LocalPlayer = data.player; Seed = data.seed; round = 0; Active = true; Notice = "";
                flow.BeginOnlineMatch(data.first, data.second, data.stage);
            }
            catch (Exception) { Fail("対戦開始データが不正です。接続し直してください。"); }
        }
        private void ValidateTeam(int[] team)
        {
            if (team == null || team.Length != TeamSeries.TeamSize) throw new FormatException();
            var unique = new HashSet<int>(); foreach (int id in team) if (id < 0 || id >= flow.Setup.Characters.Length || !unique.Add(id)) throw new FormatException();
        }
        public void NewRound(int number)
        {
            // Transport epochs advance even when a draw replays the same displayed round,
            // or a completed series restarts. Delayed packets from that match stay stale.
            round++; Inputs = new OnlineInputBuffer(LocalPlayer); Waiting = false; waitingSince = 0;
            localHashes.Clear(); remoteHashes.Clear(); Send("round|" + number);
        }
        public void Queue(in FrameInput input)
        {
            int frame = Inputs.LocalTarget; Inputs.Add(LocalPlayer, frame, input);
            Send("i|" + round + "|" + OnlineInputCodec.Encode(frame, input));
        }
        public void Packet(string value)
        {
            if (!Active) return;
            try
            {
                if (value == null || value.Length > 750) throw new FormatException();
                var p = value.Split('|');
                if (p.Length != 3 || !int.TryParse(p[1], out int packetRound)) throw new FormatException();
                if (packetRound != round) return;
                if (p[0] == "i") { var input = OnlineInputCodec.Decode(p[2], out int frame); Inputs.Add(1 - LocalPlayer, frame, input); }
                else if (p[0] == "h")
                {
                    var pair = p[2].Split(','); if (pair.Length != 2 || pair[1].Length != 16 || !int.TryParse(pair[0], out int frame) || frame < Inputs.NextFrame - 600 || frame > Inputs.NextFrame + 180 || remoteHashes.Count > 8) throw new FormatException();
                    remoteHashes[frame] = pair[1]; Compare(frame);
                }
                else throw new FormatException();
            }
            catch (Exception) { Fail("通信データに問題があります。対戦を終了しました。"); }
        }
        public void AfterTick()
        {
            int frame = Inputs.NextFrame;
            if (frame % 120 != 0) return;
            string hash = OnlineStateHash.Compute(flow.Runner.Simulation); localHashes[frame] = hash;
            Send("h|" + round + "|" + frame + "," + hash); Compare(frame);
            var old = new List<int>(); foreach (int key in localHashes.Keys) if (key < frame - 600) old.Add(key); foreach (int key in old) localHashes.Remove(key);
        }
        private void Compare(int frame)
        {
            if (!localHashes.TryGetValue(frame, out string a) || !remoteHashes.TryGetValue(frame, out string b)) return;
            remoteHashes.Remove(frame);
            if (a != b) Fail("同期ずれを検出したため対戦を停止しました。勝敗・コイン報酬は送信しません。");
        }
        public void SetWaiting(bool value)
        {
            if (value && !Waiting) waitingSince = Time.realtimeSinceStartup;
            Waiting = value;
        }
        private void Update()
        { if (Active && Waiting && Time.realtimeSinceStartup - waitingSince > 8) Fail("相手の入力が届かないため終了しました。再接続してください。"); }
        public void ReadyNext() { Notice = "相手の準備を待っています"; Ui("next"); }
        public void ContinueRound(string unused) { if (Active) { Notice = ""; flow.ContinueOnlineRound(); } }
        public void Disconnected(string reason) { Fail("接続が終了しました。無料の直接通信で接続できない回線もあります。"); }
        public void Fail(string reason)
        {
            Active = false; Waiting = false; Inputs = null; Notice = reason;
            if (flow) flow.EndOnlineMatch(); Ui("stop|" + reason);
        }
        public void OpenLobby() => Ui("open");
    }
}
