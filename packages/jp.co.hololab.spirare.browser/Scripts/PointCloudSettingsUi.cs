using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser
{
    public class PointCloudSettingsUi : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Min point size in meter")]
        private float minPointSize = 0.0001f;

        [SerializeField]
        [Tooltip("Max point size in meter")]
        private float maxPointSize = 1f;

        [SerializeField]
        private PointCloudRenderSettings pointCloudRenderSettings;

        [SerializeField]
        private Slider pointSizeSlider;

        [SerializeField]
        private TMP_Text pointSizeText;

        private const string pointSizeSaveKey = "PointCloudSettingsUi_pointSizeKey";

        private void Awake()
        {
            LoadPointSize();

            pointSizeSlider.onValueChanged.AddListener(PointSizeSlider_OnValueChanged);
        }

        private void PointSizeSlider_OnValueChanged(float value)
        {
            if (pointCloudRenderSettings == null)
            {
                return;
            }

            float pointSize = 0;
            if (value > 0)
            {
                pointSize = LogarithmicLerp(minPointSize, maxPointSize, value);
            }
            pointCloudRenderSettings.PointSize = pointSize;

            SavePointSize(pointSize);

            // Update point size text
            var pointSizeInMillimeter = pointSize * 1000;
            string pointSizeString;
            if (pointSize > 0.01)
            {
                pointSizeString = Mathf.Round(pointSizeInMillimeter).ToString();
            }
            else if (pointSize > 0.001)
            {
                pointSizeString = pointSizeInMillimeter.ToString("F1");
            }
            else
            {
                pointSizeString = pointSizeInMillimeter.ToString("F2");
            }

            pointSizeText.text = $"Point cloud point size: {pointSizeString}mm";
        }

        private void SavePointSize(float pointSize)
        {
            PlayerPrefs.SetFloat(pointSizeSaveKey, pointSize);
            PlayerPrefs.Save();
        }

        private void LoadPointSize()
        {
            var pointSize = PlayerPrefs.GetFloat(pointSizeSaveKey, 0f);

            float sliderValue = 0;
            if (pointSize > 0f)
            {
                sliderValue = LogarithmicInverseLerp(minPointSize, maxPointSize, pointSize);
            }

            pointSizeSlider.value = sliderValue;
            PointSizeSlider_OnValueChanged(pointSizeSlider.value);
        }

        private static float LogarithmicLerp(float a, float b, float t)
        {
            var logA = Mathf.Log(a);
            var logB = Mathf.Log(b);

            return Mathf.Exp(Mathf.Lerp(logA, logB, t));
        }

        private static float LogarithmicInverseLerp(float a, float b, float t)
        {
            var logA = Mathf.Log(a);
            var logB = Mathf.Log(b);
            return Mathf.InverseLerp(logA, logB, Mathf.Log(t));
        }
    }
}
