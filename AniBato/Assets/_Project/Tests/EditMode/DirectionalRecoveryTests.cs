using AniBato.Battle;
using AniBato.Game;
using AniBato.Presentation;
using NUnit.Framework;
using UnityEditor;

namespace AniBato.Tests
{
    public sealed class DirectionalRecoveryTests
    {
        private static BattleSetup Setup => AssetDatabase.LoadAssetAtPath<BattleSetup>("Assets/_Project/Data/BattleSetup.asset");
        private static BattleSimulation Create(int index)
        {
            var rules = PhaseOneDefaults.Rules(); rules.EnableRingOut = false;
            rules.CombatInput = CombatInputDefinition.Default; rules.CombatInput.DownSpecialEnabled = rules.CombatInput.UpRecoveryEnabled = true;
            var sim = BattleTestFactory.Create(false, rules, Setup.Character(index).ToDefinition());
            Air(sim.P1, 0, 100); return sim;
        }
        private static void Air(FighterState f, float x, float y)
        { f.Position = new BattleVector(x, y); f.Grounded = false; f.GroundPlatform = -1; f.Velocity = default; }
        private static void Start(BattleSimulation sim, int x = 0)
        { sim.Tick(new FrameInput(x, 1, pressed: InputButtons.Special), default); }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void EveryFighterCanActuallyReturnFromBelowEitherLedge(int index)
        {
            foreach (int side in new[] {-1,1})
            {
                var rules=Create(index).Rules; rules.EnableRingOut=true;
                var sim=BattleTestFactory.Create(false,rules,Setup.Character(index).ToDefinition());
                Air(sim.P1,side*20,-3); sim.P1.Facing=-side;
                Start(sim,-side); bool landed=false;
                for(int i=0;i<180&&!sim.Match.IsFinished;i++)
                {
                    sim.Tick(new FrameInput(-side,1,disableDoubleTapDash:true),default);
                    if(sim.P1.Grounded&&sim.P1.Position.X>-18&&sim.P1.Position.X<18){landed=true;break;}
                }
                Assert.That(landed,Is.True,index+" side "+side+" at "+sim.P1.Position.X+","+sim.P1.Position.Y+" "+sim.P1.Mode);
            }
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void EveryRecoveryUsesUpSlotOneThirdDamageAndAnIndependentCooldown(int index)
        {
            var sim = Create(index); Start(sim); var attack = sim.P1.Attack;
            Assert.That(sim.P1.CurrentSpecial, Is.EqualTo(SpecialSlot.Up)); Assert.That(sim.P1.UpSpecialUsed, Is.True);
            Assert.That(attack.Definition.DamageMode, Is.EqualTo(AttackDamageMode.NormalAccumulation));
            Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Up), Is.EqualTo(90));
            Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Neutral) + sim.P1.SpecialCooldown(SpecialSlot.Down), Is.Zero);
            BattleTestFactory.Idle(sim, attack.Definition.StartupFrames);
            Air(sim.P2, sim.P1.Position.X, sim.P1.Position.Y); sim.Tick(default, default);
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1), index.ToString()); Assert.That(sim.P2.Hearts, Is.EqualTo(3));
            for (int i = 0; i < 12; i++) { Air(sim.P2, sim.P1.Position.X, sim.P1.Position.Y); sim.Tick(default, default); }
            Assert.That(sim.P2.NormalHitCount, Is.EqualTo(1));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)]
        public void AirborneReuseFailsUntilAnActualLandingEvenAfterCooldownEnds(int index)
        {
            var sim = Create(index); Air(sim.P1, 0, 1000); Start(sim); BattleTestFactory.Idle(sim, System.Math.Max(100,sim.P1.Attack.Definition.TotalFrames + 10));
            Assert.That(sim.P1.Attack, Is.Null); Assert.That(sim.P1.Grounded, Is.False); Assert.That(sim.P1.SpecialCooldown(SpecialSlot.Up), Is.Zero);
            Start(sim); Assert.That(sim.P1.Attack, Is.Null);
            Air(sim.P1, 0, 1.12f); sim.P1.Velocity = new BattleVector(0, -2); sim.Tick(default, default);
            Assert.That(sim.P1.Grounded, Is.True); Assert.That(sim.P1.UpSpecialUsed, Is.False);
            Start(sim); Assert.That(sim.P1.Attack, Is.Not.Null); Assert.That(sim.P1.UpSpecialUsed, Is.True);
        }
        [TestCase(-1)] [TestCase(1)]
        public void RookRisesVerticallyBishopRisesDiagonallyAndFloatCanSteer(int side)
        {
            var rook = Create(1); var bishop = Create(5); Start(rook, side); Start(bishop, side);
            BattleTestFactory.Idle(rook, 18); BattleTestFactory.Idle(bishop, 18);
            Assert.That(rook.P1.Position.X, Is.EqualTo(0)); Assert.That(rook.P1.Position.Y, Is.GreaterThan(102));
            Assert.That(bishop.P1.Position.X * side, Is.GreaterThan(1)); Assert.That(bishop.P1.Position.Y, Is.GreaterThan(102));
            foreach (int index in new[] { 0, 2, 7 })
            {
                var sim = Create(index); Start(sim); BattleTestFactory.Idle(sim, sim.P1.Attack.Definition.StartupFrames);
                sim.Tick(new FrameInput(side), default); Assert.That(sim.P1.Velocity.X * side, Is.GreaterThan(0));
                sim.Tick(new FrameInput(-side), default); Assert.That(sim.P1.Velocity.X * side, Is.LessThan(0));
                sim.Tick(default, default); Assert.That(sim.P1.Velocity.X, Is.Zero); Assert.That(sim.P1.Velocity.Y, Is.GreaterThan(0));
            }
        }
        [Test]
        public void KingChangesDirectionForSecondBurstAndStopsDuringTheBriefGap()
        {
            var sim = Create(6); Start(sim, 1); BattleTestFactory.Idle(sim, sim.P1.Attack.Definition.StartupFrames);
            float x = sim.P1.Position.X; Assert.That(sim.P1.Velocity.X, Is.GreaterThan(0));
            while (sim.P1.Attack.Frame < sim.P1.Attack.Definition.StartupFrames + sim.P1.Attack.Definition.Recovery.BurstFrames) sim.Tick(default, default);
            Assert.That(sim.P1.Velocity, Is.EqualTo(default(BattleVector)));
            while (sim.P1.Attack.Frame < sim.P1.Attack.Definition.StartupFrames + sim.P1.Attack.Definition.Recovery.SecondStartFrame - 1) sim.Tick(default, default);
            sim.Tick(new FrameInput(-1, 0), default); Assert.That(sim.P1.Velocity.X, Is.LessThan(0)); Assert.That(sim.P1.Velocity.Y, Is.Zero);
            Assert.That(sim.P1.Position.X, Is.GreaterThan(x));
        }
        [TestCase(3)] [TestCase(4)]
        public void TeleportCrossesInterveningSpaceButNeverEndsInsideASolid(int index)
        {
            var c = Setup.Character(index).ToDefinition(); var r = Create(index).Rules;
            var stage = new StageGeometry(new[] { new StagePlatform(new BattleRect(new BattleVector(0,-1), new BattleVector(20,2)), false),
                new StagePlatform(new BattleRect(new BattleVector(3,10), new BattleVector(1,20)), false) }, -50,50,-50,new BattleVector(0,10),new BattleVector(-8,10));
            var sim = new BattleSimulation(c,c,r,stage); Start(sim);
            for (int i=0;i<30;i++)sim.Tick(new FrameInput(1),default);
            Assert.That(sim.P1.Position.X, Is.EqualTo(7.5f).Within(.001));
            var blockedStage = new StageGeometry(new[] {stage[0],new StagePlatform(new BattleRect(new BattleVector(7.5f,10),new BattleVector(1,20)),false)},
                -50,50,-50,new BattleVector(0,10),new BattleVector(-8,10));
            var blocked = new BattleSimulation(c,c,r,blockedStage); Start(blocked);
            for(int i=0;i<30;i++)blocked.Tick(new FrameInput(1),default);
            Assert.That(blocked.P1.Position.X,Is.GreaterThan(6));
            Assert.That(blocked.P1.Position.X,Is.LessThanOrEqualTo(7-c.HurtboxSize.X*.5f));
            Assert.That(blocked.P1.Hurtbox.Overlaps(blocked.Stage[1].Bounds),Is.False);
            var free = Create(index); Start(free); for(int i=0;i<30;i++)free.Tick(new FrameInput(-1),default);
            Assert.That(free.P1.Position.X, Is.EqualTo(-7.5f).Within(.001));
        }
        [Test]
        public void RecoveryArtworkRetainsNativePixelsAndCooldownShowsLandingWait()
        {
            foreach (int index in new[]{0,2,7,8})
            {
                var pose = Setup.Character(index).PaperMotion.UpRecovery;
                Assert.That(pose.Valid, Is.True); Assert.That(pose.Frames.Length, Is.EqualTo(1));
                Assert.That(pose.Frames[0].pixelsPerUnit, Is.EqualTo(532.5893f).Within(.001));
            }
            var sim = Create(6); Start(sim); BattleTestFactory.Idle(sim,150);
            Assert.That(SpecialCooldownDisplay.Text(sim.P1,sim.Rules.CombatInput),Does.Contain("↑ 着地待ち"));
        }
        [TestCase(3)] [TestCase(4)]
        public void ConsciousTransferAcceptsAllEightDirectionsAtRelease(int index)
        {
            for (int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)
            {
                if(x==0&&y==0)continue;
                var sim=Create(index); Start(sim);
                BattleTestFactory.Idle(sim,29); var before=sim.P1.Position;
                sim.Tick(new FrameInput(x,y),default);
                var direction=RecoveryMotionSystem.Direction(x,y,default);
                Assert.That(sim.P1.Position.X-before.X,Is.EqualTo(direction.X*7.5f).Within(.001));
                Assert.That(sim.P1.Position.Y-before.Y,Is.EqualTo(direction.Y*7.5f-sim.P1.Definition.Gravity/3600).Within(.001));
            }
        }
        [Test]
        public void HitstopFreezesBothBurstAgeAndDisplacementAndDamageInterruptsTheRecovery()
        {
            var sim=Create(6); Start(sim,1); BattleTestFactory.Idle(sim,7);
            var position=sim.P1.Position; int frame=sim.P1.Attack.Frame; sim.P1.HitstopRemaining=5;
            for(int i=0;i<5;i++)sim.Tick(new FrameInput(-1),default);
            Assert.That(sim.P1.Position,Is.EqualTo(position)); Assert.That(sim.P1.Attack.Frame,Is.EqualTo(frame));
            var hit=new AttackInstance(900,PhaseOneDefaults.Normal(),0,true);
            new DamageSystem().Apply(new HitContact(sim.P2,sim.P1,hit),new ComboTracker(),sim.Match.TickCount,sim.Rules,new System.Collections.Generic.List<BattleEvent>());
            Assert.That(sim.P1.Attack,Is.Null); Assert.That(sim.P1.UpSpecialUsed,Is.True);
            Assert.That(sim.P1.Mode,Is.EqualTo(FighterMode.Hitstun));
        }
        [Test]
        public void TransferDoesNotClampAnOutwardDestinationAndCanPassOneWayPlatforms()
        {
            var rules=Create(3).Rules; rules.EnableRingOut=true;
            var sim=BattleTestFactory.Create(false,rules,Setup.Character(3).ToDefinition());
            Air(sim.P1,20,20); Start(sim); for(int i=0;i<30;i++)sim.Tick(new FrameInput(1),default);
            Assert.That(sim.P1.Position.X,Is.EqualTo(27.5f).Within(.001)); Assert.That(sim.P1.Mode,Is.EqualTo(FighterMode.KO));
            var pass=Create(3); Air(pass.P1,8,1.1f); Start(pass); BattleTestFactory.Idle(pass,30);
            Assert.That(pass.P1.Position.Y,Is.GreaterThan(pass.Stage[2].Bounds.Top+pass.P1.Definition.HurtboxSize.Y));
        }
        [Test]
        public void CpuRequestsTheNewUpSlotAfterJumpsAreSpentAndKeepsSteeringRecovery()
        {
            for(int index=0;index<9;index++)
            {
                var sim=Create(index); Air(sim.P1,20,-3); sim.P1.JumpsUsed=sim.P1.Definition.MaxJumps;
                sim.P1.AirDashesUsed=sim.P1.Definition.MaxAirDashes; sim.P1.Velocity=new BattleVector(0,-2);
                var brain=new CpuBrain(CpuDefinition.ForDifficulty(1),17);
                var input=brain.NextInput(sim,0); Assert.That(input.IsPressed(InputButtons.Special),Is.True,index.ToString()); Assert.That(input.Y,Is.EqualTo(1));
                sim.Tick(input,default); Assert.That(sim.P1.CurrentSpecial,Is.EqualTo(SpecialSlot.Up));
                input=brain.NextInput(sim,0); Assert.That(input.X,Is.LessThan(0)); Assert.That(input.Y,Is.EqualTo(1));
            }
        }
    }
}
