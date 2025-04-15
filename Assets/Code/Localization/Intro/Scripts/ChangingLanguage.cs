using UnityEngine;
using Localization;

public class ChangingLanguage : MonoBehaviour
{
    public void SetEnglish(){
        Localize.SetCurrentLanguage(SystemLanguage.English);
    }
    public void SetRussian(){
        Localize.SetCurrentLanguage(SystemLanguage.Russian);
    }
    public void SetUkrainian(){
        Localize.SetCurrentLanguage(SystemLanguage.Ukrainian);
    }
}
