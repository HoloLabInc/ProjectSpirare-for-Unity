using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloLab.Spirare.Cesium.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class CesiumCreditCanvasScaler : MonoBehaviour
    {
        [SerializeField]
        private float startScalingInch = 10f;

        private CanvasScaler canvasScaler;

        private void Awake()
        {
            canvasScaler = GetComponent<CanvasScaler>();
        }

        private void Update()
        {
            var dpi = Screen.dpi;
            if (dpi > 0)
            {
                var screenWidthInches = Screen.width / dpi;
                var scale = Mathf.Clamp01(screenWidthInches / startScalingInch);
                canvasScaler.scaleFactor = scale;
            }
        }
    }
}

