#if MRTK2_PRESENT
using HoloLab.PositioningTools.GeographicCoordinate;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#endif

using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
#if MRTK2_PRESENT
    public class CesiumRectanbleMapManipulator : MonoBehaviour, IMixedRealityPointerHandler
    {
        private class PointerData
        {
            public Vector3 PreviousPosition;
            public IMixedRealityPointer Pointer;
            public readonly bool IsTargetPositionLockedOnFocusLock;

            public PointerData(IMixedRealityPointer pointer, Vector3 previousPosition)
            {
                Pointer = pointer;
                PreviousPosition = previousPosition;
                IsTargetPositionLockedOnFocusLock = pointer.IsTargetPositionLockedOnFocusLock;
            }
        }

        private CesiumRectangleMap cesiumRectangleMap;

        private readonly List<PointerData> pointerDataList = new List<PointerData>();

        private int PointerCount => pointerDataList.Count;

        public event Action OnChangeScaleStarted;
        public event Action OnChangeScaleEnded;

        private void Start()
        {
            cesiumRectangleMap = GetComponentInParent<CesiumRectangleMap>();

            var currentMapSize = (cesiumRectangleMap.MapSizeX, cesiumRectangleMap.MapSizeZ);
            CesiumRectangleMap_OnMapSizeChanged(currentMapSize);

            cesiumRectangleMap.OnMapSizeChanged += CesiumRectangleMap_OnMapSizeChanged;
        }

        private void CesiumRectangleMap_OnMapSizeChanged((float MapSizeX, float MapSizeZ) mapSize)
        {
            var localScale = transform.localScale;
            localScale.x = mapSize.MapSizeX;
            localScale.y = mapSize.MapSizeZ;
            transform.localScale = localScale;
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (TryGetPointerData(eventData.Pointer.PointerId, out _))
            {
                return;
            }

            var pointer = eventData.Pointer;
            var pointerLocalPosition = GetLocalPosition(pointer.Result.Details.Point);
            var pointerData = new PointerData(pointer, pointerLocalPosition);
            pointer.IsTargetPositionLockedOnFocusLock = false;

            pointerDataList.Add(pointerData);

            eventData.Use();

            if (PointerCount == 2)
            {
                OnChangeScaleStarted?.Invoke();
            }
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            var pointer = eventData.Pointer;
            if (TryGetPointerData(pointer.PointerId, out var pointerData) == false)
            {
                return;
            }

            var pointerLocalPosition = GetPointerLocalPosition(pointer);
            var deltaPosition = pointerLocalPosition - pointerData.PreviousPosition;

            ChangeMapCenter(deltaPosition);

            if (PointerCount >= 2)
            {
                ChangeMapScale(pointerData, pointerLocalPosition);
            }

            pointerData.PreviousPosition = pointerLocalPosition;
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            var pointer = eventData.Pointer;

            if (TryGetPointerData(pointer.PointerId, out var pointerData) == false)
            {
                return;
            }

            pointer.IsTargetPositionLockedOnFocusLock = pointerData.IsTargetPositionLockedOnFocusLock;
            pointerDataList.Remove(pointerData);

            eventData.Use();

            if (PointerCount == 1)
            {
                OnChangeScaleEnded?.Invoke();
            }
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            // Do nothing
        }

        private Vector3 GetPointerLocalPosition(IMixedRealityPointer pointer)
        {
            var pointerLocalPosition = GetLocalPosition(pointer.Result.Details.Point);

            if (pointer.Result.Details.LastRaycastHit.transform == transform)
            {
                return pointerLocalPosition;
            }
            else
            {
                var startPoint = GetLocalPosition(pointer.Result.StartPoint);

                var t = Mathf.InverseLerp(startPoint.z, pointerLocalPosition.z, 0);
                var pointerLocalPositionIntersectingOperatingPlane = Vector3.Lerp(startPoint, pointerLocalPosition, t);
                return pointerLocalPositionIntersectingOperatingPlane;
            }
        }

        private Vector3 GetLocalPosition(Vector3 worldPosition)
        {
            var localPoint = transform.InverseTransformPoint(worldPosition);
            return localPoint;
        }

        private bool TryGetPointerData(uint pointerId, out PointerData pointerData)
        {
            pointerData = pointerDataList.FirstOrDefault(x => x.Pointer.PointerId == pointerId);
            return pointerData != null;
        }

        private void ChangeMapCenter(Vector3 deltaPosition)
        {
            var panDelta = deltaPosition / PointerCount;

            var newCenterEnu = ConvertLocalPositionToEnuPosition(new Vector3(-panDelta.x, -panDelta.y, 0));
            var newCenterPosition = cesiumRectangleMap.ConvertEnuPositionToGeodeticPosition(newCenterEnu);

            var currentCenter = cesiumRectangleMap.Center;
            cesiumRectangleMap.Center = new GeodeticPosition(newCenterPosition.Latitude, newCenterPosition.Longitude, currentCenter.EllipsoidalHeight);
        }

        private void ChangeMapScale(PointerData pointerData, Vector3 pointerLocalPosition)
        {
            var otherPointersSum = pointerDataList
                .Where(x => x != pointerData)
                .Aggregate(Vector3.zero, (s, x) => s + x.PreviousPosition);

            var previousCenter = (otherPointersSum + pointerData.PreviousPosition) / PointerCount;
            var currentCenter = (otherPointersSum + pointerLocalPosition) / PointerCount;

            // To prevent sudden changes in scale, use threshold for pointer distance.
            var distanceThreshold = 0.01f;
            var previousDistance = Mathf.Max((previousCenter - pointerData.PreviousPosition).magnitude, distanceThreshold);
            var currentDistance = Mathf.Max((currentCenter - pointerLocalPosition).magnitude, distanceThreshold);

            var mapScale = cesiumRectangleMap.Scale / previousDistance * currentDistance;
            var scaleCenter = (previousCenter + currentCenter) / 2;

            var scaleCenterEnu = ConvertLocalPositionToEnuPosition(scaleCenter);
            cesiumRectangleMap.ScaleAroundEnuPosition(mapScale, scaleCenterEnu);
        }

        private EnuPosition ConvertLocalPositionToEnuPosition(Vector3 localPosition)
        {
            var mapScale = cesiumRectangleMap.Scale;

            var east = localPosition.x * cesiumRectangleMap.MapSizeX / mapScale;
            var north = localPosition.y * cesiumRectangleMap.MapSizeZ / mapScale;
            var up = localPosition.z / mapScale;

            return new EnuPosition(east, north, up);
        }
    }
#else
    public class CesiumRectanbleMapManipulator : MonoBehaviour { }
#endif
}

