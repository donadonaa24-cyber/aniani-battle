using System;
using System.Globalization;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class OnlineInputTests
    {
        [Test]
        public void CodecPreservesLatchedDirectionsAndCulture()
        {
            var culture=CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("de-DE");
                var latch=new FrameInputLatch();
                latch.Sample(new FrameInput(.3f,-.8f,InputButtons.Attack,InputButtons.Attack));
                latch.Sample(new FrameInput(-.7f,.9f,InputButtons.Special,InputButtons.Special,disableDoubleTapDash:true));
                var source=latch.Consume();var decoded=OnlineInputCodec.Decode(OnlineInputCodec.Encode(71,source),out int frame);
                Assert.That(frame,Is.EqualTo(71));Assert.That(decoded.X,Is.EqualTo(source.X));Assert.That(decoded.Y,Is.EqualTo(source.Y));
                Assert.That(decoded.Pressed,Is.EqualTo(source.Pressed));Assert.That(decoded.Held,Is.EqualTo(source.Held));Assert.That(decoded.DisableDoubleTapDash,Is.True);
                for(int i=0;i<8;i++)Assert.That(decoded.DirectionFor((InputButtons)(1<<i)),Is.EqualTo(source.DirectionFor((InputButtons)(1<<i))));
            }finally{CultureInfo.CurrentCulture=culture;}
        }
        [TestCase("0,0,0,0,NaN,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0")]
        [TestCase("0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0")]
        [TestCase("0,256,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0")]
        [TestCase("-1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0")]
        [TestCase("1,2,3")]
        public void InvalidPacketFailsClosed(string value)=>Assert.Throws<FormatException>(()=>OnlineInputCodec.Decode(value,out _));
        [Test]
        public void MissingRemoteInputCannotAdvanceOrBeReplacedByNeutral()
        {
            var b=new OnlineInputBuffer(0);
            for(int i=0;i<OnlineInputBuffer.Delay;i++)Assert.That(b.TryTake(out _,out _),Is.True);
            b.Add(0,6,new FrameInput(1));Assert.That(b.TryTake(out _,out _),Is.False);Assert.That(b.NextFrame,Is.EqualTo(6));
            b.Add(1,6,new FrameInput(-1));Assert.That(b.TryTake(out var a,out var c),Is.True);Assert.That(a.X,Is.EqualTo(1));Assert.That(c.X,Is.EqualTo(-1));
        }
        [Test]
        public void DuplicateAndUnboundedFutureInputRejected()
        {
            var b=new OnlineInputBuffer(1);b.Add(1,6,default);Assert.Throws<FormatException>(()=>b.Add(1,6,default));Assert.Throws<FormatException>(()=>b.Add(0,1000,default));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void BothPlayersRunIdenticallyWithWireInputsAndSameSupportSeed(int character)
        {
            var setup=AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
            var rules=AssetDatabase.LoadAssetAtPath<BattleRules>("Assets/_Project/Data/BattleRules.asset").ToDefinition();
            var a=new BattleSimulation(setup.Character(character).ToDefinition(),setup.Character((character+1)%9).ToDefinition(),rules,setup.Stages[0].ToGeometry(),12345);
            var b=new BattleSimulation(setup.Character(character).ToDefinition(),setup.Character((character+1)%9).ToDefinition(),rules,setup.Stages[0].ToGeometry(),12345);
            for(int i=0;i<800;i++)
            {
                var one=new FrameInput(i%80<40?.5f:-.5f,0, i%100<30?InputButtons.Attack:InputButtons.None, i%100==0?InputButtons.Attack:InputButtons.None);
                var two=new FrameInput(i%60<30?-.5f:.5f,0,i%120<10?InputButtons.Special:InputButtons.None,i%120==0?InputButtons.Special:InputButtons.None);
                var wire1=OnlineInputCodec.Decode(OnlineInputCodec.Encode(i,one),out _);var wire2=OnlineInputCodec.Decode(OnlineInputCodec.Encode(i,two),out _);
                a.Tick(one,two);b.Tick(wire1,wire2);
                if(i%120==0)Assert.That(OnlineStateHash.Compute(b),Is.EqualTo(OnlineStateHash.Compute(a)),"frame "+i);
            }
            Assert.That(OnlineStateHash.Compute(b),Is.EqualTo(OnlineStateHash.Compute(a)));
            b.P1.StaminaUnits--;Assert.That(OnlineStateHash.Compute(b),Is.Not.EqualTo(OnlineStateHash.Compute(a)));
        }
    }
}
