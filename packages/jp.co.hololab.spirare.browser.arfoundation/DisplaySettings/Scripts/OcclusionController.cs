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
            arOcclusionManager.requestedOcclusionPreferenceMode = OcclusionTypeToPreferenceMode(occlusion);
            arOcclusionManager.requestedEnvironmentDepthMode = OcclusionTypeToEnvironmentDepthMode(occlusion);
            arOcclusionManager.requestedHumanDepthMode = OcclusionTypeToHumanDepthMode(occlusion);
            arOcclusionManager.requestedHumanStencilMode = OcclusionTypeToHumanStencilMode(occlusion);
        }

        private OcclusionPreferenceMode OcclusionTypeToPreferenceMode(OcclusionType occlusion)
        {
            return occlusion switch
            {
                OcclusionType.None => OcclusionPreferenceMode.NoOcclusion,
                OcclusionType.EnvironmentFastest => OcclusionPreferenceMode.PreferEnvironmentOcclusion,
                OcclusionType.EnvironmentMedium => OcclusionPreferenceMode.PreferEnvironmentOcclusion,
                OcclusionType.EnvironmentBest => OcclusionPreferenceMode.PreferEnvironmentOcclusion,
                OcclusionType.HumanFastest => OcclusionPreferenceMode.PreferHumanOcclusion,
                OcclusionType.HumanBest => OcclusionPreferenceMode.PreferHumanOcclusion,
                _ => OcclusionPreferenceMode.PreferEnvironmentOcclusion,
            };
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

        private static HumanSegmentationDepthMode OcclusionTypeToHumanDepthMode(OcclusionType occlusion)
        {
            return occlusion switch
            {
                OcclusionType.None => HumanSegmentationDepthMode.Disabled,
                OcclusionType.HumanFastest => HumanSegmentationDepthMode.Fastest,
                OcclusionType.HumanBest => HumanSegmentationDepthMode.Best,
                _ => HumanSegmentationDepthMode.Disabled,
            };
        }

        private static HumanSegmentationStencilMode OcclusionTypeToHumanStencilMode(OcclusionType occlusion)
        {
            return occlusion switch
            {
                OcclusionType.None => HumanSegmentationStencilMode.Disabled,
                OcclusionType.HumanFastest => HumanSegmentationStencilMode.Fastest,
                OcclusionType.HumanBest => HumanSegmentationStencilMode.Best,
                _ => HumanSegmentationStencilMode.Disabled,
            };
        }
    }
}
