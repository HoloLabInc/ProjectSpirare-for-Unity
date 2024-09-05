#if PRESENT_CESIUM_CARTOGRAPHIC_POLYGON && PRESENT_SPLINES
#define CESIUM_CARTOGRAPHIC_POLYGON_ENABLED
#endif

using CesiumForUnity;
using HoloLab.PositioningTools.GeographicCoordinate;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

#if CESIUM_CARTOGRAPHIC_POLYGON_ENABLED
using System.Collections.Generic;
using UnityEngine.Splines;
#endif

namespace HoloLab.Spirare.Cesium
{
    [RequireComponent(typeof(Cesium3DTileset))]
    public class Cesium3dTilesElementComponent : SpecificObjectElementComponentBase<PomlCesium3dTilesElement>
    {
        private Cesium3DTileset tileset;

#if CESIUM_CARTOGRAPHIC_POLYGON_ENABLED
        private readonly List<CesiumCartographicPolygon> cesiumCartographicPolygons = new List<CesiumCartographicPolygon>();
        private GameObject maskObject;
        private CesiumPolygonRasterOverlay cesiumPolygonRasterOverlay;
#endif

        private bool useLocalFileServer = false;

        private static LocalFileServer localFileServer;
        private static int localFileServerUsageCount = 0;

        private void Awake()
        {
            tileset = GetComponent<Cesium3DTileset>();
        }

        private void OnDestroy()
        {
            if (useLocalFileServer)
            {
                localFileServerUsageCount -= 1;
                if (localFileServerUsageCount == 0)
                {
                    localFileServer.Dispose();
                    localFileServer = null;
                }
            }
        }

        protected override Task UpdateGameObjectCore()
        {
            StartLocalFileServerIfNeeded(element);

            var tilesetUrl = GetTilesetUrl(element);
            if (tileset.url != tilesetUrl)
            {
                tileset.url = tilesetUrl;
            }

            UpdateMask();

            return Task.CompletedTask;
        }

        private void StartLocalFileServerIfNeeded(PomlCesium3dTilesElement cesium3dTilesElement)
        {
            var url = cesium3dTilesElement.Src;
            if (url.StartsWith("file://"))
            {
                if (useLocalFileServer == false)
                {
                    useLocalFileServer = true;
                    localFileServerUsageCount += 1;
                }

                if (localFileServer == null)
                {
                    localFileServer = new LocalFileServer();
                    localFileServer.StartOnRandomPort();
                }
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
                cesiumPolygonRasterOverlay.materialKey = "Clipping";
            }

            cesiumPolygonRasterOverlay.polygons = cesiumCartographicPolygons;
        }

        private CesiumCartographicPolygon CreateCesiumCartographicPolygon(string name, Transform parent, PomlCesium3dTilesMaskBounds bounds)
        {
            var boundsObject = new GameObject(name);
            boundsObject.transform.SetParent(parent, false);

            var splineContainer = boundsObject.AddComponent<SplineContainer>();
            var spline = splineContainer.Spline;
            spline.Closed = true;

            var vertices = Poml3dTilesParserUtility.ParseAsBoundsVerticesAttribute(bounds.Vertices);
            var points = ConvertBoundsVerticesAttributeToUnityPositions(vertices);
            foreach (var point in points)
            {
                var knot = new BezierKnot(point);
                spline.Add(knot, TangentMode.Linear);
            }

            if (vertices.CoordinateSystem == PomlBoundsVerticesAttribute.CoordinateSystemType.Geodetic)
            {
                AddCesiumGlobeAnchor(boundsObject, vertices);
            }

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

                if (localFileServer == null)
                {
                    Debug.LogError("Local file server has not started.");
                    return null;
                }

                var localServerUrl = $"http://localhost:{localFileServer.Port}/{fileName}?basepath={directoryPath}";
                return localServerUrl;
            }
            else
            {
                return url;
            }
        }

        private static Vector3[] ConvertBoundsVerticesAttributeToUnityPositions(PomlBoundsVerticesAttribute vertices)
        {
            switch (vertices.CoordinateSystem)
            {
                case PomlBoundsVerticesAttribute.CoordinateSystemType.Relative:
                    return vertices.RelativePositions.Select(v => new Vector3(-v.y, 0, v.x)).ToArray();

                case PomlBoundsVerticesAttribute.CoordinateSystemType.Geodetic:
                    var firstVertex = vertices.GeodeticPositions.FirstOrDefault();
                    return vertices.GeodeticPositions.Select(x => GeodeticToRelative(x, firstVertex)).ToArray();

                default:
                    return Array.Empty<Vector3>();
            }
        }

        private static Vector3 GeodeticToRelative(PomlGeodeticPosition target, PomlGeodeticPosition origin)
        {
            return GeographicCoordinateConversion.GeodeticToEnu(
                target.Latitude, target.Longitude, target.EllipsoidalHeight,
                origin.Latitude, origin.Longitude, origin.EllipsoidalHeight)
                .ToUnityVector();
        }

        private static void AddCesiumGlobeAnchor(GameObject boundsObject, PomlBoundsVerticesAttribute vertices)
        {
            if (vertices.GeodeticPositions.Length == 0)
            {
                return;
            }

            var firstVertex = vertices.GeodeticPositions[0];
            var cesiumGlobeAnchor = boundsObject.AddComponent<CesiumGlobeAnchor>();
            var longitudeLatitudeHeight = new double3(firstVertex.Longitude, firstVertex.Latitude, firstVertex.EllipsoidalHeight);
            cesiumGlobeAnchor.longitudeLatitudeHeight = longitudeLatitudeHeight;
        }
    }
}

