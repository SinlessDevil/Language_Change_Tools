using TMPro;
using UnityEngine;

namespace Code.Localization.Code
{
    [DisallowMultipleComponent]
    public class TMP_Localizer : LocalizeBase
    {
        [SerializeField] private TMP_Text _text;

        private void OnValidate()
        {
            if(_text == null)
                _text = GetComponent<TMP_Text>();
        }

        public override void UpdateLocale()
        {
            if (!_text) 
                return;

            if (!System.String.IsNullOrEmpty(localizationKey) &&
                Locale.currentLanguageStrings.ContainsKey(localizationKey))
                _text.text = Locale.currentLanguageStrings[localizationKey].Replace(@"\n", "" + '\n');
        }
    }
}