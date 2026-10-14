using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace StageGame
{
    [Serializable] public sealed class StringEntry { public string key, ko, en; }
    [Serializable] public sealed class StringFile { public int schemaVersion; public StringEntry[] strings; }

    // One lookup table shared by static prefab labels and state-dependent game text.
    public sealed class StringTable
    {
        readonly Dictionary<string, StringEntry> entries = new Dictionary<string, StringEntry>(StringComparer.Ordinal);
        static readonly Regex Tokens = new Regex(@"\{\{|\}\}|\{([0-9]+)\}|[{}]");
        public string Language { get; private set; } = "ko";
        public static string NormalizeLanguage(string language) => language == "en" ? "en" : "ko";
        public void SetLanguage(string language) { Language = NormalizeLanguage(language); }
        public bool Contains(string key) => key != null && entries.ContainsKey(key);

        public StringTable(string json)
        {
            var file = JsonUtility.FromJson<StringFile>(json);
            if (file == null || file.schemaVersion != 1 || file.strings == null || file.strings.Length == 0)
                throw new FormatException("Invalid string table version or list.");
            foreach (var entry in file.strings)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key) || string.IsNullOrWhiteSpace(entry.ko) ||
                    string.IsNullOrWhiteSpace(entry.en) || entries.ContainsKey(entry.key))
                    throw new FormatException("Missing or duplicate string key/translation.");
                if (!Arguments(entry.ko).SetEquals(Arguments(entry.en)))
                    throw new FormatException("Translation arguments differ: " + entry.key);
                entries.Add(entry.key, entry);
            }
        }

        static HashSet<int> Arguments(string value)
        {
            var indices = new HashSet<int>();
            foreach (Match token in Tokens.Matches(value))
            {
                if (token.Value == "{{" || token.Value == "}}") continue;
                if (!token.Groups[1].Success || !int.TryParse(token.Groups[1].Value, out int index))
                    throw new FormatException("Unsupported string format: " + value);
                indices.Add(index);
            }
            for (int i = 0; i < indices.Count; i++)
                if (!indices.Contains(i)) throw new FormatException("String arguments must start at zero and be contiguous.");
            return indices;
        }

        public string Text(string key)
        {
            if (!entries.TryGetValue(key, out var entry)) throw new KeyNotFoundException("Missing UI string: " + key);
            return Language == "en" ? entry.en : entry.ko;
        }

        public string Format(string key, params object[] args) => string.Format(CultureInfo.InvariantCulture, Text(key), args);

        public static StringTable Load()
        {
            var asset = Resources.Load<TextAsset>("GameData/strings");
            if (asset == null) throw new FormatException("Missing string table.");
            return new StringTable(asset.text);
        }
    }
}
