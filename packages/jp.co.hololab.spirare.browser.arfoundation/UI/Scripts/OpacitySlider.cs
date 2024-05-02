using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class OpacitySlider : MonoBehaviour
    {
        [SerializeField]
        private Slider slider;

        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private void Start()
        {
            DisplaySettingsState_OnOpacityChanged(displaySettingsState.Opacity);
            displaySettingsState.OnOpacityChanged += DisplaySettingsState_OnOpacityChanged;

            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void DisplaySettingsState_OnOpacityChanged(float opacity)
        {
            slider.value = Mathf.Clamp01(opacity);
            ChangeValueText(slider.value);
        }

        private void OnValueChanged(float value)
        {
            ChangeValueText(value);
            displaySettingsState.Opacity = value;
        }

        private void ChangeValueText(float value)
        {
            valueText.text = $"{(int)(value * 100)}%";
        }
    }
}

