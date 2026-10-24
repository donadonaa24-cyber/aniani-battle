using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class AppIconBuilder
    {
        public const string ArtRoot = "Assets/_Project/Art/Branding/";

        public static void ConfigureAndroid()
        {
            var legacy = Load("app-icon-legacy.png");
            var background = Load("app-icon-background.png");
            var foreground = Load("app-icon-foreground.png");
            // Unity 6000.6 exposes Adaptive slots on Android. The general icon is
            // the branded fallback for exporters/launchers requesting a flat bitmap.
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { legacy }, IconKind.Application);
            var target = NamedBuildTarget.Android;
            var kinds = PlayerSettings.GetSupportedIconKinds(target);
            if (kinds.Length == 0) throw new InvalidOperationException("Android icon slots are unavailable.");
            foreach (var kind in kinds)
            {
                var icons = PlayerSettings.GetPlatformIcons(target, kind);
                foreach (var icon in icons)
                {
                    var layers = new Texture2D[icon.maxLayerCount];
                    layers[0] = icon.maxLayerCount > 1 ? background : legacy;
                    if (layers.Length > 1) layers[1] = foreground;
                    icon.SetTextures(layers);
                }
                PlayerSettings.SetPlatformIcons(target, kind, icons);
            }
        }

        private static Texture2D Load(string name)
        {
            string path = ArtRoot + name;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing launcher artwork: " + path);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
