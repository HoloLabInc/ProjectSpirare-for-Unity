using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    [RequireComponent(typeof(ARCameraManager))]
    public class ARLightEstimation : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("light")]
        private Light sceneLight;

        private ARCameraManager cameraManager;

        private void OnEnable()
        {
            cameraManager = GetComponent<ARCameraManager>();
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
                sceneLight.intensity = intensity;
                RenderSettings.ambientIntensity = intensity;
            }

            var colorTemperature = e.lightEstimation.averageColorTemperature;
            if (colorTemperature.HasValue)
            {
                sceneLight.colorTemperature = colorTemperature.Value;
            }
        }
    }
}
