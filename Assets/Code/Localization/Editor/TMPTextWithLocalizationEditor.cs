#if UNITY_EDITOR
using Code.Localization.Code;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Code.Localization.Editor
{
    [CustomEditor(typeof(RectTransform))]
    [CanEditMultipleObjects]
    public class TMP_Text_ExtensionEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            RectTransform rectTransform = (RectTransform)target;
            TMP_Text text = rectTransform.GetComponent<TMP_Text>();
            if (text != null && rectTransform.GetComponent<TMP_Localizer>() == null)
            {
                rectTransform.gameObject.AddComponent<TMP_Localizer>();
            }
        }
    }
}
#endif