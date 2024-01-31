using CesiumForUnity;
using HoloLab.PositioningTools.CoordinateSystem;
using HoloLab.PositioningTools.GeographicCoordinate;
using HoloLab.Spirare.Cesium;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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
                scale = Mathf.Max(value, MinimumScale);
                SaveScale();
                UpdateMap();
                InvokeOnScaleChanged(scale);
            }
        }

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

        private const float MinimumScale = 0.000001f;

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

        public bool TryConvertEnuPositionToGeodeticPosition(EnuPosition enuPosition, out GeodeticPosition geodeticPosition)
        {
            var georeference = cesiumGeoreferences.FirstOrDefault();
            if (georeference == null)
            {
                geodeticPosition = default;
                return false;
            }

            var geodeticDouble3 = EnuToGeodetic(georeference, new double3(enuPosition.East, enuPosition.Up, enuPosition.North));
            geodeticPosition = new GeodeticPosition(geodeticDouble3.y, geodeticDouble3.x, geodeticDouble3.z);
            return true;
        }

        public void ScaleAroundEnuPosition(float mapScale, EnuPosition scaleCenter)
        {
            var scaleCenterGeodetic = GeographicCoordinateConversion.EnuToGeodetic(scaleCenter, Center);
            var scaleCenterToCurrentMapCenter = GeographicCoordinateConversion.GeodeticToEnu(Center, scaleCenterGeodetic);

            var relativeScale = (double)Scale / mapScale;
            var scaleCenterToNewMapCenter = new EnuPosition(
                scaleCenterToCurrentMapCenter.East * relativeScale,
                scaleCenterToCurrentMapCenter.North * relativeScale,
                scaleCenterToCurrentMapCenter.Up * relativeScale);

            var newMapCenter = GeographicCoordinateConversion.EnuToGeodetic(scaleCenterToNewMapCenter, scaleCenterGeodetic);
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
            var georeference = cesiumGeoreferences.FirstOrDefault();
            if (georeference == null)
            {
                return;
            }

            var upperLeftLonLatHeight = EnuToGeodetic(georeference, new double3(-mapSizeX / 2 / scale, 0, mapSizeZ / 2 / scale));
            var lowerRightLonLatHeight = EnuToGeodetic(georeference, new double3(mapSizeX / 2 / scale, 0, -mapSizeZ / 2 / scale));

            foreach (var cesiumGeodeticAreaExcluder in cesiumGeodeticAreaExcluders)
            {
                cesiumGeodeticAreaExcluder.UpperLeftLongitude = upperLeftLonLatHeight.x;
                cesiumGeodeticAreaExcluder.UpperLeftLatitude = upperLeftLonLatHeight.y;

                cesiumGeodeticAreaExcluder.LowerRightLongitude = lowerRightLonLatHeight.x;
                cesiumGeodeticAreaExcluder.LowerRightLatitude = lowerRightLonLatHeight.y;
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

        private static double3 EnuToGeodetic(CesiumGeoreference georeference, double3 enuPosition)
        {
            var ecef = georeference.TransformUnityPositionToEarthCenteredEarthFixed(enuPosition);
            var lonLatHeight = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);
            return lonLatHeight;
        }
    }
}
