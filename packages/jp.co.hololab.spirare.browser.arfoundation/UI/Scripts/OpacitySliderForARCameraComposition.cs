using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class OpacitySliderForARCameraComposition : MonoBehaviour
    {
        [SerializeField]
        private Slider slider;

        [SerializeField]
        private TMP_Text valueText;

        private void Start()
        {
            ChangeValueText(slider.value);
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(float value)
        {
            ChangeValueText(value);
        }

        private void ChangeValueText(float value)
        {
            valueText.text = $"{(int)(value * 100)}%";
        }
    }
}
