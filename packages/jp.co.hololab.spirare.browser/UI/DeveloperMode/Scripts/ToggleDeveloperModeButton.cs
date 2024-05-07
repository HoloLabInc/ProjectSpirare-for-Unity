using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HoloLab.Spirare.Browser.UI
{
    public class ToggleDeveloperModeButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        private DeveloperModeState developerModeState;

        private float? pointerDownTime;

        private float longTapDuration = 3.0f;

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownTime = Time.realtimeSinceStartup;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerDownTime.HasValue && Time.realtimeSinceStartup - pointerDownTime > longTapDuration)
            {
                ToggleDeveloperMode();
            }

            pointerDownTime = null;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerDownTime = null;
        }

        private void ToggleDeveloperMode()
        {
            developerModeState.Enabled = !developerModeState.Enabled;
        }
    }
}

