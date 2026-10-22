using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace AniBato.Battle
{
    // Diagnostic, not authentication. Stop on divergence instead of accepting an unverified score.
    public static class OnlineStateHash
    {
        private static readonly Dictionary<Type, FieldInfo[]> fields = new Dictionary<Type, FieldInfo[]>();
        public static string Compute(BattleSimulation simulation)
        { ulong hash = 14695981039346656037UL; Walk(simulation, ref hash, 0); return hash.ToString("x16", CultureInfo.InvariantCulture); }
        private static void Text(string text, ref ulong hash)
        { foreach (char c in text) { hash = unchecked((hash ^ c) * 1099511628211UL); } hash = unchecked((hash ^ 255) * 1099511628211UL); }
        private static void Walk(object value, ref ulong hash, int depth)
        {
            if (value == null) { Text("null", ref hash); return; }
            if (depth > 24) throw new InvalidOperationException("状態の参照が深すぎます");
            var type = value.GetType();
            if (type.IsEnum || type.IsPrimitive || value is string || value is decimal)
            { Text(value is float f ? f.ToString("R", CultureInfo.InvariantCulture) : value is double d ? d.ToString("R", CultureInfo.InvariantCulture) : Convert.ToString(value, CultureInfo.InvariantCulture), ref hash); return; }
            if (value is IEnumerable sequence)
            {
                // Sets are logically unordered; dictionary/set iteration must not cause false divergence.
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
                {
                    var sorted = new List<string>(); foreach (var item in sequence) { ulong h = 14695981039346656037UL; Walk(item, ref h, depth + 1); sorted.Add(h.ToString("x16")); }
                    sorted.Sort(StringComparer.Ordinal); foreach (var item in sorted) Text(item, ref hash);
                }
                else foreach (var item in sequence) Walk(item, ref hash, depth + 1);
                Text("end", ref hash); return;
            }
            if (!fields.TryGetValue(type, out var members))
            {
                members = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Array.Sort(members, (a,b) => string.CompareOrdinal(a.Name, b.Name)); fields[type] = members;
            }
            foreach (var field in members)
            {
                // Immutable rule/character definitions are validated by the exact WebGL build identity.
                if (field.FieldType.Name.EndsWith("Definition", StringComparison.Ordinal) || typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;
                Text(field.Name, ref hash); Walk(field.GetValue(value), ref hash, depth + 1);
            }
        }
    }
}
