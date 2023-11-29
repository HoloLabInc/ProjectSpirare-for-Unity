using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class CesiumRectangleMapTilesetClipper : MonoBehaviour
    {
        private CesiumRectangleMap rectangleMap;
        private CesiumTilesetRectangleClipper rectangleClipper;

        private void Start()
        {
            rectangleMap = GetComponentInParent<CesiumRectangleMap>();
            if (rectangleMap == null)
            {
                return;
            }

            if (TryGetComponent(out rectangleClipper) == false)
            {
                rectangleClipper = gameObject.AddComponent<CesiumTilesetRectangleClipper>();
            }

            rectangleClipper.ClippingOriginTransform = rectangleMap.transform;
            rectangleClipper.ClippingSizeX = rectangleMap.MapSizeX;
            rectangleClipper.ClippingSizeZ = rectangleMap.MapSizeZ;

            rectangleMap.OnMapSizeChanged += OnMapSizeChanged;
        }

        private void OnDestroy()
        {
            if (rectangleMap != null)
            {
                rectangleMap.OnMapSizeChanged -= OnMapSizeChanged;
            }
        }

        private void OnMapSizeChanged((float MapSizeX, float MapSizeZ) mapSize)
        {
            rectangleClipper.ClippingSizeX = mapSize.MapSizeX;
            rectangleClipper.ClippingSizeZ = mapSize.MapSizeZ;
        }
    }
}
