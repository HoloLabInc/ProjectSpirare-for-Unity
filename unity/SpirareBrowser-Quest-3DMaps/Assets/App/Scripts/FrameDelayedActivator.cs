using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.Quest3DMaps
{
    public class FrameDelayedActivator : MonoBehaviour
    {
        [SerializeField]
        private int waitFrames = 1;

        [SerializeField]
        private GameObject targetObject;

        private void Awake()
        {
            StartCoroutine(InitializeSceneContentWithDelay());
        }

        private IEnumerator InitializeSceneContentWithDelay()
        {
            for (var i = 0; i < waitFrames; i++)
            {
                yield return null;
            }

            targetObject.SetActive(true);
        }
    }
}

