using System.Collections.Generic;

namespace Code.Localization.Code.Services.LocalizeLanguageService
{
    public interface ILocalizeLanguageService
    {
        List<string> GetAvailableLanguages();
        void SetLanguage(string languageName);
        string GetCurrentLanguage();
    }   
}