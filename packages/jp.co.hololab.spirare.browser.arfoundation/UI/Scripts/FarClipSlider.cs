using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class FarClipSlider : MonoBehaviour
    {
        [SerializeField]
        private float farClipMin = 5;

        [SerializeField]
        private float farClipMax = 1000;

        [SerializeField]
        private TMP_Text farClipText = null;

        [SerializeField]
        private Slider farClipSlider = null;

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private void Start()
        {
            UpdateSliderValue(displaySettingsState.FarClip);
            displaySettingsState.OnFarClipChanged += DisplaySettingsState_OnFarClipChanged;

            farClipSlider.onValueChanged.AddListener(FarClipSlider_OnValueChanged);
        }

        private void OnDestroy()
        {
            displaySettingsState.OnFarClipChanged -= DisplaySettingsState_OnFarClipChanged;
            farClipSlider.onValueChanged.RemoveListener(FarClipSlider_OnValueChanged);
        }

        private void DisplaySettingsState_OnFarClipChanged(float farClip)
        {
            UpdateSliderValue(farClip);
        }

        private void UpdateSliderValue(float farClip)
        {
            farClipSlider.value = UiUtils.LogarithmicInverseLerp(farClipMin, farClipMax, farClip);
            UpdateFarClipText(farClip);
        }

        private void UpdateFarClipText(float farClip)
        {
            farClipText.text = $"{(int)farClip}m";
        }

        private void FarClipSlider_OnValueChanged(float value)
        {
            var farClip = UiUtils.LogarithmicLerp(farClipMin, farClipMax, value);

            UpdateFarClipText(farClip);
            displaySettingsState.FarClip = farClip;
        }
    }
}

