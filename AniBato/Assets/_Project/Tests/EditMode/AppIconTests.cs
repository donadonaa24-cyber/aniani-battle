using AniBato.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace AniBato.Tests
{
    public sealed class AppIconTests
    {
        [Test] public void AndroidLauncherHasEverySupportedLayerAndBrandedFlatFallbackAfterReconfiguration()
        {
            AppIconBuilder.ConfigureAndroid();
            var legacy = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconBuilder.ArtRoot + "app-icon-legacy.png");
            var background = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconBuilder.ArtRoot + "app-icon-background.png");
            var foreground = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconBuilder.ArtRoot + "app-icon-foreground.png");
            var fallback = PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Application);
            Assert.That(fallback, Has.Length.EqualTo(1));
            Assert.That(fallback[0], Is.SameAs(legacy));
            int adaptiveSlots = 0;
            foreach (var kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android))
            foreach (var icon in PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind))
            {
                Assert.That(icon.GetTexture(0), Is.SameAs(icon.maxLayerCount > 1 ? background : legacy));
                if (icon.maxLayerCount > 1)
                {
                    adaptiveSlots++;
                    Assert.That(icon.GetTexture(1), Is.SameAs(foreground));
                }
            }
            Assert.That(adaptiveSlots, Is.EqualTo(6), "All six Android density slots must be configured.");
            foreach (var texture in new[] { legacy, background, foreground })
            {
                Assert.That(texture.width, Is.EqualTo(1024));
                Assert.That(texture.height, Is.EqualTo(1024));
            }
        }
    }
}
