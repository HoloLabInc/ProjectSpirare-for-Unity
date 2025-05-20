using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Quest
{
    public class MenuPanel : MonoBehaviour
    {
        [SerializeField]
        private float distanceFromCamera = 1f;

        private Canvas menuCanvas;

        private void Start()
        {
            menuCanvas = GetComponentInChildren<Canvas>();
            menuCanvas.gameObject.SetActive(false);
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
    }
}

