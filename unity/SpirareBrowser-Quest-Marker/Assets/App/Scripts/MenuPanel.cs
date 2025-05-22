using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Quest
{
    public class MenuPanel : MonoBehaviour
    {
        [SerializeField]
        private float distanceFromCamera = 1f;

        [SerializeField]
        private ScrollRect contentScrollRect;

        [SerializeField]
        private List<MenuItem> menuItems = new List<MenuItem>();

        private Canvas menuCanvas;

        private enum MenuType
        {
            Content = 0,
            License
        }

        [Serializable]
        private class MenuItem
        {
            public MenuType menuType;
            public Toggle navToggle;
            public GameObject content;
        }

        private void Start()
        {
            menuCanvas = GetComponentInChildren<Canvas>();
            menuCanvas.gameObject.SetActive(false);

            OnNavToggleSelected(menuItems[0]);

            foreach (var menuItem in menuItems)
            {
                menuItem.navToggle.onValueChanged.AddListener(isToggled =>
                {
                    if (isToggled)
                    {
                        OnNavToggleSelected(menuItem);
                    }
                    else
                    {
                        menuItem.navToggle.SetIsOnWithoutNotify(true);
                    }
                });
            }
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.Button.Start))
            {
                ToggleMenu();
            }
        }

        private void ToggleMenu()
        {
            var active = !menuCanvas.gameObject.activeSelf;
            if (active)
            {
                var cameraTransform = Camera.main.transform;
                var forwardHorizontal = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                var position = cameraTransform.position + forwardHorizontal * distanceFromCamera;
                menuCanvas.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forwardHorizontal));
            }

            menuCanvas.gameObject.SetActive(active);
        }

        private void OnNavToggleSelected(MenuItem selectedMenuItem)
        {
            foreach (var menuItem in menuItems)
            {
                if (menuItem == selectedMenuItem)
                {
                    menuItem.navToggle.SetIsOnWithoutNotify(true);
                    menuItem.content.SetActive(true);
                }
                else
                {
                    menuItem.navToggle.SetIsOnWithoutNotify(false);
                    menuItem.content.SetActive(false);
                }
            }

            contentScrollRect.verticalNormalizedPosition = 1f;
        }
    }
}

