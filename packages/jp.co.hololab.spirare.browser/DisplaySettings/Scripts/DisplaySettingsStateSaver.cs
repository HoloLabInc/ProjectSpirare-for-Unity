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
            var isMenuOpen = PlayerPrefs.GetInt("IsMenuOpen", -1);
            if (isMenuOpen == 0)
            {
                displaySettingsState.IsMenuOpen = false;
            }
            else if (isMenuOpen == 1)
            {
                displaySettingsState.IsMenuOpen = true;
            }
        }

        private void DisplaySettingsState_OnIsMenuOpenChanged()
        {
            PlayerPrefs.SetInt("IsMenuOpen", displaySettingsState.IsMenuOpen ? 1 : 0);
        }
    }
}
