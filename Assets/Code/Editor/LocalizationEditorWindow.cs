#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Localization.Editor
{
    public class LocalizationEditorWindow : OdinEditorWindow
    {
        private const string LocalizationFolder = "Assets/Code/Localization/Intro/Resources/Localization";

        [MenuItem("Tools/Localization Editor 🈺")]
        private static void OpenWindow()
        {
            var window = GetWindow<LocalizationEditorWindow>();
            window.titleContent = new GUIContent("Localization Editor");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }

        [ValueDropdown("GetAvailableLanguages")]
        [OnValueChanged("LoadLocalizationFile")]
        public string SelectedLanguage;

        [TableList(AlwaysExpanded = true)]
        public List<LocalizationEntry> Entries = new();

        [Button("💾 Save"), GUIColor(0.6f, 1f, 0.6f)]
        private void Save()
        {
            if (string.IsNullOrEmpty(SelectedLanguage)) return;

            string path = Path.Combine(LocalizationFolder, SelectedLanguage + ".txt");
            List<string> lines = new();
            foreach (var entry in Entries)
            {
                lines.Add($"{entry.Key}={entry.Value.Replace("\n", "\\n")}");
            }

            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();

            Debug.Log($"✅ Saved language file: {path}");
        }

        private void LoadLocalizationFile()
        {
            Entries.Clear();
            string path = Path.Combine(LocalizationFolder, SelectedLanguage + ".txt");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"File not found: {path}");
                return;
            }

            string[] lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                string[] pair = line.Split(new[] { '=' }, 2);
                if (pair.Length == 2)
                {
                    Entries.Add(new LocalizationEntry { Key = pair[0].Trim(), Value = pair[1].Trim().Replace("\\n", "\n") });
                }
            }
        }

        private IEnumerable<string> GetAvailableLanguages()
        {
            if (!Directory.Exists(LocalizationFolder))
                Directory.CreateDirectory(LocalizationFolder);

            string[] files = Directory.GetFiles(LocalizationFolder, "*.txt");
            foreach (string file in files)
                yield return Path.GetFileNameWithoutExtension(file);
        }

        [Serializable]
        public class LocalizationEntry
        {
            [HorizontalGroup("Row", Width = 250)]
            public string Key;

            [HorizontalGroup("Row")]
            [MultiLineProperty(2)]
            public string Value;
        }
    }
}
#endif
