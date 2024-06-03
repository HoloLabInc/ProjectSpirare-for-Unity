using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HoloLab.Spirare.Browser.DisplaySettingsState;

namespace HoloLab.Spirare.Browser.ARFoundation.ARCoreExtensions
{
    public class StreetscapeGeometryOcclusionController : MonoBehaviour
    {
        [SerializeField]
        private StreetscapeGeometryController streetscapeGeometryController;

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private void Start()
        {
            SetStreetscapeGeometryControllerOcclusionType(displaySettingsState.StreetscapeGeometryOcclusion);

            displaySettingsState.OnStreetscapeGeometryOcclusionChanged += DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged;
        }

        private void DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged(StreetscapeGeometryOcclusionType occlusionType)
        {
            SetStreetscapeGeometryControllerOcclusionType(occlusionType);
        }

        private void SetStreetscapeGeometryControllerOcclusionType(StreetscapeGeometryOcclusionType occlusionType)
        {
            var controllerOcclusionType = occlusionType switch
            {
                StreetscapeGeometryOcclusionType.None => StreetscapeGeometryController.StreetscapeGeometryOcclusionType.None,
                StreetscapeGeometryOcclusionType.Building => StreetscapeGeometryController.StreetscapeGeometryOcclusionType.Building,
                StreetscapeGeometryOcclusionType.Terrain => StreetscapeGeometryController.StreetscapeGeometryOcclusionType.Terrain,
                StreetscapeGeometryOcclusionType.All => StreetscapeGeometryController.StreetscapeGeometryOcclusionType.All,
                _ => StreetscapeGeometryController.StreetscapeGeometryOcclusionType.None
            };

            streetscapeGeometryController.SetOcclusionType(controllerOcclusionType);
        }
    }
}

