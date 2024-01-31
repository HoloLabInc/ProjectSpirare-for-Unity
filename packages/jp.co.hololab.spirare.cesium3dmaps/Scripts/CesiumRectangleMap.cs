using CesiumForUnity;
using HoloLab.PositioningTools.CoordinateSystem;
using HoloLab.PositioningTools.GeographicCoordinate;
using HoloLab.Spirare.Cesium;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class CesiumRectangleMap : MonoBehaviour
    {
        [SerializeField]
        private float mapSizeX = 1;

        public float MapSizeX
        {
            get
            {
                return mapSizeX;
            }
            set
            {
                mapSizeX = value;
                UpdateMap();
                InvokeOnMapSizeChanged(mapSizeX, mapSizeZ);
            }
        }

        [SerializeField]
        private float mapSizeZ = 1;

        public float MapSizeZ
        {
            get
            {
                return mapSizeZ;
            }
            set
            {
                mapSizeZ = value;
                UpdateMap();
                InvokeOnMapSizeChanged(mapSizeX, mapSizeZ);
            }
        }

        [SerializeField]
        private float scale = 1 / 1000f;

        public float Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = Mathf.Clamp(value, scaleMin, scaleMax);
                SaveScale();
                UpdateMap();
                InvokeOnScaleChanged(scale);
            }
        }

        [SerializeField]
        private float scaleMin = 1 / 1_000_000f;

        [SerializeField]
        private float scaleMax = 1;

        [SerializeField]
        private GeodeticPositionForInspector center;

        public GeodeticPosition Center
        {
            get
            {
                return center.ToGeodeticPosition();
            }
            set
            {
                center = new GeodeticPositionForInspector(value);
                SaveCenterPosition();
                UpdateMap();
                InvokeOnCenterChanged(value);
            }
        }

        [SerializeField]
        private bool attatchTilesetClipperForChildTilesets = true;

        [SerializeField]
        private CesiumRectangleMapBase mapBase;

        private CesiumGeoreference[] cesiumGeoreferences;
        private CesiumGeodeticAreaExcluder[] cesiumGeodeticAreaExcluders;

        private const string PlayerPrefs_CenterKey = "CesiumRectangleMap_Center";
        private const string PlayerPrefs_ScaleKey = "CesiumRectangleMap_Scale";

        public Action<float> OnScaleChanged;
        public Action<GeodeticPosition> OnCenterChanged;
        public Action<(float MapSizeX, float MapSizeZ)> OnMapSizeChanged;

        private void Start()
        {
            cesiumGeoreferences = GetComponentsInChildren<CesiumGeoreference>();
            cesiumGeodeticAreaExcluders = GetComponentsInChildren<CesiumGeodeticAreaExcluder>();

            if (attatchTilesetClipperForChildTilesets)
            {
                AttachTilesetClipperForChildTilesets();
            }

            LoadCenterPosition();
            LoadScale();

            UpdateMap();
        }

        public GeodeticPosition ConvertEnuPositionToGeodeticPosition(EnuPosition enuPosition)
        {
            return EnuToGeodetic(enuPosition);
        }

        public void ScaleAroundEnuPosition(float mapScale, EnuPosition scaleCenter)
        {
            var scaleCenterGeodetic = EnuToGeodetic(scaleCenter, Center);
            var scaleCenterToCurrentMapCenter = GeodeticToEnu(Center, scaleCenterGeodetic);

            var relativeScale = (double)Scale / mapScale;
            var scaleCenterToNewMapCenter = new EnuPosition(
                scaleCenterToCurrentMapCenter.East * relativeScale,
                scaleCenterToCurrentMapCenter.North * relativeScale,
                scaleCenterToCurrentMapCenter.Up * relativeScale);

            var newMapCenter = EnuToGeodetic(scaleCenterToNewMapCenter, scaleCenterGeodetic);
            var newMapCenterWithSameHeight = new GeodeticPosition(newMapCenter.Latitude, newMapCenter.Longitude, Center.EllipsoidalHeight);

            Center = newMapCenterWithSameHeight;
            Scale = mapScale;
        }

        private void AttachTilesetClipperForChildTilesets()
        {
            var tilesets = GetComponentsInChildren<Cesium3DTileset>();
            foreach (var tileset in tilesets)
            {
                if (tileset.gameObject.TryGetComponent<CesiumRectangleMapTilesetClipper>(out _) == false)
                {
                    tileset.gameObject.AddComponent<CesiumRectangleMapTilesetClipper>();
                }
            }
        }

        private void SaveCenterPosition()
        {
            var centerString = $"{Center.Latitude} {Center.Longitude} {Center.EllipsoidalHeight}";
            PlayerPrefs.SetString(PlayerPrefs_CenterKey, centerString);
            PlayerPrefs.Save();
        }

        private void LoadCenterPosition()
        {
            var centerString = PlayerPrefs.GetString(PlayerPrefs_CenterKey);
            if (string.IsNullOrEmpty(centerString))
            {
                return;
            }
            var centerArray = centerString.Split(' ');
            if (centerArray.Length != 3)
            {
                return;
            }
            var latitude = double.Parse(centerArray[0]);
            var longitude = double.Parse(centerArray[1]);
            var ellipsoidalHeight = double.Parse(centerArray[2]);
            Center = new GeodeticPosition(latitude, longitude, ellipsoidalHeight);
        }

        private void SaveScale()
        {
            PlayerPrefs.SetFloat(PlayerPrefs_ScaleKey, Scale);
            PlayerPrefs.Save();
        }

        private void LoadScale()
        {
            var scale = PlayerPrefs.GetFloat(PlayerPrefs_ScaleKey);
            if (scale > 0)
            {
                Scale = scale;
            }
        }

        private void UpdateMap()
        {
            UpdateMapBase();
            UpdateCesiumGeoreferences();
            UpdateCesiumGeodeticAreaExcluders();
        }

        private void UpdateMapBase()
        {
            mapBase.ChangeSize(mapSizeX, mapSizeZ);
        }

        private void UpdateCesiumGeoreferences()
        {
            foreach (var georeference in cesiumGeoreferences)
            {
                georeference.SetOriginLongitudeLatitudeHeight(center.Longitude, center.Latitude, center.EllipsoidalHeight);
                georeference.transform.localScale = scale * Vector3.one;
            }
        }

        private void UpdateCesiumGeodeticAreaExcluders()
        {
            var upperLeftLonLatHeight = EnuToGeodetic(new EnuPosition(-mapSizeX / 2 / scale, mapSizeZ / 2 / scale, 0));
            var lowerRightLonLatHeight = EnuToGeodetic(new EnuPosition(mapSizeX / 2 / scale, -mapSizeZ / 2 / scale, 0));

            foreach (var cesiumGeodeticAreaExcluder in cesiumGeodeticAreaExcluders)
            {
                cesiumGeodeticAreaExcluder.UpperLeftLongitude = upperLeftLonLatHeight.Longitude;
                cesiumGeodeticAreaExcluder.UpperLeftLatitude = upperLeftLonLatHeight.Latitude;

                cesiumGeodeticAreaExcluder.LowerRightLongitude = lowerRightLonLatHeight.Longitude;
                cesiumGeodeticAreaExcluder.LowerRightLatitude = lowerRightLonLatHeight.Latitude;
            }
        }

        private void InvokeOnScaleChanged(float scale)
        {
            try
            {
                OnScaleChanged?.Invoke(scale);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void InvokeOnCenterChanged(GeodeticPosition center)
        {
            try
            {
                OnCenterChanged?.Invoke(center);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void InvokeOnMapSizeChanged(float mapSizeX, float mapSizeZ)
        {
            try
            {
                OnMapSizeChanged?.Invoke((mapSizeX, mapSizeZ));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private GeodeticPosition EnuToGeodetic(EnuPosition enuPosition)
        {
            return EnuToGeodetic(enuPosition, Center);
        }

        private static GeodeticPosition EnuToGeodetic(EnuPosition enuPosition, GeodeticPosition originPosition)
        {
            return GeographicCoordinateConversion.EnuToGeodetic(enuPosition, originPosition);
        }

        private static EnuPosition GeodeticToEnu(GeodeticPosition geodeticPosition, GeodeticPosition originPosition)
        {
            return GeographicCoordinateConversion.GeodeticToEnu(geodeticPosition, originPosition);
        }
    }
}
