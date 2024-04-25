using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Browser
{
    public class LicensePanel : MonoBehaviour
    {
        [SerializeField]
        private LicensePanelState licensePanelState;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private TMP_Text licenseText;

        [SerializeField]
        private TextAsset licenseTextAsset;

        private void Start()
        {
            licenseText.text = licenseTextAsset.text;

            LicensePanelState_OnIsPanelOpenChanged();
            licensePanelState.OnIsPanelOpenChanged += LicensePanelState_OnIsPanelOpenChanged;

            closeButton.onClick.AddListener(CloseButton_OnClick);
        }

        private void LicensePanelState_OnIsPanelOpenChanged()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(licensePanelState.IsPanelOpen);
            }
        }

        private void CloseButton_OnClick()
        {
            licensePanelState.IsPanelOpen = false;
        }
    }
}

