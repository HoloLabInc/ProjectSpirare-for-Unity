using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static HoloLab.Spirare.Browser.DisplaySettingsState;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    public class OcculusionSettingsUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown occulusionDropdown = null;

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private readonly List<(OcclusionType Occlusion, string DropdownLabel)> dropdownOptions
            = new List<(OcclusionType Occlusion, string DropdownLabel)>();

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

        private void Start()
        {
            occulusionDropdown.interactable = false;
            UpdateDropdownOptions(false, false);

            displaySettingsState.OnOcclusionChanged += DisplaySettingsState_OnOcclusionChanged;

            occulusionDropdown.onValueChanged.AddListener(OnValueChanged);

            ARSession.stateChanged += ARSession_stateChanged;
            ARSession_stateChanged(new ARSessionStateChangedEventArgs(ARSession.state));

#if UNITY_EDITOR
            InitializeInEditor();
#endif
        }

        private void DisplaySettingsState_OnOcclusionChanged(OcclusionType occlusion)
        {
            ChangeDropdownSelection(occlusion);
        }

        private void OnValueChanged(int selectedIndex)
        {
            if (0 <= selectedIndex && selectedIndex < dropdownOptions.Count)
            {
                var selectedOption = dropdownOptions[selectedIndex];
                displaySettingsState.Occlusion = selectedOption.Occlusion;
            }
        }

        private async void ARSession_stateChanged(ARSessionStateChangedEventArgs args)
        {
            switch (args.state)
            {
                case ARSessionState.Ready:
                case ARSessionState.SessionInitializing:
                case ARSessionState.SessionTracking:
                    ARSession.stateChanged -= ARSession_stateChanged;
                    await InitializeDropdownAsync();
                    break;
            }
        }

        private async void InitializeInEditor()
        {
            await Task.Delay(3000);
            UpdateDropdownOptions(true, true);
            occulusionDropdown.interactable = true;
        }

        private async Task InitializeDropdownAsync()
        {
            var (environmentOcclusionEnabled, humanOcclusionEnabled) = await CheckAvailableOcclusionAsync();
            UpdateDropdownOptions(environmentOcclusionEnabled, humanOcclusionEnabled);
            occulusionDropdown.interactable = true;
        }

        private void UpdateDropdownOptions(bool environmentOcclusionEnabled, bool humanOcclusionEnabled)
        {
            dropdownOptions.Clear();
            dropdownOptions.AddRange(defaultOptions);

            if (environmentOcclusionEnabled)
            {
                dropdownOptions.AddRange(environmentOcclusionOptions);
            }
            if (humanOcclusionEnabled)
            {
                dropdownOptions.AddRange(humanOcclusionOptions);
            }

            occulusionDropdown.options = dropdownOptions.ConvertAll(option => new TMP_Dropdown.OptionData(option.DropdownLabel));

            ChangeDropdownSelection(displaySettingsState.Occlusion);
        }

        private async Task<(bool EnvironmentOcclusionSupported, bool PeopleOcclusionSupported)> CheckAvailableOcclusionAsync()
        {
            var arOcclusionManager = FindObjectOfType<AROcclusionManager>();
            if (arOcclusionManager == null)
            {
                return (false, false);
            }

            var xrOcclusionSubsystem = arOcclusionManager.subsystem;
            if (xrOcclusionSubsystem == null)
            {
                return (false, false);
            }

            var descriptor = xrOcclusionSubsystem.subsystemDescriptor;

            var environmentOcclusionSupported = await CheckSupportedAsync(() => descriptor.environmentDepthImageSupported);
            var peopleOcclusionSupported = await CheckSupportedAsync(() => descriptor.humanSegmentationStencilImageSupported);

            return (environmentOcclusionSupported, peopleOcclusionSupported);
        }

        private void ChangeDropdownSelection(OcclusionType occlusion)
        {
            var index = dropdownOptions.FindIndex(option => option.Occlusion == occlusion);
            occulusionDropdown.value = index;
        }

        private static async Task<bool> CheckSupportedAsync(Func<Supported> getSupportedFunc)
        {
            while (true)
            {
                var supported = getSupportedFunc();
                if (supported == Supported.Unknown)
                {
                    await Task.Delay(100);
                    continue;
                }
                else
                {
                    return supported == Supported.Supported;
                }
            }
        }
    }
}

