using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static HoloLab.Spirare.Browser.DisplaySettingsState;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class OcclusionController : MonoBehaviour
    {
        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        AROcclusionManager arOcclusionManager = null;

        private void Start()
        {
            arOcclusionManager = FindObjectOfType<AROcclusionManager>();

            if (arOcclusionManager == null)
            {
                Debug.LogWarning($"{nameof(AROcclusionManager)} not found in scene");
                return;
            }

            SetOcclusion(displaySettingsState.Occlusion);

            displaySettingsState.OnOcclusionChanged += DisplaySettingsState_OnOcclusionChanged;
        }

        private void DisplaySettingsState_OnOcclusionChanged(OcclusionType occlusion)
        {
            SetOcclusion(occlusion);
        }

        private void SetOcclusion(OcclusionType occlusion)
        {
            arOcclusionManager.requestedEnvironmentDepthMode = OcclusionTypeToEnvironmentDepthMode(occlusion);
        }

        private static EnvironmentDepthMode OcclusionTypeToEnvironmentDepthMode(OcclusionType occlusion)
        {
            return occlusion switch
            {
                OcclusionType.None => EnvironmentDepthMode.Disabled,
                OcclusionType.EnvironmentFastest => EnvironmentDepthMode.Fastest,
                OcclusionType.EnvironmentMedium => EnvironmentDepthMode.Medium,
                OcclusionType.EnvironmentBest => EnvironmentDepthMode.Best,
                _ => EnvironmentDepthMode.Disabled,
            };
        }
    }
}

