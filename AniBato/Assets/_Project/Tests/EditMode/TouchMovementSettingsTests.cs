using AniBato.Game;
using NUnit.Framework;
using UnityEngine;

namespace AniBato.Tests
{
    public sealed class TouchMovementSettingsTests
    {
        [Test] public void MissingAndInvalidPreferencesUseContextDefaultsAndSavingKeepsThemSeparate()
        {
            string[] keys = { TouchMovementSettings.CombatStorage, TouchMovementSettings.MenuStorage };
            int?[] before = { PlayerPrefs.HasKey(keys[0]) ? PlayerPrefs.GetInt(keys[0]) : (int?)null,
                PlayerPrefs.HasKey(keys[1]) ? PlayerPrefs.GetInt(keys[1]) : (int?)null };
            try
            {
                foreach (string key in keys) PlayerPrefs.DeleteKey(key);
                var settings = TouchMovementSettings.Load();
                Assert.That(settings.Combat, Is.EqualTo(TouchMovementMode.Stick));
                Assert.That(settings.Menu, Is.EqualTo(TouchMovementMode.DirectionalPad));
                settings.Set(false, TouchMovementMode.Stick);
                settings.Set(true, TouchMovementMode.DirectionalPad);
                var loaded = TouchMovementSettings.Load();
                Assert.That(loaded.Combat, Is.EqualTo(TouchMovementMode.DirectionalPad));
                Assert.That(loaded.Menu, Is.EqualTo(TouchMovementMode.Stick));
                PlayerPrefs.SetInt(keys[0], 9); PlayerPrefs.SetInt(keys[1], -5);
                loaded = TouchMovementSettings.Load();
                Assert.That(loaded.Combat, Is.EqualTo(TouchMovementMode.Stick));
                Assert.That(loaded.Menu, Is.EqualTo(TouchMovementMode.DirectionalPad));
            }
            finally { for (int i=0;i<keys.Length;i++) { if(before[i].HasValue) PlayerPrefs.SetInt(keys[i],before[i].Value);else PlayerPrefs.DeleteKey(keys[i]); } PlayerPrefs.Save(); }
        }
    }
}
