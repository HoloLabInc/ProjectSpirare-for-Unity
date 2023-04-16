using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class ARLightEstimation : MonoBehaviour
    {
        [SerializeField]
        private ARCameraManager cameraManager;

        [SerializeField]
        private Light light;

        private void OnEnable()
        {
            cameraManager.frameReceived += OnCameraFrameReceived;
        }

        private void OnDisable()
        {
            cameraManager.frameReceived -= OnCameraFrameReceived;
        }

        private void Awake()
        {
            DynamicGI.UpdateEnvironment();
        }

        private void OnCameraFrameReceived(ARCameraFrameEventArgs e)
        {
            var averageBrightness = e.lightEstimation.averageBrightness;
            if (averageBrightness.HasValue)
            {
                var intensity = Mathf.Clamp01(averageBrightness.Value * 2);
                light.intensity = intensity;
                RenderSettings.ambientIntensity = intensity;
            }

            var colorTemperature = e.lightEstimation.averageColorTemperature;
            if (colorTemperature.HasValue)
            {
                light.colorTemperature = colorTemperature.Value;
            }
        }
    }
}
