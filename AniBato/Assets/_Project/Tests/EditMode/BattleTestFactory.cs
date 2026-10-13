using AniBato.Battle;

namespace AniBato.Tests
{
    internal static class BattleTestFactory
    {
        internal static BattleSimulation Create(bool adjacent = false, BattleRuleDefinition? rules = null, CharacterDefinition? definition = null)
        {
            var stage = PhaseOneDefaults.Stage();
            if (adjacent)
            {
                var platforms = new StagePlatform[stage.PlatformCount];
                for (int i = 0; i < platforms.Length; i++) platforms[i] = stage[i];
                stage = new StageGeometry(platforms, -24, 24, -10, new BattleVector(0, 1.1f), new BattleVector(1.5f, 1.1f));
            }
            var character = definition ?? PhaseOneDefaults.Character();
            return new BattleSimulation(character, character, rules ?? PhaseOneDefaults.Rules(), stage);
        }
        internal static CharacterDefinition StationaryAttack(int activeFrames = 3, int startup = 4)
        {
            var character = PhaseOneDefaults.Character();
            var attack = PhaseOneDefaults.Normal(); attack.Knockback = default; attack.HitstopFrames = 0;
            attack.HitstunFrames = 0; attack.ActiveFrames = activeFrames; attack.StartupFrames = startup;
            // This fixture isolates damage/KO ordering at fixed positions, rather than locomotion.
            attack.NormalForwardDistance = 0; attack.DamageRecoilDistance = 0; attack.DamageRecoilFrames = 0;
            character.GroundNormals = new[] { attack, attack, attack }; character.AirNormals = new[] { attack, attack, attack };
            return character;
        }
        internal static void Idle(BattleSimulation simulation, int ticks)
        { for (int i = 0; i < ticks; i++) simulation.Tick(default, default); }
        internal static void Hit(BattleSimulation simulation)
        {
            simulation.Tick(new FrameInput(pressed: InputButtons.Attack), default);
            Idle(simulation, 25);
        }
    }
}
