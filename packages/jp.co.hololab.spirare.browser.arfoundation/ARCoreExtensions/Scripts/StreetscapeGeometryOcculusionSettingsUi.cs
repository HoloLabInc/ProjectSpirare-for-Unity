using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using static HoloLab.Spirare.Browser.DisplaySettingsState;

namespace HoloLab.Spirare.Browser.ARFoundation.ARCoreExtensions
{
    public class StreetscapeGeometryOcclusionSettingsUi : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("occulusionDropdown")]
        private TMP_Dropdown occlusionDropdown = null;

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

        private void Start()
        {
            UpdateDropdownOptions();

            displaySettingsState.OnStreetscapeGeometryOcclusionChanged += DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged;

            occlusionDropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            displaySettingsState.OnStreetscapeGeometryOcclusionChanged -= DisplaySettingsState_OnStreetscapeGeometryOcclusionChanged;
            occlusionDropdown.onValueChanged.RemoveListener(OnValueChanged);
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
            occlusionDropdown.options = dropdownOptions.ConvertAll(option => new TMP_Dropdown.OptionData(option.DropdownLabel));

            ChangeDropdownSelection(displaySettingsState.StreetscapeGeometryOcclusion);
        }

        private void ChangeDropdownSelection(StreetscapeGeometryOcclusionType occlusion)
        {
            var index = dropdownOptions.FindIndex(option => option.Occlusion == occlusion);
            occlusionDropdown.value = index;
        }
    }
}

