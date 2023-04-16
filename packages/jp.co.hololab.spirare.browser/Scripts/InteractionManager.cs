using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class InteractionManager : MonoBehaviour
    {
        private Camera mainCamera;

        [SerializeField]
        private Camera screenSpaceCamera;

        private RaycastHit[] raycastHits = new RaycastHit[1];

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            var selectedPoint = GetSelectedScreenPoint();
            if (selectedPoint.HasValue == false)
            {
                return;
            }

            var hit = RaycastInScreenSpace(screenSpaceCamera, selectedPoint.Value, raycastHits);

            if (hit == false)
            {
                hit = RaycastInWorldSpace(mainCamera, selectedPoint.Value, raycastHits);
            }

            if (hit)
            {
                var firstHit = raycastHits[0];
                var hitObject = firstHit.collider.gameObject;
                var hitPomlelement = hitObject.GetComponentInParent<PomlObjectElementComponent>();
                if (hitPomlelement != null)
                {
                    hitPomlelement.Select();
                }
            }
        }

        private Vector2? GetSelectedScreenPoint()
        {
            var touchPoint = GetTouchPoint();
            if (touchPoint.HasValue)
            {
                return touchPoint;
            }

            return GetMouseClickPoint();
        }

        private Vector2? GetTouchPoint()
        {
            if (Input.touchCount != 1)
            {
                return null;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended)
            {
                return null;
            }

            return touch.position;
        }

        private Vector2? GetMouseClickPoint()
        {
            if (Input.GetMouseButtonUp(0))
            {
                return Input.mousePosition;
            }
            return null;
        }

        private static bool RaycastInScreenSpace(Camera screenSpaceCamera, Vector2 screenPoint, RaycastHit[] raycastHits)
        {
            if (screenSpaceCamera == null)
            {
                return false;
            }

            var ray = screenSpaceCamera.ScreenPointToRay(screenPoint);
            var hitCount = Physics.RaycastNonAlloc(ray, raycastHits, screenSpaceCamera.farClipPlane, screenSpaceCamera.cullingMask);

            return hitCount > 0;
        }

        private static bool RaycastInWorldSpace(Camera camera, Vector2 screenPoint, RaycastHit[] raycastHits)
        {
            var ray = camera.ScreenPointToRay(screenPoint);
            var hitCount = Physics.RaycastNonAlloc(ray, raycastHits, float.MaxValue, camera.cullingMask);

            return hitCount > 0;
        }
    }
}
