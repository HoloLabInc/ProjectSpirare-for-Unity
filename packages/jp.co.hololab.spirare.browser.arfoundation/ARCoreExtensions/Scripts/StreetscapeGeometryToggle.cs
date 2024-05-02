using HoloLab.Spirare.Browser.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.ARFoundation.ARCoreExtensions
{
    public class StreetscapeGeometryToggle : MonoBehaviour
    {
        [SerializeField]
        private ToggleButton toggleButton;

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private void Start()
        {
            toggleButton.ChangeStateWithoutAnimation(displaySettingsState.StreetscapeGeometryOcclusionEnabled);
            displaySettingsState.OnStreetscapeGeometryOcclusionEnabledChanged += DisplaySettingsState_OnStreetscapeGeometryOcclusionEnabledChanged;

            toggleButton.OnToggle += ToggleButton_OnToggle;
        }

        private void DisplaySettingsState_OnStreetscapeGeometryOcclusionEnabledChanged(bool enabled)
        {
            toggleButton.IsOn = enabled;
        }

        private void ToggleButton_OnToggle(bool value)
        {
            displaySettingsState.StreetscapeGeometryOcclusionEnabled = value;
        }
    }
}

