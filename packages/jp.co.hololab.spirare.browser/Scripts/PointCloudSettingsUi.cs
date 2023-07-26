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

        private void Awake()
        {
            pointSizeSlider.onValueChanged.AddListener(PointSizeSlider_OnValueChanged);
        }

        private void PointSizeSlider_OnValueChanged(float value)
        {
            if (pointCloudRenderSettings != null)
            {
                float pointSize = 0;
                if (value > 0)
                {
                    pointSize = LogarithmicLerp(minPointSize, maxPointSize, value);
                }
                pointCloudRenderSettings.PointSize = pointSize;

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
