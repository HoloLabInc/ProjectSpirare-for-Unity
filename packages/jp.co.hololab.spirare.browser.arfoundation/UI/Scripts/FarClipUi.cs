using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class FarClipUi : MonoBehaviour
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
        private Camera[] cameraList = null;

        private void Start()
        {
            if (cameraList == null || cameraList.Length == 0)
            {
                cameraList = new Camera[] { Camera.main };
            }

            // Reflect the initial value on the slider.
            var firstCamera = cameraList[0];
            farClipSlider.value = LogarithmicInverseLerp(farClipMin, farClipMax, firstCamera.farClipPlane);
            FarClipSlider_OnValueChanged(farClipSlider.value);

            farClipSlider.onValueChanged.AddListener(FarClipSlider_OnValueChanged);
        }

        private void FarClipSlider_OnValueChanged(float value)
        {
            var farClipLength = LogarithmicLerp(farClipMin, farClipMax, value);
            foreach (var camera in cameraList)
            {
                camera.farClipPlane = farClipLength;
            }
            farClipText.text = $"Far clip: {(int)farClipLength}m";
        }

        private float LogarithmicLerp(float a, float b, float t)
        {
            var logA = Mathf.Log(a);
            var logB = Mathf.Log(b);

            return Mathf.Exp(Mathf.Lerp(logA, logB, t));
        }

        private float LogarithmicInverseLerp(float a, float b, float t)
        {
            var logA = Mathf.Log(a);
            var logB = Mathf.Log(b);
            return Mathf.InverseLerp(logA, logB, Mathf.Log(t));
        }
    }
}
