using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class MapScaleSlider : MonoBehaviour
    {
        [SerializeField]
        private float minScale = 0.0001f;

        [SerializeField]
        private float maxScale = 1f;

        [SerializeField]
        private Slider slider;

        [SerializeField]
        private TMP_Text text;

        private CesiumRectangleMap cesiumRectangleMap;

        private void Start()
        {
            cesiumRectangleMap = FindObjectOfType<CesiumRectangleMap>();

            if (cesiumRectangleMap != null)
            {
                CesiumRectangleMap_OnScaleChanged(cesiumRectangleMap.Scale);
                cesiumRectangleMap.OnScaleChanged += CesiumRectangleMap_OnScaleChanged;
            }

            slider.onValueChanged.AddListener(Slider_OnValueChanged);
        }

        private void Slider_OnValueChanged(float sliderValue)
        {
            if (cesiumRectangleMap == null)
            {
                return;
            }

            var scale = LogarithmicLerp(minScale, maxScale, sliderValue);
            cesiumRectangleMap.Scale = scale;

            text.text = $"Map scale: {scale}";
        }

        private void CesiumRectangleMap_OnScaleChanged(float scale)
        {
            var sliderValue = LogarithmicInverseLerp(minScale, maxScale, scale);
            slider.value = sliderValue;
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
