using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.UI
{
    public class DisplaySettingsPanel : MonoBehaviour
    {
        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        [SerializeField]
        private GameObject panelRoot;

        private void Start()
        {
            DisplaySettingsState_OnIsMenuOpenChanged();
            displaySettingsState.OnIsMenuOpenChanged += DisplaySettingsState_OnIsMenuOpenChanged;
        }

        private void DisplaySettingsState_OnIsMenuOpenChanged()
        {
            panelRoot.SetActive(displaySettingsState.IsMenuOpen);
        }
    }
}

