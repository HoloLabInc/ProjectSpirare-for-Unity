using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using static HoloLab.Spirare.Browser.DisplaySettingsState;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class OcculusionSettingsUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown occulusionDropdown = null;

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private List<(OcclusionType Occlusion, string DropdownLabel)> dropdownOptions
            = new List<(OcclusionType Occlusion, string DropdownLabel)>()
                {
                    (OcclusionType.None, "Disabled"),
                    (OcclusionType.EnvironmentFastest, "Fastest"),
                    (OcclusionType.EnvironmentMedium, "Medium"),
                    (OcclusionType.EnvironmentBest, "Best"),
                };

        private void Start()
        {
            occulusionDropdown.options = dropdownOptions.ConvertAll(option => new TMP_Dropdown.OptionData(option.DropdownLabel));

            ChangeDropdownSelection(displaySettingsState.Occlusion);
            displaySettingsState.OnOcclusionChanged += DisplaySettingsState_OnOcclusionChanged;

            occulusionDropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void DisplaySettingsState_OnOcclusionChanged(OcclusionType occlusion)
        {
            ChangeDropdownSelection(occlusion);
        }

        private void ChangeDropdownSelection(OcclusionType occlusion)
        {
            var index = dropdownOptions.FindIndex(option => option.Occlusion == occlusion);
            occulusionDropdown.value = index;
        }

        private void OnValueChanged(int selectedIndex)
        {
            if (0 <= selectedIndex && selectedIndex < dropdownOptions.Count)
            {
                var selectedOption = dropdownOptions[selectedIndex];
                displaySettingsState.Occlusion = selectedOption.Occlusion;
            }
        }
    }
}

