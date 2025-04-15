using UnityEngine;
using TMPro;

namespace Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class Localize : LocalizBase
    {
        private TMP_Text _text;

        protected override void Start()
        {
            _text = GetComponent<TMP_Text>();
            base.Start();
        }

        public override void UpdateLocale()
        {
            if (!_text) return;

            if (!System.String.IsNullOrEmpty(localizationKey) &&
                Locale.currentLanguageStrings.ContainsKey(localizationKey))
                _text.text = Locale.currentLanguageStrings[localizationKey].Replace(@"\n", "" + '\n');
            ;
        }
    }
}