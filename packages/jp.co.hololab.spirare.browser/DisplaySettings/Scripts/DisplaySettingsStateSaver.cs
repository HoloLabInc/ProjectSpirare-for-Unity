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

        private const string IsMenuOpenKey = "DisplaySettingsStateSaver_IsMenuOpen";
        private const string OpacityKey = "DisplaySettingsStateSaver_Opacity";

        private void Awake()
        {
            LoadState();

            displaySettingsState.OnIsMenuOpenChanged += DisplaySettingsState_OnIsMenuOpenChanged;
            displaySettingsState.OnOpacityChanged += DisplaySettingsState_OnOpacityChanged;
        }

        private void LoadState()
        {
            LoadIsMenuOpen();
            LoadOpacity();
        }

        private void LoadIsMenuOpen()
        {
            if (PlayerPrefsUtility.TryGetBoolean(IsMenuOpenKey, out var isMenuOpen))
            {
                displaySettingsState.IsMenuOpen = isMenuOpen;
            }
        }

        private void DisplaySettingsState_OnIsMenuOpenChanged(bool isMenuOpen)
        {
            PlayerPrefsUtility.SetBoolean(IsMenuOpenKey, isMenuOpen);
        }

        private void LoadOpacity()
        {
            if (PlayerPrefsUtility.TryGetFloat(OpacityKey, out var opacity))
            {
                displaySettingsState.Opacity = opacity;
            }
        }

        private void DisplaySettingsState_OnOpacityChanged(float opacity)
        {
            PlayerPrefs.SetFloat(OpacityKey, opacity);
        }
    }
}

