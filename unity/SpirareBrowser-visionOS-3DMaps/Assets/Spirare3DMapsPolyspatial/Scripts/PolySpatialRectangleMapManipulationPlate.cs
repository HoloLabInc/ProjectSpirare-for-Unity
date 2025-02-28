using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloLab.PositioningTools.GeographicCoordinate;
using HoloLab.Spirare.PolySpatial.Browser;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace HoloLab.Spirare.Cesium3DMaps.PolySpatial
{
    public class PolySpatialRectangleMapManipulationPlate : InteractableBase
    {
        private class PointerData
        {
            public int InteractionId;
            public Vector3 StartPosition;
            public Vector3 PreviousPosition;
            public Vector3 InputDevicePosition;
            public Vector3 PreviousInputDevicePosition;
        }

        private CesiumRectangleMap rectangleMap;

        private List<PointerData> pointerDataList = new List<PointerData>();

        private bool scaleChanging;

        private void Awake()
        {
            rectangleMap = GetComponentInParent<CesiumRectangleMap>();
        }

        public override void OnTouchBegan(SpatialPointerState touchData)
        {
            if (IsValidTouchKind(touchData) == false)
            {
                return;
            }

            if (pointerDataList.Count >= 2)
            {
                return;
            }

            if (pointerDataList.Exists(x => x.InteractionId == touchData.interactionId) == false)
            {
                var touchPosition = GetPositionInLocalSpace(touchData);
                pointerDataList.Add(new PointerData()
                {
                    InteractionId = touchData.interactionId,
                    StartPosition = touchPosition,
                    PreviousPosition = touchPosition,
                    InputDevicePosition = touchData.inputDevicePosition,
                    PreviousInputDevicePosition = touchData.inputDevicePosition,
                });

                if (pointerDataList.Count >= 2 && scaleChanging == false)
                {
                    scaleChanging = true;
                    rectangleMap.StartMapScaleChange();
                }
            }
        }

        public override void OnTouchMoved(SpatialPointerState touchData)
        {
            if (IsValidTouchKind(touchData) == false)
            {
                return;
            }

            if (TryGetPointerData(touchData.interactionId, out var pointerData) == false)
            {
                return;
            }

            pointerData.InputDevicePosition = touchData.inputDevicePosition;

            if (pointerDataList.Count == 1)
            {
                var delta = GetDeltaPositionInLocalSpace(touchData);
                ChangeMapCenter(delta);
            }
            else
            {
                ChangeMapScale(pointerData, touchData);
            }

            pointerData.PreviousPosition = GetPositionInLocalSpace(touchData);
            pointerData.PreviousInputDevicePosition = touchData.inputDevicePosition;
        }

        public override void OnTouchEnded(SpatialPointerState touchData)
        {
            pointerDataList.RemoveAll(x => x.InteractionId == touchData.interactionId);

            if (pointerDataList.Count <= 1 && scaleChanging)
            {
                scaleChanging = false;
                rectangleMap.EndMapScaleChange();
            }
        }

        private void ChangeMapCenter(Vector3 delta)
        {
            var newCenterEnu = ConvertLocalPositionToEnuPosition(new Vector3(-delta.x, -delta.y, 0));
            var newCenter = rectangleMap.ConvertEnuPositionToGeodeticPosition(newCenterEnu);
            rectangleMap.Center = newCenter;
        }

        private void ChangeMapScale(PointerData pointerData, SpatialPointerState touchData)
        {
            var lastPointerData = pointerDataList.Last();
            var scalingCenter = lastPointerData.StartPosition;

            var inputDeviceCenter = pointerDataList.Aggregate(Vector3.zero, (s, x) => s + x.InputDevicePosition) / pointerDataList.Count;
            var centerToInputPosition = pointerData.InputDevicePosition - inputDeviceCenter;
            centerToInputPosition.y = 0;
            centerToInputPosition.Normalize();

            var dot = Vector3.Dot(touchData.inputDevicePosition - pointerData.PreviousInputDevicePosition, centerToInputPosition);

            var scaleFactor = 3f;
            float scaleDelta;
            if (dot > 0)
            {
                scaleDelta = 1 + dot * scaleFactor;
            }
            else
            {
                scaleDelta = 1 / (1 - dot * scaleFactor);
            }

            var scale = scaleDelta * rectangleMap.Scale;
            var scaleCenterEnu = ConvertLocalPositionToEnuPosition(scalingCenter);
            rectangleMap.ScaleAroundEnuPosition(scale, scaleCenterEnu);
        }

        private bool TryGetPointerData(int interactionId, out PointerData pointerData)
        {
            pointerData = pointerDataList.FirstOrDefault(x => x.InteractionId == interactionId);
            return pointerData != null;
        }

        private Vector3 GetDeltaPositionInLocalSpace(SpatialPointerState touchData)
        {
            return transform.InverseTransformVector(touchData.deltaInteractionPosition);
        }

        private Vector3 GetPositionInLocalSpace(SpatialPointerState touchData)
        {
            return transform.InverseTransformPoint(touchData.interactionPosition);
        }

        private EnuPosition ConvertLocalPositionToEnuPosition(Vector3 localPosition)
        {
            var mapScale = rectangleMap.Scale;

            var east = localPosition.x * rectangleMap.MapSizeX / mapScale;
            var north = localPosition.y * rectangleMap.MapSizeZ / mapScale;
            var up = localPosition.z / mapScale;

            return new EnuPosition(east, north, up);
        }

        private bool IsValidTouchKind(SpatialPointerState touchData)
        {
            switch (touchData.Kind)
            {
                case SpatialPointerKind.IndirectPinch:
                case SpatialPointerKind.DirectPinch:
                    return true;
                default:
                    return false;
            }
        }
    }
}
