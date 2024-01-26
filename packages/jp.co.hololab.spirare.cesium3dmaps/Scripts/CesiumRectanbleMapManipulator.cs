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

    private List<PointerData> pointerDataList = new List<PointerData>();

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
        return Vector3.Scale(localPoint, transform.transform.localScale);
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        var pointer = eventData.Pointer;
        if (TryGetPointerData(pointer.PointerId, out var pointerData) == false)
        {
            return;
        }

        //var pointerLocalPosition = GetLocalPosition(pointer.Position);
        //Debug.Log(pointer.Result.CurrentPointerTarget);
        //Debug.Log(pointer.Result.Details.Object);
        Debug.Log(pointer.Result.Details.LastRaycastHit.transform == transform);

        Vector3 pointerLocalPosition;

        if (pointer.Result.Details.LastRaycastHit.transform == transform)
        {
            pointerLocalPosition = GetLocalPosition(pointer.Result.Details.Point);
        }
        else
        {
            var startPoint = GetLocalPosition(pointer.Result.StartPoint);
            var pointerLocalPosition2 = GetLocalPosition(pointer.Result.Details.Point);

            var t = Mathf.InverseLerp(startPoint.z, pointerLocalPosition2.z, 0);
            pointerLocalPosition = Vector3.Lerp(startPoint, pointerLocalPosition2, t);
        }


        var deltaPosition = pointerLocalPosition - pointerData.PreviousPosition;

        pointerData.PreviousPosition = pointerLocalPosition;


        var panDelta = deltaPosition;
        var mapScale = cesiumRectangleMap.Scale;

        var east = -panDelta.x / mapScale;
        var north = -panDelta.y / mapScale;
        var newCenterEnu = new EnuPosition(east, north, 0);
        if (cesiumRectangleMap.TryConvertEnuPositionToGeodeticPosition(newCenterEnu, out var newCenterPosition) == false)
        {
            return;
        }

        cesiumRectangleMap.Center = newCenterPosition;

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
    }
}
