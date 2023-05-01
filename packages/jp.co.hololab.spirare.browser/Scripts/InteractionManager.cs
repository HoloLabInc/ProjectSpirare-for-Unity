using System;
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

        private RaycastHit[] raycastHits = new RaycastHit[100];

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

            var result = SelectScreenSpaceObject(selectedPoint.Value);

            if (result == false)
            {
                SelectWorldSpaceObject(selectedPoint.Value);
            }
        }

        private bool SelectScreenSpaceObject(Vector2 selectedPoint)
        {
            var hitCount = RaycastInScreenSpace(screenSpaceCamera, selectedPoint, raycastHits);

            if (hitCount == 0)
            {
                return false;
            }

            SelectNearestHitObject(raycastHits, hitCount, screenSpaceCamera.transform.position);
            return true;
        }

        private bool SelectWorldSpaceObject(Vector2 selectedPoint)
        {
            var hitCount = RaycastInWorldSpace(mainCamera, selectedPoint, raycastHits);

            if (hitCount == 0)
            {
                return false;
            }

            SelectNearestHitObject(raycastHits, hitCount, mainCamera.transform.position);
            return true;
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

        private static void SelectNearestHitObject(RaycastHit[] raycastHits, int hitCount, Vector3 origin)
        {
            if (hitCount == 0)
            {
                return;
            }

            float minDistance = Mathf.Infinity;
            RaycastHit nearestHit = raycastHits[0];

            for (int i = 0; i < hitCount; i++)
            {
                float sqrDistance = (origin - raycastHits[i].point).sqrMagnitude;
                if (sqrDistance < minDistance)
                {
                    minDistance = sqrDistance;
                    nearestHit = raycastHits[i];
                }
            }

            var hitObject = nearestHit.collider.gameObject;
            var hitPomlelement = hitObject.GetComponentInParent<PomlObjectElementComponent>();
            if (hitPomlelement != null)
            {
                hitPomlelement.Select();
            }
        }

        private static int RaycastInScreenSpace(Camera screenSpaceCamera, Vector2 screenPoint, RaycastHit[] raycastHits)
        {
            if (screenSpaceCamera == null)
            {
                return 0;
            }

            var ray = screenSpaceCamera.ScreenPointToRay(screenPoint);
            var hitCount = Physics.RaycastNonAlloc(ray, raycastHits, screenSpaceCamera.farClipPlane, screenSpaceCamera.cullingMask);

            return hitCount;
        }

        private static int RaycastInWorldSpace(Camera camera, Vector2 screenPoint, RaycastHit[] raycastHits)
        {
            var ray = camera.ScreenPointToRay(screenPoint);
            var hitCount = Physics.RaycastNonAlloc(ray, raycastHits, float.MaxValue, camera.cullingMask);

            return hitCount;
        }
    }
}
