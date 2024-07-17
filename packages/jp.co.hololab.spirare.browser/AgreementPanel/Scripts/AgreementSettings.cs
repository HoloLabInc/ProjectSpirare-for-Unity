using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class AgreementSettings : ScriptableObject
    {
        [Multiline]
        public string AgreementText;

        public string PlayerPrefsKey;
    }
}

