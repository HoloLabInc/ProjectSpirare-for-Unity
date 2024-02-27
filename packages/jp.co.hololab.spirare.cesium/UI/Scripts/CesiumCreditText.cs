using CesiumForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Cesium.UI
{
    public class CesiumCreditText : MonoBehaviour
    {
        private TMP_Text tmpText;
        private Text unityText;

        private CesiumCreditSystemStringConverter creditSystemStringConverter;

        private void Start()
        {
            tmpText = GetComponent<TMP_Text>();
            unityText = GetComponent<Text>();

            CesiumCreditSystemStringConverter_OnCreditsUpdated("");

            creditSystemStringConverter = FindObjectOfType<CesiumCreditSystemStringConverter>();
            creditSystemStringConverter.OnCreditsUpdated += CesiumCreditSystemStringConverter_OnCreditsUpdated;
        }

        private void CesiumCreditSystemStringConverter_OnCreditsUpdated(string credit)
        {
            if (tmpText != null)
            {
                tmpText.text = credit;
            }
            if (unityText != null)
            {
                unityText.text = credit;
            }
        }
    }
}
