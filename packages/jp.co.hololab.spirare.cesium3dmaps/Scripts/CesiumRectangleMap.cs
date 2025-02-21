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
        [Tooltip("90 means the east is the forward direction.")]
        private float heading;

        public float Heading
        {
            get
            {
                return heading;
            }
            set
            {
                heading = value;
                SaveHeading();
                UpdateMap();
                InvokeOnHeadingChanged(value);
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

        [SerializeField]
        private BaseMapSettings baseMapSettings;

        public BaseMapSettings BaseMapSettings => baseMapSettings;

        private int baseMapIndex = -1;

        public int BaseMapIndex => baseMapIndex;

        private GameObject baseMapObject;

        private CesiumGeoreference[] cesiumGeoreferences;
        private CesiumGeodeticAreaExcluder[] cesiumGeodeticAreaExcluders;

        private Camera mainCamera;
        private RaycastHit[] hits = new RaycastHit[100];

        private const string PlayerPrefs_CenterKey = "CesiumRectangleMap_Center";
        private const string PlayerPrefs_ScaleKey = "CesiumRectangleMap_Scale";
        private const string PlayerPrefs_HeadingKey = "CesiumRectangleMap_Heading";
        private const string PlayerPrefs_AutoAdjustCenterHeightKey = "CesiumRectangleMap_AutoAdjustCenterHeight";
        private const string PlayerPrefs_BaseMapIndexKey = "CesiumRectangleMap_BaseMapIndex";
        private const string PlayerPrefs_BaseMapNameKey = "CesiumRectangleMap_BaseMapName";

        public event Action<float> OnScaleChanged;
        public event Action<GeodeticPosition> OnCenterChanged;
        public event Action<float> OnHeadingChanged;
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
            LoadHeading();
            LoadScale();
            LoadAutoAdjustCenterHeight();
            LoadBaseMapSelection();

            ChangeBaseMap(baseMapIndex);
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

        private void OnApplicationFocus(bool focus)
        {
            if (focus == false)
            {
                PlayerPrefs.Save();
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

        public void SelectBaseMap(int index)
        {
            if (baseMapSettings == null)
            {
                Debug.LogWarning("BaseMapSettings is not set.");
                return;
            }

            var baseMaps = baseMapSettings.BaseMaps;
            if (index < 0 || index >= baseMaps.Count)
            {
                return;
            }

            if (baseMapIndex != index)
            {
                ChangeBaseMap(index);
            }

            baseMapIndex = index;
            SaveBaseMapSelection();
        }

        private void ChangeBaseMap(int index)
        {
            if (baseMapSettings == null)
            {
                Debug.LogWarning("BaseMapSettings is not set.");
                return;
            }

            if (cesiumGeoreferences.Length == 0)
            {
                Debug.LogWarning("CesiumGeoreference is not found.");
                return;
            }

            var baseMaps = baseMapSettings.BaseMaps;
            if (index < 0 || index >= baseMaps.Count)
            {
                return;
            }

            if (baseMapObject != null)
            {
                Destroy(baseMapObject);
            }

            var baseMapSetting = baseMaps[index];
            baseMapObject = Instantiate(baseMapSetting.MapPrefab, cesiumGeoreferences[0].transform);

            if (attatchTilesetClipperForChildTilesets)
            {
                if (baseMapObject.TryGetComponent<Cesium3DTileset>(out var tileset))
                {
                    if (tileset.gameObject.TryGetComponent<CesiumRectangleMapTilesetClipper>(out _) == false)
                    {
                        tileset.gameObject.AddComponent<CesiumRectangleMapTilesetClipper>();
                    }
                }
            }

            mapBase.ChangeCredit(baseMapSetting.CreditPrefab);
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
            // Wait a few seconds for initial loading
            yield return new WaitForSeconds(3f);

            while (true)
            {
                if (autoAdjustCenterHeight && MapOriginIsVisible())
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

        private void SaveHeading()
        {
            PlayerPrefs.SetFloat(PlayerPrefs_HeadingKey, Heading);
        }

        private void LoadHeading()
        {
            var heading = PlayerPrefs.GetFloat(PlayerPrefs_HeadingKey);
            if (heading > 0)
            {
                Heading = heading;
            }
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

        private void SaveBaseMapSelection()
        {
            PlayerPrefs.SetInt(PlayerPrefs_BaseMapIndexKey, baseMapIndex);
            if (baseMapSettings != null)
            {
                var baseMaps = baseMapSettings.BaseMaps;
                if (0 < baseMapIndex || baseMapIndex < baseMaps.Count)
                {
                    var baseMapName = baseMaps[baseMapIndex].MapName;
                    PlayerPrefs.SetString(PlayerPrefs_BaseMapNameKey, baseMapName);
                }
            }
        }

        private void LoadBaseMapSelection()
        {
            var mapName = PlayerPrefs.GetString(PlayerPrefs_BaseMapNameKey, "");
            if (string.IsNullOrEmpty(mapName) == false && baseMapSettings != null)
            {
                var baseMaps = baseMapSettings.BaseMaps;
                for (var i = 0; i < baseMaps.Count; i++)
                {
                    if (baseMaps[i].MapName == mapName)
                    {
                        baseMapIndex = i;
                        return;
                    }
                }
            }

            baseMapIndex = PlayerPrefs.GetInt(PlayerPrefs_BaseMapIndexKey, -1);

            if (baseMapIndex < 0)
            {
                baseMapIndex = 0;
            }

            if (baseMapSettings != null && baseMapSettings.BaseMaps.Count <= baseMapIndex)
            {
                baseMapIndex = 0;
            }
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
                georeference.transform.localRotation = Quaternion.AngleAxis(-heading, Vector3.up);
                georeference.transform.localScale = scale * Vector3.one;
            }
        }

        private void UpdateCesiumGeodeticAreaExcluders()
        {
            var halfX = mapSizeX / 2 / scale;
            var halfZ = mapSizeZ / 2 / scale;

            var theta = Mathf.Atan2(mapSizeZ, mapSizeX) + heading * Mathf.Deg2Rad;
            var len = Mathf.Sqrt(halfX * halfX + halfZ * halfZ);

            var upperLeftEnuPosition = new EnuPosition(-len * Mathf.Cos(theta), len * Mathf.Sin(theta), 0);
            var lowerRightEnuPosition = new EnuPosition(-upperLeftEnuPosition.East, -upperLeftEnuPosition.North, 0);

            var upperLeftLonLatHeight = EnuToGeodetic(upperLeftEnuPosition);
            var lowerRightLonLatHeight = EnuToGeodetic(lowerRightEnuPosition);

            foreach (var cesiumGeodeticAreaExcluder in cesiumGeodeticAreaExcluders)
            {
                cesiumGeodeticAreaExcluder.UpperLeftLongitude = upperLeftLonLatHeight.Longitude;
                cesiumGeodeticAreaExcluder.UpperLeftLatitude = upperLeftLonLatHeight.Latitude;

                cesiumGeodeticAreaExcluder.LowerRightLongitude = lowerRightLonLatHeight.Longitude;
                cesiumGeodeticAreaExcluder.LowerRightLatitude = lowerRightLonLatHeight.Latitude;

                cesiumGeodeticAreaExcluder.NorthHeading = heading;
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

        private void InvokeOnHeadingChanged(float heading)
        {
            try
            {
                OnHeadingChanged?.Invoke(heading);
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

        private bool MapOriginIsVisible()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    return false;
                }
            }

            var viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
            if (viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z >= 0)
            {
                return true;
            }
            else
            {
                return false;
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
