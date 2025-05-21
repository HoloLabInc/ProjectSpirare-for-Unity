using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System;
using UnityEngine;

namespace HoloLab.Spirare.Quest
{
    public class SurfaceExpanderForDragging : MonoBehaviour
    {
        private BoundsClipper boundsCliper;
        private Vector3 boundsClipperDefaultSize;

        private int selectCount = 0;

        private static Vector3 infinitySize = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

        private void Start()
        {
            var rayInteractable = GetComponent<RayInteractable>();
            var surface = rayInteractable.Surface;
            boundsCliper = surface.Transform.GetComponent<BoundsClipper>();
            boundsClipperDefaultSize = boundsCliper.Size;

            rayInteractable.WhenPointerEventRaised += WhenPointerEventRaised;
        }

        private void WhenPointerEventRaised(PointerEvent pointerEvent)
        {
            switch (pointerEvent.Type)
            {
                case PointerEventType.Select:
                    selectCount += 1;
                    boundsCliper.Size = infinitySize;
                    break;
                case PointerEventType.Unselect:
                    if (selectCount > 0)
                    {
                        selectCount -= 1;
                    }
                    if (selectCount == 0)
                    {
                        boundsCliper.Size = boundsClipperDefaultSize;
                    }
                    break;
            }
        }
    }
}

