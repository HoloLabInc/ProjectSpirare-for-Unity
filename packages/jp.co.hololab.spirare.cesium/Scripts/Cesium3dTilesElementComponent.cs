#if PRESENT_CESIUM_CARTOGRAPHIC_POLYGON && PRESENT_SPLINES
#define CESIUM_CARTOGRAPHIC_POLYGON_ENABLED
#endif

using CesiumForUnity;
using HoloLab.PositioningTools.CoordinateSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

namespace HoloLab.Spirare.Cesium
{
    [RequireComponent(typeof(Cesium3DTileset))]
    public class Cesium3dTilesElementComponent : SpecificObjectElementComponentBase<PomlCesium3dTilesElement>
    {
        private Cesium3DTileset tileset;

#if CESIUM_CARTOGRAPHIC_POLYGON_ENABLED
        private List<CesiumCartographicPolygon> cesiumCartographicPolygons = new List<CesiumCartographicPolygon>();
        private GameObject maskObject;
        private CesiumPolygonRasterOverlay cesiumPolygonRasterOverlay;
#endif

        private static LocalFileServer localFileServer;

        private void Awake()
        {
            tileset = GetComponent<Cesium3DTileset>();
        }

        protected override Task UpdateGameObjectCore()
        {
            StartLoacalFileServerIfNeeded(element);

            var tilesetUrl = GetTilesetUrl(element);
            if (tileset.url != tilesetUrl)
            {
                tileset.url = tilesetUrl;
            }

            UpdateMask();

            return Task.CompletedTask;
        }

        private void StartLoacalFileServerIfNeeded(PomlCesium3dTilesElement cesium3dTilesElement)
        {
            if (localFileServer != null)
            {
                return;
            }

            var url = cesium3dTilesElement.Src;
            if (url.StartsWith("file://"))
            {
                localFileServer = new LocalFileServer();
                localFileServer.StartOnRandomPort();
            }
        }

        private void UpdateMask()
        {
#if CESIUM_CARTOGRAPHIC_POLYGON_ENABLED
            ClearMask();

            var masks = element.Masks;
            if (masks.Count == 0)
            {
                if (cesiumPolygonRasterOverlay != null)
                {
                    Destroy(cesiumPolygonRasterOverlay);
                }
                return;
            }

            var mask = masks[0];
            CreateMaskObjects(mask);
            UpdatePolygonRasterOverlay();
#endif
        }

#if CESIUM_CARTOGRAPHIC_POLYGON_ENABLED
        private void ClearMask()
        {
            if (maskObject != null)
            {
                Destroy(maskObject);
            }

            maskObject = null;
            cesiumCartographicPolygons.Clear();

            if (cesiumPolygonRasterOverlay != null)
            {
                cesiumPolygonRasterOverlay.polygons.Clear();
            }
        }

        private void CreateMaskObjects(PomlCesium3dTilesMask mask)
        {
            maskObject = new GameObject("Mask");
            maskObject.transform.SetParent(transform, false);

            foreach (var bounds in mask.Bounds)
            {
                var cesiumCartographicPolygon = CreateCesiumCartographicPolygon("Bounds", maskObject.transform, bounds);
                cesiumCartographicPolygons.Add(cesiumCartographicPolygon);
            }
        }

        private void UpdatePolygonRasterOverlay()
        {
            if (cesiumPolygonRasterOverlay == null)
            {
                cesiumPolygonRasterOverlay = tileset.gameObject.AddComponent<CesiumPolygonRasterOverlay>();
            }

            var polygons = cesiumPolygonRasterOverlay.polygons;
            polygons.AddRange(cesiumCartographicPolygons);
        }

        private CesiumCartographicPolygon CreateCesiumCartographicPolygon(string name, Transform parent, PomlCesium3dTilesMaskBounds bounds)
        {
            var boundsObject = new GameObject(name);
            boundsObject.transform.SetParent(parent, false);

            var splineContainer = boundsObject.AddComponent<SplineContainer>();
            var cesiumGlobeAnchor = boundsObject.AddComponent<CesiumGlobeAnchor>();
            var cesiumCartographicPolygon = boundsObject.AddComponent<CesiumCartographicPolygon>();
            return cesiumCartographicPolygon;
        }
#endif

        private static string GetTilesetUrl(PomlCesium3dTilesElement cesium3dTilesElement)
        {
            var url = cesium3dTilesElement.Src;

            if (url.StartsWith("file://"))
            {
                // Remove file:// from url
                var path = url.Substring(7);

                var directoryPath = Path.GetDirectoryName(path);
                var fileName = Path.GetFileName(path);

                var localServerUrl = $"http://localhost:{localFileServer.Port}/{fileName}?basepath={directoryPath}";
                return localServerUrl;
            }
            else
            {
                return url;
            }
        }
    }
}

