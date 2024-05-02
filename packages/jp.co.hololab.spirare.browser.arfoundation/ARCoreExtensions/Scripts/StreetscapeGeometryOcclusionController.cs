using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.ARFoundation.ARCoreExtensions
{
    public class StreetscapeGeometryOcclusionController : MonoBehaviour
    {
        [SerializeField]
        private GameObject streetscapeGeometryController;

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private void Start()
        {
            SetStreetscapeGeometryControllerActive(displaySettingsState.StreetscapeGeometryOcclusionEnabled);

            displaySettingsState.OnStreetscapeGeometryOcclusionEnabledChanged += DisplaySettingsState_OnStreetscapeGeometryOcclusionEnabledChanged;
        }

        private void DisplaySettingsState_OnStreetscapeGeometryOcclusionEnabledChanged(bool enabled)
        {
            SetStreetscapeGeometryControllerActive(enabled);
        }

        private void SetStreetscapeGeometryControllerActive(bool enabled)
        {
            streetscapeGeometryController.SetActive(enabled);
        }
    }
}

