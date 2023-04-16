using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class OcculusionSettingsUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown environmentOcculusionDropdown = null;

        AROcclusionManager arOcclusionManager = null;

        void Start()
        {
            arOcclusionManager = FindObjectOfType<AROcclusionManager>();
            environmentOcculusionDropdown.onValueChanged.AddListener(OnValueChanged);

            // Reflect the initial value.
            UpdateOcclusionSetting();
        }

        private void OnValueChanged(int selectedIndex)
        {
            UpdateOcclusionSetting();
        }

        private void UpdateOcclusionSetting()
        {
            var selectedOption = environmentOcculusionDropdown.options[environmentOcculusionDropdown.value];
            arOcclusionManager.requestedEnvironmentDepthMode = StringToEnvironmentDepthMode(selectedOption.text);
        }

        private static EnvironmentDepthMode StringToEnvironmentDepthMode(string depthMode)
        {
            switch (depthMode.ToLower())
            {
                case "disabled":
                    return EnvironmentDepthMode.Disabled;
                case "fastest":
                    return EnvironmentDepthMode.Fastest;
                case "medium":
                    return EnvironmentDepthMode.Medium;
                case "best":
                    return EnvironmentDepthMode.Best;
                default:
                    return EnvironmentDepthMode.Disabled;
            }
        }
    }
}
