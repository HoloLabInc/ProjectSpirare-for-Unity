using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class PointCloudRenderSettingsStateSaver : MonoBehaviour
    {
        [SerializeField]
        private PointCloudRenderSettings renderSettings;

        private const string PointSizeKey = "PointCloudRenderSettings_PointSize";

        private void Awake()
        {
            LoadState();

            renderSettings.OnPointSizeChanged += RenderSettings_OnPointSizeChanged;
        }


        private void OnDestroy()
        {
            renderSettings.OnPointSizeChanged -= RenderSettings_OnPointSizeChanged;
        }

        private void LoadState()
        {
            LoadPointSize();
        }

        private void LoadPointSize()
        {
            var value = PlayerPrefs.GetFloat(PointSizeKey, float.NaN);
            if (float.IsNaN(value) == false)
            {
                renderSettings.PointSize = value;
            }
        }

        private void RenderSettings_OnPointSizeChanged(float pointSize)
        {
            PlayerPrefs.SetFloat(PointSizeKey, pointSize);
        }
    }
}

