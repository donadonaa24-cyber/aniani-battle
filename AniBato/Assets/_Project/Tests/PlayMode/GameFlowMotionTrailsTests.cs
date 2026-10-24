using System.Collections;
using System.Linq;
using AniBato.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AniBato.Tests
{
    public sealed partial class GameFlowSceneTests
    {
        [UnityTest] public IEnumerator MotionTrailOptionPersistsAndBackStillReturnsToTheMenu()
        {
            bool existed = PlayerPrefs.HasKey(BattleRunner.MotionTrailsPreference);
            int previous = PlayerPrefs.GetInt(BattleRunner.MotionTrailsPreference);
            try
            {
                Flow.Runner.SetMotionTrails(true, false);
                yield return ChooseMain(3); yield return SettingsItem(11);
                yield return Tap(keyboard.enterKey);
                Assert.That(Flow.Runner.MotionTrailsEnabled, Is.False);
                Assert.That(PlayerPrefs.GetInt(BattleRunner.MotionTrailsPreference), Is.Zero);
                Assert.That(Object.FindObjectsByType<UnityEngine.UI.Text>().Any(t => t.text.Contains("OFF（元の表示）")), Is.True);
                yield return Tap(keyboard.dKey);
                Assert.That(Flow.Runner.MotionTrailsEnabled, Is.True);
                Assert.That(PlayerPrefs.GetInt(BattleRunner.MotionTrailsPreference), Is.EqualTo(1));
                CaptureScreen("settings", 720, "motion_trails");
                yield return SettingsItem(15); yield return Tap(keyboard.enterKey);
                Assert.That(Flow.Screen, Is.EqualTo(GameScreen.MainMenu));
                yield return PracticeToBattle(); yield return Tap(keyboard.escapeKey);
                Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause));
                yield return Tap(keyboard.sKey); yield return Tap(keyboard.enterKey);
                yield return SettingsItem(11); yield return Tap(keyboard.enterKey);
                Assert.That(Flow.Runner.MotionTrailsEnabled, Is.False);
                Assert.That(Flow.Runner.Paused, Is.True);
                yield return Tap(keyboard.escapeKey); Assert.That(Flow.Screen, Is.EqualTo(GameScreen.Pause));
            }
            finally
            {
                if (existed) PlayerPrefs.SetInt(BattleRunner.MotionTrailsPreference, previous);
                else PlayerPrefs.DeleteKey(BattleRunner.MotionTrailsPreference);
                PlayerPrefs.Save();
            }
        }
    }
}
