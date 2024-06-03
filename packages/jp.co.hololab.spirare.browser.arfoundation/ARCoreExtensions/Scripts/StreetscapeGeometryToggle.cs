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
            toggleButton.ChangeStateWithoutAnimation(displaySettingsState.StreetscapeGeometryOcclusion == DisplaySettingsState.StreetscapeGeometryOcclusionType.All);
            displaySettingsState.OnStreetscapeGeometryOcclusionChanged += DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged;

            toggleButton.OnToggle += ToggleButton_OnToggle;
        }

        private void DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged(DisplaySettingsState.StreetscapeGeometryOcclusionType occlusionType)
        {
            toggleButton.IsOn = occlusionType == DisplaySettingsState.StreetscapeGeometryOcclusionType.All;
        }

        private void ToggleButton_OnToggle(bool value)
        {
            //displaySettingsState.StreetscapeGeometryOcclusion = value;
            if (value)
            {
                displaySettingsState.StreetscapeGeometryOcclusion = DisplaySettingsState.StreetscapeGeometryOcclusionType.All;
            }
            else
            {
                displaySettingsState.StreetscapeGeometryOcclusion = DisplaySettingsState.StreetscapeGeometryOcclusionType.None;
            }
        }
    }
}

