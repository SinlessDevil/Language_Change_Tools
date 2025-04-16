using UnityEngine;

namespace Code.Localization.Code
{
    public abstract class LocalizeBase : MonoBehaviour
    {
        public string localizationKey;

        public abstract void UpdateLocale();

        protected virtual void Start()
        {
            if (!Locale.currentLanguageHasBeenSet)
            {
                Locale.currentLanguageHasBeenSet = true;
                SetCurrentLanguage(Locale.PlayerLanguage);
            }

            UpdateLocale();
        }

        public static string GetLocalizedString(string key)
        {
            if (Locale.currentLanguageStrings.ContainsKey(key))
                return Locale.currentLanguageStrings[key];
            
            return string.Empty;
        }

        public static void SetCurrentLanguage(SystemLanguage language)
        {
            Locale.CurrentLanguage = language.ToString();
            Locale.PlayerLanguage = language;
            Localize[] allTexts = FindObjectsOfType<Localize>();
            for (int i = 0; i < allTexts.Length; i++)
                allTexts[i].UpdateLocale();
        }
    }
}