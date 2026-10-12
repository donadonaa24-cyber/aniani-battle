using AniBato.Battle;

namespace AniBato.Tests
{
    internal static class PhaseTwoTestFactory
    {
        internal static CharacterDefinition Character(int startup = 0)
        {
            var character = BattleTestFactory.StationaryAttack(startup: startup);
            for (int i = 0; i < character.Specials.Length; i++)
            {
                var attack = character.Specials[i];
                attack.StartupFrames = startup; attack.ActiveFrames = 1; attack.RecoveryFrames = 1;
                attack.HitstopFrames = 0; attack.HitstunFrames = 0; attack.Knockback = default;
                attack.SpawnProjectile = false; attack.ActiveVelocity = default;
                attack.Hitbox = character.GroundNormals[0].Hitbox;
                character.Specials[i] = attack;
            }
            var ultimate = character.Ultimate;
            ultimate.StartupFrames = startup; ultimate.HitstopFrames = 0; ultimate.HitstunFrames = 0; ultimate.Knockback = default;
            character.Ultimate = ultimate;
            return character;
        }
        internal static FrameInput Guard => new FrameInput(held: InputButtons.Guard, pressed: InputButtons.Guard);
        internal static FrameInput Button(InputButtons button) => new FrameInput(pressed: button);
    }
}
