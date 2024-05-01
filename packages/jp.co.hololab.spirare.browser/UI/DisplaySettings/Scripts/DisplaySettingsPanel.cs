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
            ChangeScale();

            closeButton.onClick.AddListener(() =>
            {
                displaySettingsState.IsMenuOpen = false;
            });

            DisplaySettingsState_OnIsMenuOpenChanged();
            displaySettingsState.OnIsMenuOpenChanged += DisplaySettingsState_OnIsMenuOpenChanged;
        }

        private void ChangeScale()
        {
            var screenWidth = Screen.width / Screen.dpi;
            var screenHeight = Screen.height / Screen.dpi;
            var diagonalInches = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);

            if (diagonalInches >= 7)
            {
                transform.localScale = Vector3.one * 1.5f;
            }
        }

        private void DisplaySettingsState_OnIsMenuOpenChanged()
        {
            panelRoot.SetActive(displaySettingsState.IsMenuOpen);
        }
    }
}

