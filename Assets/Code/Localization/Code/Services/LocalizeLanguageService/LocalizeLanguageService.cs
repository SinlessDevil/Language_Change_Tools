using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Code.Localization.Code.Services.LocalizeLanguageService
{
    public class LocalizeLanguageService : ILocalizeLanguageService
    {
        private const string ResourcesPath = "Localization";

        public List<string> GetAvailableLanguages()
        {
            List<string> languages = new();

            TextAsset[] files = Resources.LoadAll<TextAsset>(ResourcesPath);
            foreach (TextAsset file in files)
            {
                languages.Add(Path.GetFileNameWithoutExtension(file.name));
            }

            return languages;
        }

        public void SetLanguage(string languageName)
        {
            Localize.SetCurrentLanguage(LanguageNameToSystemLanguage(languageName));
        }

        public string GetCurrentLanguage()
        {
            return Locale.CurrentLanguage;
        }
        
        private SystemLanguage LanguageNameToSystemLanguage(string name)
        {
            if (System.Enum.TryParse<SystemLanguage>(name, true, out var lang))
                return lang;

            Debug.LogWarning($"⚠️ Unknown language '{name}', fallback to English.");
            return SystemLanguage.English;
        }
    }
}