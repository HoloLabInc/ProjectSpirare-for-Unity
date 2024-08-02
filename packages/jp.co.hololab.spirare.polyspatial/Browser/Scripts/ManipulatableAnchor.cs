using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace HoloLab.Spirare.PolySpatial.Browser
{
    public class ManipulatableAnchor : MonoBehaviour
    {
        private ARAnchor anchor;
        private Transform targetTransform;

        private void Start()
        {
            var objectManipulator = GetComponent<ObjectManipulator>();
            targetTransform = objectManipulator.TargetTransform;
            objectManipulator.OnManipulationStarted += OnManipulationStarted;
            objectManipulator.OnManipulationEnded += OnManipulationEnded;
        }

        private void OnManipulationStarted()
        {
            if (anchor != null)
            {
                Destroy(anchor);
                anchor = null;
            }
        }

        private void OnManipulationEnded()
        {
            if (anchor == null)
            {
                anchor = targetTransform.gameObject.AddComponent<ARAnchor>();
            }
        }
    }
}
