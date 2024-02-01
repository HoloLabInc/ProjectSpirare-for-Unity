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
                UpdateMapBase();
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
                UpdateMapBase();
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
        private float scaleMin = 1 / 2_000_000f;

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
        private bool autoAdjustCenterHeight = true;

        public bool AutoAdjustCenterHeight
        {
            get
            {
                return autoAdjustCenterHeight;
            }
            set
            {
                autoAdjustCenterHeight = value;
                if (autoAdjustCenterHeight == false)
                {
                    centerTargetEllipsoidalHeight = null;
                }
                SaveAutoAdjustCenterHeight();
                InvokeOnAutoAdjustCenterHeightChanged(value);
            }
        }

        private float? centerTargetEllipsoidalHeight = null;

        [SerializeField]
        private bool attatchTilesetClipperForChildTilesets = true;

        [SerializeField]
        private CesiumRectangleMapBase mapBase;

        private CesiumGeoreference[] cesiumGeoreferences;
        private CesiumGeodeticAreaExcluder[] cesiumGeodeticAreaExcluders;

        private RaycastHit[] hits = new RaycastHit[100];

        private const string PlayerPrefs_CenterKey = "CesiumRectangleMap_Center";
        private const string PlayerPrefs_ScaleKey = "CesiumRectangleMap_Scale";
        private const string PlayerPrefs_AutoAdjustCenterHeightKey = "CesiumRectangleMap_AutoAdjustCenterHeight";

        public event Action<float> OnScaleChanged;
        public event Action<GeodeticPosition> OnCenterChanged;
        public event Action<(float MapSizeX, float MapSizeZ)> OnMapSizeChanged;
        public event Action<bool> OnAutoAdjustCenterHeightChanged;

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
            LoadAutoAdjustCenterHeight();

            UpdateMap();
            UpdateMapBase();

            StartCoroutine(AdjustMapHeightLoopCoroutine());
        }

        private void Update()
        {
            if (centerTargetEllipsoidalHeight.HasValue)
            {
                float newHeight;

                if (Mathf.Abs((float)center.EllipsoidalHeight - centerTargetEllipsoidalHeight.Value) * scale < 0.001)
                {
                    newHeight = centerTargetEllipsoidalHeight.Value;
                    centerTargetEllipsoidalHeight = null;
                }
                else
                {
                    var lerpLate = 2f;
                    newHeight = Mathf.Lerp((float)center.EllipsoidalHeight, centerTargetEllipsoidalHeight.Value, lerpLate * Time.deltaTime);
                }

                var newCenter = new GeodeticPosition(center.Latitude, center.Longitude, newHeight);
                Center = newCenter;
            }
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

        private IEnumerator AdjustMapHeightLoopCoroutine()
        {
            while (true)
            {
                if (autoAdjustCenterHeight)
                {
                    AdjustMapHeight();
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private void AdjustMapHeight()
        {
            var mapCenterEcef = GeographicCoordinateConversion.GeodeticToEcef(Center);
            var distanceFromEarthCenter = Mathf.Sqrt((float)(mapCenterEcef.X * mapCenterEcef.X + mapCenterEcef.Y * mapCenterEcef.Y + mapCenterEcef.Z * mapCenterEcef.Z));

            var raycastCenterDepth = Mathf.Max(-distanceFromEarthCenter * scale, -50000);
            var raycastCenter = transform.TransformPoint(new Vector3(0, raycastCenterDepth, 0));
            var lossyScale = transform.lossyScale;
            var halfExtent = new Vector3(lossyScale.x * mapSizeX / 2, lossyScale.y, lossyScale.z * mapSizeZ / 2);
            var layerMask = LayerMask.GetMask("Ignore Raycast");

            var queriesHitBackfaces = Physics.queriesHitBackfaces;
            Physics.queriesHitBackfaces = true;
            var hitCount = Physics.BoxCastNonAlloc(raycastCenter, halfExtent, transform.up, hits, transform.rotation, float.MaxValue, layerMask);
            Physics.queriesHitBackfaces = queriesHitBackfaces;

            int? nearestHitIndex = null;
            var nearestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                if (hits[i].distance < nearestDistance && IsDescendant(hits[i].transform, transform))
                {
                    nearestHitIndex = i;
                    nearestDistance = hits[i].distance;
                }
            }

            if (nearestHitIndex.HasValue)
            {
                var hitPointLocal = transform.InverseTransformPoint(hits[nearestHitIndex.Value].point);

                if (0 <= hitPointLocal.y && hitPointLocal.y <= 0.005)
                {
                    return;
                }

                var heightChange = hitPointLocal.y / scale;
                centerTargetEllipsoidalHeight = (float)center.EllipsoidalHeight + heightChange;
            }
        }

        #region Save load methods

        private void SaveCenterPosition()
        {
            var centerString = $"{Center.Latitude} {Center.Longitude} {Center.EllipsoidalHeight}";
            PlayerPrefs.SetString(PlayerPrefs_CenterKey, centerString);
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
        }

        private void LoadScale()
        {
            var scale = PlayerPrefs.GetFloat(PlayerPrefs_ScaleKey);
            if (scale > 0)
            {
                Scale = scale;
            }
        }

        private void SaveAutoAdjustCenterHeight()
        {
            PlayerPrefs.SetInt(PlayerPrefs_AutoAdjustCenterHeightKey, autoAdjustCenterHeight ? 1 : 0);
        }

        private void LoadAutoAdjustCenterHeight()
        {
            var value = PlayerPrefs.GetInt(PlayerPrefs_AutoAdjustCenterHeightKey, -1);
            if (value == -1)
            {
                return;
            }

            AutoAdjustCenterHeight = value == 1;
        }

        #endregion

        #region Update map methods

        private void UpdateMap()
        {
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

        #endregion

        #region Invoke events

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

        private void InvokeOnAutoAdjustCenterHeightChanged(bool autoAdjustCenterHeight)
        {
            try
            {
                OnAutoAdjustCenterHeightChanged?.Invoke(autoAdjustCenterHeight);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        #endregion

        #region Utility methods

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

        private static bool IsDescendant(Transform target, Transform parent)
        {
            var targetParent = target.parent;
            while (targetParent != null)
            {
                if (targetParent == parent)
                {
                    return true;
                }
                targetParent = targetParent.parent;
            }
            return false;
        }

        #endregion
    }
}
