using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class PointCloudSizeSlider : MonoBehaviour
    {
        [SerializeField]
        private float sizeMin = 0.0001f;

        [SerializeField]
        private float sizeMax = 0.1f;

        [SerializeField]
        private TMP_Text pointCloudSizeText = null;

        [SerializeField]
        private Slider pointCloudSizeSlider = null;

        [SerializeField]
        private PointCloudRenderSettings renderSettings;

        private void Start()
        {
            UpdateSliderValue(renderSettings.PointSize);
            renderSettings.OnPointSizeChanged += RenderSettings_OnPointSizeChanged;

            pointCloudSizeSlider.onValueChanged.AddListener(PointCloudSizeSlider_OnValueChanged);
        }

        private void OnDestroy()
        {
            renderSettings.OnPointSizeChanged -= RenderSettings_OnPointSizeChanged;
            pointCloudSizeSlider.onValueChanged.RemoveListener(PointCloudSizeSlider_OnValueChanged);
        }

        private void RenderSettings_OnPointSizeChanged(float pointSize)
        {
            UpdateSliderValue(pointSize);
        }

        private void UpdateSliderValue(float pointSize)
        {
            pointCloudSizeSlider.value = UiUtils.LogarithmicInverseLerp(sizeMin, sizeMax, pointSize);
            UpdatePointSizeText(pointSize);
        }

        private void UpdatePointSizeText(float pointSize)
        {
            string pointSizeText;
            if (pointSize <= 0)
            {
                pointSizeText = $"1px";
            }
            else if (pointSize > 0.001f)
            {
                pointSizeText = $"{(int)Mathf.Round(pointSize * 1000)}mm";
            }
            else
            {
                pointSizeText = $"{(pointSize * 1000):F1}mm";
            }

            pointCloudSizeText.text = pointSizeText;
        }

        private void PointCloudSizeSlider_OnValueChanged(float value)
        {
            float pointSize;
            if (value == 0)
            {
                pointSize = 0;
            }
            else
            {
                pointSize = UiUtils.LogarithmicLerp(sizeMin, sizeMax, value);
            }

            UpdatePointSizeText(pointSize);
            renderSettings.PointSize = pointSize;
        }
    }
}

