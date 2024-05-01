using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class DisplaySettingsStateSaver : MonoBehaviour
    {
        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        private void Awake()
        {
            LoadState();
            displaySettingsState.OnIsMenuOpenChanged += DisplaySettingsState_OnIsMenuOpenChanged;
        }

        private void LoadState()
        {
            LoadIsMenuOpen();
        }

        private void LoadIsMenuOpen()
        {
            if (PlayerPrefsUtility.TryGetBoolean("IsMenuOpen", out var isMenuOpen))
            {
                displaySettingsState.IsMenuOpen = isMenuOpen;
            }
        }

        private void DisplaySettingsState_OnIsMenuOpenChanged(bool isMenuOpen)
        {
            PlayerPrefsUtility.SetBoolean("IsMenuOpen", isMenuOpen);
        }
    }
}

