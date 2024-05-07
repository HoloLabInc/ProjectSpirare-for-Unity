using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class FpsText : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text text = null;

        private float deltaTime = 0.0f;

        private float textUpdateIntervalMilliseconds = 250;
        private float lastUpdateTime = 0;

        private void Update()
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

            if (Time.realtimeSinceStartup - lastUpdateTime > textUpdateIntervalMilliseconds / 1000)
            {
                lastUpdateTime = Time.realtimeSinceStartup;

                var fps = Mathf.Round(1.0f / deltaTime);
                text.text = $"FPS: {fps}";
            }
        }
    }
}

