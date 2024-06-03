using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static HoloLab.Spirare.Browser.DisplaySettingsState;

namespace HoloLab.Spirare.Browser.ARFoundation.ARCoreExtensions
{
    public class StreetscapeGeometryOcculusionSettingsUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown occulusionDropdown = null;

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private readonly List<(StreetscapeGeometryOcclusionType Occlusion, string DropdownLabel)> dropdownOptions
            = new List<(StreetscapeGeometryOcclusionType Occlusion, string DropdownLabel)>()
            {
                (StreetscapeGeometryOcclusionType.None, "Disabled"),
                (StreetscapeGeometryOcclusionType.Building, "Building"),
                (StreetscapeGeometryOcclusionType.Terrain, "Terrain"),
                (StreetscapeGeometryOcclusionType.All, "All"),
            };

        /*
        private readonly List<(OcclusionType Occlusion, string DropdownLabel)> defaultOptions
            = new List<(OcclusionType Occlusion, string DropdownLabel)>()
                {
                    (OcclusionType.None, "Disabled"),
                };

        private readonly List<(OcclusionType Occlusion, string DropdownLabel)> environmentOcclusionOptions
            = new List<(OcclusionType Occlusion, string DropdownLabel)>()
                {
                    (OcclusionType.EnvironmentFastest, "Environment: Fastest"),
                    (OcclusionType.EnvironmentMedium, "Environment: Medium"),
                    (OcclusionType.EnvironmentBest, "Environment: Best"),
                };

        private readonly List<(OcclusionType Occlusion, string DropdownLabel)> humanOcclusionOptions
            = new List<(OcclusionType Occlusion, string DropdownLabel)>()
                {
                    (OcclusionType.HumanFastest, "Human: Fastest"),
                    (OcclusionType.HumanBest, "Human: Best"),
                };
        */

        private void Start()
        {
            UpdateDropdownOptions();

            displaySettingsState.OnStreetscapeGeometryOcclusionChanged += DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged;

            occulusionDropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged(StreetscapeGeometryOcclusionType occlusion)
        {
            ChangeDropdownSelection(occlusion);
        }

        private void OnValueChanged(int selectedIndex)
        {
            if (0 <= selectedIndex && selectedIndex < dropdownOptions.Count)
            {
                var selectedOption = dropdownOptions[selectedIndex];
                displaySettingsState.StreetscapeGeometryOcclusion = selectedOption.Occlusion;
            }
        }

        private void UpdateDropdownOptions()
        {
            occulusionDropdown.options = dropdownOptions.ConvertAll(option => new TMP_Dropdown.OptionData(option.DropdownLabel));

            ChangeDropdownSelection(displaySettingsState.StreetscapeGeometryOcclusion);
        }

        private void ChangeDropdownSelection(StreetscapeGeometryOcclusionType occlusion)
        {
            var index = dropdownOptions.FindIndex(option => option.Occlusion == occlusion);
            occulusionDropdown.value = index;
        }
    }
}

