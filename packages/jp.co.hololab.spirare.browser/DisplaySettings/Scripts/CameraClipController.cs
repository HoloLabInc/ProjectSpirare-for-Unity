using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class CameraClipController : MonoBehaviour
    {
        [SerializeField]
        private DisplaySettingsState displaySettingsState;

        [SerializeField]
        private Camera[] cameraList = null;

        private void Start()
        {
            if (cameraList == null || cameraList.Length == 0)
            {
                cameraList = new Camera[] { Camera.main };
            }

            SetFarClip(displaySettingsState.FarClip);
            displaySettingsState.OnFarClipChanged += DisplaySettingsState_OnFarClipChanged;
        }

        private void OnDestroy()
        {
            displaySettingsState.OnFarClipChanged -= DisplaySettingsState_OnFarClipChanged;
        }

        private void DisplaySettingsState_OnFarClipChanged(float farClip)
        {
            SetFarClip(farClip);
        }

        private void SetFarClip(float farClip)
        {
            foreach (var camera in cameraList)
            {
                if (camera != null)
                {
                    camera.farClipPlane = farClip;
                }
            }
        }
    }
}

