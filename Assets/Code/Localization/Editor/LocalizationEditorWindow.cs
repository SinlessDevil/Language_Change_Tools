#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Code.Localization.Code;
using TMPro;

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
            window.minSize = new Vector2(600, 800);
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
        
        [FoldoutGroup("Create New Language TxT", expanded: true)]
        [ShowInInspector, PropertyOrder(0)]
        [ValueDropdown("GetNewLanguages")]
        private SystemLanguage _NewLanguage = SystemLanguage.Unknown;

        [FoldoutGroup("Create New Language TxT")]
        [ShowInInspector, PropertyOrder(1)]
        [ValueDropdown("GetAvailableLanguages")]
        private string _BaseLanguage = "English";

        [FoldoutGroup("Create New Language TxT")]
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

        [FoldoutGroup("Find Localized Assets")]
        [TableList(AlwaysExpanded = true)]
        [ShowInInspector]
        private List<TMPAssetEntry> _localizersInAssets = new();

        [FoldoutGroup("Find Localized Assets", expanded: true)]
        [Button("📁 Find All TMP_Localizers in Resources", ButtonSizes.Large), GUIColor(0.1f, 0.8f, 1f)]
        private void FindTMPLocalizersInAssets()
        {
            _localizersInAssets.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                if (prefabRoot == null)
                {
                    Debug.LogWarning($"❌ Failed to load prefab contents: {path}");
                    continue;
                }
                
                LocalizeBase[] localizers = prefabRoot.GetComponentsInChildren<LocalizeBase>(true);

                foreach (var localizer in localizers)
                {
                    string objName = localizer.gameObject.name;
                    string parentName = localizer.transform.parent != null ? localizer.transform.parent.name : "(root)";
                    string childName = localizer.transform.childCount > 0 ? localizer.transform.GetChild(0).name : "(no child)";

                    _localizersInAssets.Add(new TMPAssetEntry
                    {
                        AssetPath = path,
                        GameObjectName = objName,
                        ParentName = parentName,
                        ChildName = childName,
                        Component = localizer,
                        LocalizationKey = localizer.localizationKey
                    });
                }

                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            Debug.Log($"✅ Done! Total found TMP_Localizers in prefabs: {_localizersInAssets.Count}");
        }
        
        [FoldoutGroup("Find Localized Assets")]
        [Button("💾 Save Changes To Assets", ButtonSizes.Large), GUIColor(0.6f, 1f, 0.6f)]
        private void ApplyLocalizationKeyChangesToAssets()
        {
            foreach (var entry in _localizersInAssets)
            {
                if (entry.Component == null) continue;

                Undo.RecordObject(entry.Component, "Change Localization Key");
                entry.Component.localizationKey = entry.LocalizationKey;
                EditorUtility.SetDirty(entry.Component);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("✅ Localization keys in assets updated.");
        }
        
        [FoldoutGroup("Add Missing Localizers", expanded: true)]
        [Button("➕ Add TMP_Localizer to All TMP_Text In Resources", ButtonSizes.Large), GUIColor(0.8f, 0.9f, 1f)]
        private void AddTMP_LocalizersToResources()
        {
            int addedCount = 0;

            GameObject[] allPrefabs = Resources.LoadAll<GameObject>("");
            foreach (var prefab in allPrefabs)
            {
                if (prefab == null) continue;

                var path = AssetDatabase.GetAssetPath(prefab);
                if (string.IsNullOrEmpty(path)) continue;

                var root = PrefabUtility.LoadPrefabContents(path);
                bool wasModified = false;

                var texts = root.GetComponentsInChildren<TMP_Text>(true);
                foreach (var text in texts)
                {
                    if (text.GetComponent<LocalizeBase>() == null)
                    {
                        Undo.RegisterCompleteObjectUndo(text.gameObject, "Add TMP_Localizer");
                        text.gameObject.AddComponent<TMP_Localizer>();
                        EditorUtility.SetDirty(text.gameObject);
                        wasModified = true;
                        addedCount++;
                    }
                }

                if (wasModified)
                {
                    EditorUtility.SetDirty(root); // 💡 чтобы точно зафиксировалось
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✅ Added TMP_Localizer to {addedCount} TMP_Text objects.");
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
        
        [Serializable]
        public class TMPAssetEntry
        {
            [ReadOnly, TableColumnWidth(200)]
            public string GameObjectName;

            [ReadOnly, TableColumnWidth(150)]
            public string ParentName;

            [ReadOnly, TableColumnWidth(150)]
            public string ChildName;

            [ReadOnly, TableColumnWidth(300)]
            public string AssetPath;

            [HideInInspector]
            public LocalizeBase Component; 

            [TableColumnWidth(400, resizable: true)]
            public string LocalizationKey;
        }
    }
}
#endif
