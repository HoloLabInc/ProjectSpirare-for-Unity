using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser.UI
{
    public class ShowLicenseButton : MonoBehaviour
    {
        [SerializeField]
        private LicensePanelState licensePanelState;

        private Button button;

        private SideMenu sideMenu;

        private void Awake()
        {
            sideMenu = GetComponentInParent<SideMenu>();
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            licensePanelState.IsPanelOpen = true;
            if (sideMenu != null)
            {
                sideMenu.CloseMenu();
            }
        }
    }
}

