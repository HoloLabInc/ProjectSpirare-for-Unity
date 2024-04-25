using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser.UI
{
    public class DisplaySettingsPanel : MonoBehaviour
    {
        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        [SerializeField]
        private GameObject panelRoot;

        [SerializeField]
        private Button closeButton;

        private void Start()
        {
            closeButton.onClick.AddListener(() =>
            {
                displaySettingsState.IsMenuOpen = false;
            });

            DisplaySettingsState_OnIsMenuOpenChanged();
            displaySettingsState.OnIsMenuOpenChanged += DisplaySettingsState_OnIsMenuOpenChanged;
        }

        private void DisplaySettingsState_OnIsMenuOpenChanged()
        {
            panelRoot.SetActive(displaySettingsState.IsMenuOpen);
        }
    }
}

