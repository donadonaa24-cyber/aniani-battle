using System.Collections;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AniBato.Tests
{
    public sealed partial class PaperCharacterSceneTests
    {
        [UnityTest]
        public IEnumerator AllNineDownSpecialsUseRealDirectionalInputAndShowCooldownsOnBothSides()
        {
            var keys = controller.Bindings.P1;
            for (int character = 0; character < 9; character++) foreach (int facing in new[] { 1, -1 })
            {
                runner.BeginMatch(runner.Setup.Character(character), runner.Setup.Character(2), runner.CurrentStage, false);
                var sim = runner.Simulation; Set(sim.P1, "Facing", facing); var expected = sim.P1.Definition.Special(SpecialSlot.Down);
                yield return Key(keys.Down, true); yield return Key(keys.Special, true); Tick(); yield return Key(keys.Special, false); yield return Key(keys.Down, false);
                Assert.That(sim.P1.CurrentSpecial, Is.EqualTo(SpecialSlot.Down), "character " + character);
                Assert.That(sim.P1.Attack.Definition.Name, Is.EqualTo(expected.Name));
                for (int tick = 0; tick < expected.StartupFrames + 1; tick++) Tick();
                yield return null; view.RenderFrame(); Assert.That(view.UsesPaperSprite, Is.True);
                var hud = Object.FindAnyObjectByType<HUDController>(); hud.SendMessage("LateUpdate");
                foreach(var cooldown in Object.FindObjectsByType<SpecialCooldownHud>())cooldown.Refresh();
                var text = Object.FindObjectsByType<Text>().Single(t => t.name == "P1 Special Status 1");
                Assert.That(text.text, Does.Contain("↓ 特殊")); Assert.That(text.text, Does.Contain("秒"));
                if (character == 3 || character == 7) Assert.That(text.text, Does.Contain("設置 1/"));
                if (character == 0 || character == 4) Assert.That(body.color.b, Is.GreaterThan(.9f));
                Capture("down_" + character + "_" + facing, false, "directional_down");
            }
        }
    }
}
