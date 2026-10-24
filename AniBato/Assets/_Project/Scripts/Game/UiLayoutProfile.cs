using System;
using System.Collections.Generic;
using UnityEngine;

namespace AniBato.Game
{
    [Serializable] public sealed class UiLayoutEntry
    {
        public string Id;
        public float X, Y, Scale = 1, Opacity = 1;
    }
    [Serializable] public sealed class UiLayoutProfile
    {
        public const string Storage = "AniBato.UiLayout.v1";
        public List<UiLayoutEntry> Entries = new List<UiLayoutEntry>();
        public UiLayoutEntry Find(string id) => Entries.Find(e => e.Id == id);
        public UiLayoutEntry Set(string id, float x, float y, float scale, float opacity)
        {
            var e = Find(id); if (e == null) { e = new UiLayoutEntry { Id = id }; Entries.Add(e); }
            e.X = Safe(x, 0, 1, .5f); e.Y = Safe(y, 0, 1, .5f);
            e.Scale = Safe(scale, .5f, 2, 1); e.Opacity = Safe(opacity, .15f, 1, 1); return e;
        }
        public UiLayoutProfile Clone() => Decode(JsonUtility.ToJson(this));
        public static UiLayoutProfile Load() => Decode(PlayerPrefs.GetString(Storage, ""));
        public void Save() { PlayerPrefs.SetString(Storage, JsonUtility.ToJson(this)); PlayerPrefs.Save(); }
        public static UiLayoutProfile Decode(string text)
        {
            var result = new UiLayoutProfile();
            if (string.IsNullOrEmpty(text)) return result;
            try
            {
                var decoded = JsonUtility.FromJson<UiLayoutProfile>(text);
                if (decoded?.Entries == null) return result;
                foreach (var e in decoded.Entries)
                    if (e != null && !string.IsNullOrEmpty(e.Id) && e.Id.Length <= 64)
                        result.Set(e.Id, e.X, e.Y, e.Scale, e.Opacity);
            }
            catch (ArgumentException) { }
            return result;
        }
        private static float Safe(float value, float min, float max, float fallback) =>
            float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp(value, min, max);
    }
}
