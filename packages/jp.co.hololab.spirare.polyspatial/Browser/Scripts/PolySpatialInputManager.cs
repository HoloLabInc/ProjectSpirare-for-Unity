using UnityEngine;
using Unity.PolySpatial.InputDevices;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;

namespace HoloLab.Spirare.PolySpatial.Browser
{
    public class PolySpatialInputManager : MonoBehaviour
    {
        private Dictionary<int, IInteractable> targetObjects = new Dictionary<int, IInteractable>();

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            var activeTouches = Touch.activeTouches;

            foreach (var touch in activeTouches)
            {
                var primaryTouchData = EnhancedSpatialPointerSupport.GetPointerState(touch);

                var interactionId = primaryTouchData.interactionId;
                if (targetObjects.TryGetValue(interactionId, out var interactable))
                {
                    if (interactable == null)
                    {
                        targetObjects.Remove(interactionId);
                        return;
                    }
                }
                else
                {
                    var targetObject = primaryTouchData.targetObject;
                    if (targetObject == null)
                    {
                        return;
                    }

                    interactable = targetObject.GetComponentInParent<IInteractable>();
                    if (interactable == null)
                    {
                        return;
                    }

                    targetObjects.Add(interactionId, interactable);
                }

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        interactable.OnTouchBegan(primaryTouchData);
                        break;
                    case TouchPhase.Moved:
                        interactable.OnTouchMoved(primaryTouchData);
                        break;
                    case TouchPhase.None:
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        interactable.OnTouchEnded(primaryTouchData);
                        targetObjects.Remove(interactionId);
                        break;
                }
            }
        }
    }
}
