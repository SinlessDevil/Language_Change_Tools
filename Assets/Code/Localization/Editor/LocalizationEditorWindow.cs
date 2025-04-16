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

        [ShowInInspector, PropertyOrder(-1)]
        [FoldoutGroup("Localization File", expanded: true)]
        [ValueDropdown("GetAvailableLanguages")]
        [OnValueChanged("LoadLocalizationFile")]
        private string _selectedLanguage;

        [FoldoutGroup("Localization File")]
        [ShowInInspector, PropertyOrder(1)]
        [OnValueChanged("FilterEntries")]
        [LabelText("🔍 Search Key")]
        private string _searchKey = string.Empty;
        
        [FoldoutGroup("Localization File")]
        [ShowInInspector, PropertyOrder(2)]
        [TableList(AlwaysExpanded = true)]
        private List<LocalizationEntry> _entries = new();

        private List<LocalizationEntry> _allEntries = new();

        [FoldoutGroup("Localization File", expanded: true)]
        [ShowInInspector, PropertyOrder(3)]
        [Button("💾 Save Current Language", ButtonSizes.Large), GUIColor(0.6f, 1f, 0.6f)]
        private void Save()
        {
            if (string.IsNullOrEmpty(_selectedLanguage)) return;

            string path = Path.Combine(LocalizationFolder, _selectedLanguage + ".txt");
            List<string> lines = new();
            foreach (var entry in _entries)
            {
                lines.Add($"{entry.Key}={entry.Value.Replace("\n", "\\n")}");
            }

            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();

            Debug.Log($"✅ Saved language file: {path}");
        }

        [FoldoutGroup("Localization File", expanded: true)]
        [ShowInInspector, PropertyOrder(4)]
        [Button("🗑 Delete Selected Language", ButtonSizes.Large), GUIColor(1f, 0.4f, 0.4f)]
        private void DeleteSelectedLanguage()
        {
            if (string.IsNullOrEmpty(_selectedLanguage))
            {
                Debug.LogWarning("⚠ No language selected to delete.");
                return;
            }

            string path = Path.Combine(LocalizationFolder, _selectedLanguage + ".txt");
            if (File.Exists(path))
            {
                File.Delete(path);
                AssetDatabase.Refresh();
                Debug.Log($"🗑 Deleted language file: {path}");

                _selectedLanguage = null;
                _entries.Clear();
                _allEntries.Clear();
            }
            else
            {
                Debug.LogWarning($"⚠ File not found: {path}");
            }
        }
        
        [FoldoutGroup("Add New Language", expanded: true)]
        [ShowInInspector, PropertyOrder(0)]
        [ValueDropdown("GetNewLanguages")]
        private SystemLanguage _NewLanguage = SystemLanguage.Afrikaans;

        [FoldoutGroup("Add New Language")]
        [ShowInInspector, PropertyOrder(1)]
        [ValueDropdown("GetAvailableLanguages")]
        private string _BaseLanguage = "English";

        [FoldoutGroup("Add New Language")]
        [ShowInInspector, PropertyOrder(2)]
        [Button("🆕 Create New Language From Base", ButtonSizes.Large), GUIColor(0.1f, 0.8f, 1f)]
        private void CreateNewLanguageFromBase()
        {
            string newLang = _NewLanguage.ToString();
            string targetPath = Path.Combine(LocalizationFolder, newLang + ".txt");

            if (File.Exists(targetPath))
            {
                Debug.LogWarning($"Language '{newLang}' already exists.");
                return;
            }

            string basePath = Path.Combine(LocalizationFolder, _BaseLanguage + ".txt");
            if (!File.Exists(basePath))
            {
                Debug.LogError($"Base language file '{_BaseLanguage}.txt' not found!");
                return;
            }

            File.Copy(basePath, targetPath);
            AssetDatabase.Refresh();

            Debug.Log($"✅ Created '{newLang}' from '{_BaseLanguage}'.");
        }

        private void LoadLocalizationFile()
        {
            _entries.Clear();
            _allEntries.Clear();

            string path = Path.Combine(LocalizationFolder, _selectedLanguage + ".txt");
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
                    var entry = new LocalizationEntry
                    {
                        Key = pair[0].Trim(),
                        Value = pair[1].Trim().Replace("\\n", "\n")
                    };
                    _allEntries.Add(entry);
                }
            }

            FilterEntries();
        }

        private void FilterEntries()
        {
            if (string.IsNullOrEmpty(_searchKey))
            {
                _entries = new List<LocalizationEntry>(_allEntries);
            }
            else
            {
                _entries = _allEntries.FindAll(e =>
                    e.Key.IndexOf(_searchKey, StringComparison.OrdinalIgnoreCase) >= 0);
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

        private IEnumerable<SystemLanguage> GetNewLanguages()
        {
            var existing = new HashSet<string>(GetAvailableLanguages(), StringComparer.OrdinalIgnoreCase);
            foreach (SystemLanguage lang in Enum.GetValues(typeof(SystemLanguage)))
            {
                if (!existing.Contains(lang.ToString()))
                    yield return lang;
            }
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
