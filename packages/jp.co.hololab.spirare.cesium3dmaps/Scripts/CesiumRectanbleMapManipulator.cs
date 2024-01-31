using HoloLab.PositioningTools.GeographicCoordinate;
using HoloLab.Spirare.Cesium3DMaps;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CesiumRectanbleMapManipulator : MonoBehaviour, IMixedRealityPointerHandler
{
    private CesiumRectangleMap cesiumRectangleMap;

    private readonly List<PointerData> pointerDataList = new List<PointerData>();

    private int PointerCount => pointerDataList.Count;

    // private bool OnePointerManipulation => PointerCount == 1;

    // private bool TwoPointersManipulation => PointerCount > 2;

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

    private bool TryGetPointerData(uint pointerId, out PointerData pointerData)
    {
        pointerData = pointerDataList.FirstOrDefault(x => x.Pointer.PointerId == pointerId);
        return pointerData != null;
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
    }

    private Vector3 GetLocalPosition(Vector3 worldPosition)
    {
        var localPoint = transform.InverseTransformPoint(worldPosition);
        return localPoint;
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

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        var pointer = eventData.Pointer;
        if (TryGetPointerData(pointer.PointerId, out var pointerData) == false)
        {
            return;
        }

        var pointerLocalPosition = GetPointerLocalPosition(pointer);
        var deltaPosition = pointerLocalPosition - pointerData.PreviousPosition;

        MoveCenter(deltaPosition);
        if (PointerCount >= 2)
        {
            ChangeScale(pointerData, pointerLocalPosition);
        }

        pointerData.PreviousPosition = pointerLocalPosition;
    }

    private void ChangeScale(PointerData pointerData, Vector3 pointerLocalPosition)
    {
        var otherPointersSum = pointerDataList
            .Where(x => x != pointerData)
            .Aggregate(Vector3.zero, (s, x) => s + x.PreviousPosition);

        var previousCenter = (otherPointersSum + pointerData.PreviousPosition) / PointerCount;
        var currentCenter = (otherPointersSum + pointerLocalPosition) / PointerCount;

        var previousDistance = (previousCenter - pointerData.PreviousPosition).magnitude;
        var currentDistance = (currentCenter - pointerLocalPosition).magnitude;

        var mapScale = cesiumRectangleMap.Scale / previousDistance * currentDistance;
        var scaleCenter = (previousCenter + currentCenter) / 2;

        cesiumRectangleMap.Scale = mapScale;
    }

    private void MoveCenter(Vector3 deltaPosition)
    {
        var panDelta = deltaPosition / PointerCount;
        var mapScale = cesiumRectangleMap.Scale;

        var east = -panDelta.x * cesiumRectangleMap.MapSizeX / mapScale;
        var north = -panDelta.y * cesiumRectangleMap.MapSizeZ / mapScale;
        var newCenterEnu = new EnuPosition(east, north, 0);
        if (cesiumRectangleMap.TryConvertEnuPositionToGeodeticPosition(newCenterEnu, out var newCenterPosition) == false)
        {
            return;
        }

        var currentCenter = cesiumRectangleMap.Center;
        cesiumRectangleMap.Center = new GeodeticPosition(newCenterPosition.Latitude, newCenterPosition.Longitude, currentCenter.EllipsoidalHeight);
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
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        // Do nothing
    }
}
