using System.Collections;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class GameFlowSceneTests
    {
        [UnityTest]
        public IEnumerator OnlineTeamStartsWithSharedSeedWaitsForInputAndDisconnectReturnsToTitle()
        {
            yield return CpuToCountdown();var f=Flow;
            f.Online.Begin("{\"player\":1,\"stage\":1,\"first\":[0,1,2],\"second\":[3,4,5],\"seed\":12345}");
            Assert.That(f.OnlineMode,Is.True);Assert.That(f.CpuMode,Is.False);Assert.That(f.PracticeMode,Is.False);
            Assert.That(f.Player1Index,Is.EqualTo(0));Assert.That(f.Player2Index,Is.EqualTo(3));Assert.That(f.StageIndex,Is.EqualTo(1));Assert.That(f.Series.RoundNumber,Is.EqualTo(1));
            Assert.That(f.Online.LocalPlayer,Is.EqualTo(1));Assert.That(f.Online.Inputs.NextFrame,Is.Zero);
            f.enabled=false;f.Runner.enabled=false;f.Runner.SetPaused(false);
            // Initial delayed neutral frames are shared; unavailable frame 6 freezes both sides.
            for(int i=0;i<8;i++)f.Runner.AdvanceRenderFrame(1d/60);
            Assert.That(f.Online.Inputs.NextFrame,Is.EqualTo(6));Assert.That(f.Online.Waiting,Is.True);
            int tick=f.Runner.Simulation.Match.TickCount;
            f.Runner.AdvanceRenderFrame(.05);Assert.That(f.Runner.Simulation.Match.TickCount,Is.EqualTo(tick));
            f.Online.Packet("i|1|"+OnlineInputCodec.Encode(6,new FrameInput(1)));
            f.Runner.AdvanceRenderFrame(1d/60);Assert.That(f.Online.Inputs.NextFrame,Is.EqualTo(7));
            f.Online.Disconnected("");Assert.That(f.OnlineMode,Is.False);Assert.That(f.Screen,Is.EqualTo(GameScreen.Title));Assert.That(f.Runner.Paused,Is.True);
        }
        [UnityTest]
        public IEnumerator DrawReplayUsesNewEpochAndIgnoresDelayedOldInput()
        {
            yield return CpuToCountdown();var f=Flow;
            f.Online.Begin("{\"player\":0,\"stage\":0,\"first\":[0,1,2],\"second\":[3,4,5],\"seed\":1}");
            f.enabled=false;f.Runner.enabled=false;
            f.Online.NewRound(1); // Same displayed round, new transport epoch 2.
            for(int i=0;i<6;i++)Assert.That(f.Online.Inputs.TryTake(out _,out _),Is.True);
            f.Online.Inputs.Add(0,6,default);
            f.Online.Packet("i|1|"+OnlineInputCodec.Encode(6,new FrameInput(-1)));
            Assert.That(f.Online.Inputs.TryTake(out _,out _),Is.False,"old-round input must never fill a replay frame");
            f.Online.Packet("i|2|"+OnlineInputCodec.Encode(6,new FrameInput(1)));
            Assert.That(f.Online.Inputs.TryTake(out _,out var peer),Is.True);
            Assert.That(peer.X,Is.EqualTo(1));Assert.That(f.OnlineMode,Is.True);
            f.Online.Disconnected("");
            f.Online.Begin("{\"player\":0,\"stage\":0,\"first\":[0,1,2],\"second\":[3,4,5],\"seed\":2}");
            for(int i=0;i<6;i++)Assert.That(f.Online.Inputs.TryTake(out _,out _),Is.True);
            f.Online.Inputs.Add(0,6,default);
            f.Online.Packet("i|1|"+OnlineInputCodec.Encode(6,new FrameInput(-1)));
            Assert.That(f.Online.Inputs.TryTake(out _,out peer),Is.True,"a fresh connection resets its epoch");
            Assert.That(peer.X,Is.EqualTo(-1));
        }
        [UnityTest]
        public IEnumerator DuplicateOnlineTeamAndMalformedWireDataFailClosed()
        {
            yield return CpuToCountdown();var f=Flow;
            f.Online.Begin("{\"player\":0,\"stage\":0,\"first\":[0,0,2],\"second\":[3,4,5],\"seed\":1}");
            Assert.That(f.OnlineMode,Is.False);Assert.That(f.Screen,Is.EqualTo(GameScreen.Title));
            f.Online.Begin("{\"player\":0,\"stage\":0,\"first\":[0,1,2],\"second\":[3,4,5],\"seed\":1}");
            Assert.That(f.OnlineMode,Is.True);f.Online.Packet("i|1|bad");
            Assert.That(f.OnlineMode,Is.False);Assert.That(f.Runner.Paused,Is.True);
        }
    }
}
