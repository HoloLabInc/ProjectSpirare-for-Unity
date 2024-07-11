using System.Collections;
using System.Collections.Generic;
using Unity.PolySpatial.InputDevices;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace HoloLab.Spirare.PolySpatial.Browser
{
    public class InteractionManagerPolySpatial : MonoBehaviour
    {
        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            var activeTouches = Touch.activeTouches;

            if (activeTouches.Count > 0)
            {
                var primaryTouchData = EnhancedSpatialPointerSupport.GetPointerState(activeTouches[0]);
                if (activeTouches[0].phase == TouchPhase.Began)
                {
                    var targetObject = primaryTouchData.targetObject;
                    if (targetObject != null)
                    {
                        SelectPomlElementComponent(targetObject);
                    }
                }
            }
        }

        private void SelectPomlElementComponent(GameObject target)
        {
            var hitPomlElement = target.GetComponentInParent<PomlObjectElementComponent>();
            if (hitPomlElement != null)
            {
                hitPomlElement.Select();
            }
        }
    }
}
