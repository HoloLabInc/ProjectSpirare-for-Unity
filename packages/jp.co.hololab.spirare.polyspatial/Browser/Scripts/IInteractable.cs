using UnityEngine.InputSystem.LowLevel;

namespace HoloLab.Spirare.PolySpatial.Browser
{
    public interface IInteractable
    {
        void OnTouchBegan(SpatialPointerState touchData);
        void OnTouchMoved(SpatialPointerState touchData);
        void OnTouchEnded(SpatialPointerState touchData);
    }
}
