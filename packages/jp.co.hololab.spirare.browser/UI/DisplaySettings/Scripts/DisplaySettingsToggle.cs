using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.UI
{
    public class DisplaySettingsToggle : MonoBehaviour
    {
        [SerializeField]
        private ToggleButton toggleButton;

        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private void Start()
        {
            toggleButton.ChangeStateWithoutAnimation(displaySettingsState.IsMenuOpen);

            toggleButton.OnToggle += ToggleButton_OnToggle;
            displaySettingsState.OnIsMenuOpenChanged += DisplaySettingsState_OnIsMenuOpenChanged;
        }

        private void DisplaySettingsState_OnIsMenuOpenChanged()
        {
            toggleButton.IsOn = displaySettingsState.IsMenuOpen;
        }

        private void ToggleButton_OnToggle(bool isOn)
        {
            displaySettingsState.IsMenuOpen = isOn;
        }
    }
}

