using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace HoloLab.Spirare.PolySpatial.Browser
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        public virtual void OnTouchBegan(SpatialPointerState touchData) { }
        public virtual void OnTouchMoved(SpatialPointerState touchData) { }
        public virtual void OnTouchEnded(SpatialPointerState touchData) { }
    }
}
