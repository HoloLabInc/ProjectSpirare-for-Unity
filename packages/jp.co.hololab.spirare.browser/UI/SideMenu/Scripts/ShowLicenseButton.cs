using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HoloLab.Spirare.Browser.UI
{
    public class ShowLicenseButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IDragHandler
    {
        [SerializeField]
        private LicensePanelState licensePanelState;

        [SerializeField]
        private DeveloperModeState developerModeState;

        private float? pointerDownTime;

        private float longTapDuration = 2.0f;

        private SideMenu sideMenu;

        private void Awake()
        {
            sideMenu = GetComponentInParent<SideMenu>();
        }

        private void Update()
        {
            if (pointerDownTime.HasValue)
            {
                if (Time.realtimeSinceStartup - pointerDownTime > longTapDuration)
                {
                    ToggleDeveloperMode();
                    pointerDownTime = null;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownTime = Time.realtimeSinceStartup;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerDownTime.HasValue)
            {
                ShowLicense();
            }

            pointerDownTime = null;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerDownTime = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Do nothing
        }

        private void ShowLicense()
        {
            licensePanelState.IsPanelOpen = true;
            if (sideMenu != null)
            {
                sideMenu.CloseMenu();
            }
        }

        private void ToggleDeveloperMode()
        {
            developerModeState.Enabled = !developerModeState.Enabled;
        }
    }
}

