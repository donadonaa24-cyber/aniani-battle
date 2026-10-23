using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AniBato.EditorTools
{
    // Only measured artwork can use BC7. New/changed images and low-precision
    // effects keep their original import, so future additions fail safely.
    public static class WindowsTextureQualityPolicy
    {
        private const string FilePath="Assets/_Project/Scripts/Editor/WindowsTextureQualityPolicy.json";
        [Serializable] private sealed class Manifest { public Entry[] entries; }
        [Serializable] private sealed class Entry { public string source,sha256;public double psnr,meanAbsoluteError; }
        private static Dictionary<string,Entry> entries;
        public static bool Allows(string source,string sha256)
        {
            if(entries==null)
            {
                entries=new Dictionary<string,Entry>(StringComparer.Ordinal);
                if(File.Exists(FilePath))foreach(var entry in JsonUtility.FromJson<Manifest>(File.ReadAllText(FilePath)).entries)
                    if(entry.psnr>=35&&entry.meanAbsoluteError<=2.5&&!entry.source.Contains("/Sprites/"))entries.Add(entry.source,entry);
            }
            return entries.TryGetValue(source,out var value)&&string.Equals(value.sha256,sha256,StringComparison.OrdinalIgnoreCase);
        }
    }
}
