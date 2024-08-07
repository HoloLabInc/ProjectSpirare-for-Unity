using HoloLab.PositioningTools.GeographicCoordinate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace HoloLab.Spirare
{
    public sealed class GeometryElementComponent : SpecificObjectElementComponentBase<PomlGeometryElement>
    {
        private enum RenderType
        {
            MeshRenderer = 0,
            LineRenderer
        }

        private GeoReferenceElementComponentFactory geoReferenceElementComponentFactory;

        private Material lineMaterial;
        private Material polygonMaterial;

        private readonly List<GameObject> geometryObjects = new List<GameObject>();

        public void Initialize(PomlGeometryElement element, GeoReferenceElementComponentFactory geoReferenceElementComponentFactory, PomlLoadOptions loadOptions,
            Material lineMaterial, Material polygonMaterial)
        {
            base.Initialize(element, loadOptions);
            this.geoReferenceElementComponentFactory = geoReferenceElementComponentFactory;
            this.lineMaterial = lineMaterial;
            this.polygonMaterial = polygonMaterial;
        }

        protected override Task UpdateGameObjectCore()
        {
            DestroyGeometryObjects();

            if (DisplayType == PomlDisplayType.None || DisplayType == PomlDisplayType.Occlusion)
            {
                return Task.CompletedTask;
            }

            foreach (var geometry in element.Geometries)
            {
                switch (geometry.Type)
                {
                    case PomlGeometryType.Line:
                        if (geometry is LineGeometry line)
                        {
                            RenderType renderType;
                            if (line.Width > 0)
                            {
                                renderType = RenderType.LineRenderer;
                            }
                            else
                            {
                                renderType = RenderType.MeshRenderer;
                            }
                            var lineObject = CreateLine(line, geoReferenceElementComponentFactory, transform, renderType, lineMaterial);
                            geometryObjects.Add(lineObject);
                        }
                        break;
                    case PomlGeometryType.Polygon:
                        if (geometry is PolygonGeometry polygon)
                        {
                            var polygonObject = CreatePolygon(polygon, geoReferenceElementComponentFactory, transform, polygonMaterial);
                            geometryObjects.Add(polygonObject);
                        }
                        break;
                    case PomlGeometryType.Unknown:
                    default:
                        {
                            break;
                        }
                }
            }
            return Task.CompletedTask;
        }

        private void DestroyGeometryObjects()
        {
            foreach (var go in geometryObjects)
            {
                Destroy(go);
            }

            geometryObjects.Clear();
        }

        private static GameObject CreateLine(LineGeometry line, GeoReferenceElementComponentFactory geoReferenceElementComponentFactory, Transform parent, RenderType renderType, Material baseMaterial)
        {
            var lineObj = new GameObject("line");
            lineObj.transform.SetParent(parent, worldPositionStays: false);

            var geometryVertices = PomlGeometryParserUtility.ParseAsGeometryPositionsAttribute(line.Vertices);

            if (geometryVertices.CoordinateSystem == PomlGeometryPositionsAttribute.CoordinateSystemType.Unknown)
            {
                return lineObj;
            }

            AddGeoreferenceElementComponentWhenGeodetic(lineObj, geometryVertices, geoReferenceElementComponentFactory);
            var points = ConvertPomlGeometryPositionsAttributeToUnityPositions(geometryVertices);

            switch (renderType)
            {
                case RenderType.MeshRenderer:
                    {
                        var meshRenderer = lineObj.AddComponent<MeshRenderer>();

                        var material = new Material(baseMaterial)
                        {
                            color = line.Color
                        };
                        meshRenderer.material = material;

                        var meshFilter = lineObj.AddComponent<MeshFilter>();
                        var mesh = meshFilter.mesh;
                        mesh.vertices = points;

                        var indices = Enumerable.Range(0, points.Length).ToArray();
                        mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
                    }
                    break;
                case RenderType.LineRenderer:
                    {
                        var lineRenderer = lineObj.AddComponent<LineRenderer>();

                        lineRenderer.useWorldSpace = false;

                        lineRenderer.startWidth = line.Width;
                        lineRenderer.endWidth = line.Width;

                        lineRenderer.startColor = line.Color;
                        lineRenderer.endColor = line.Color;

                        var material = new Material(baseMaterial)
                        {
                            color = line.Color
                        };
                        lineRenderer.material = material;

                        lineRenderer.positionCount = points.Length;
                        lineRenderer.SetPositions(points);
                    }
                    break;
            }
            return lineObj;
        }

        private static GameObject CreatePolygon(PolygonGeometry polygon, GeoReferenceElementComponentFactory geoReferenceElementComponentFactory, Transform parent, Material baseMaterial)
        {
            var polygonObj = new GameObject("polygon");
            polygonObj.transform.SetParent(parent, worldPositionStays: false);

            var geometryVertices = PomlGeometryParserUtility.ParseAsGeometryPositionsAttribute(polygon.Vertices);

            if (geometryVertices.CoordinateSystem == PomlGeometryPositionsAttribute.CoordinateSystemType.Unknown)
            {
                return polygonObj;
            }

            AddGeoreferenceElementComponentWhenGeodetic(polygonObj, geometryVertices, geoReferenceElementComponentFactory);

            var meshFilter = polygonObj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = ConvertPolygonGeometryToMesh(polygon, geometryVertices);

            var material = new Material(baseMaterial)
            {
                color = polygon.Color
            };

            var meshRenderer = polygonObj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            return polygonObj;
        }

        private static void AddGeoreferenceElementComponentWhenGeodetic(GameObject targetObject, PomlGeometryPositionsAttribute geometryVertices,
            GeoReferenceElementComponentFactory geoReferenceElementComponentFactory)
        {
            if (geometryVertices.CoordinateSystem == PomlGeometryPositionsAttribute.CoordinateSystemType.Geodetic)
            {
                if (geometryVertices.GeodeticPositions.Length > 0)
                {
                    var firstVertex = geometryVertices.GeodeticPositions[0];
                    var geoReference = new PomlGeoReferenceElement()
                    {
                        Latitude = firstVertex.Latitude,
                        Longitude = firstVertex.Longitude,
                        EllipsoidalHeight = firstVertex.EllipsoidalHeight
                    };
                    geoReferenceElementComponentFactory.AddComponent(targetObject, geoReference);
                }
            }
        }

        private static Vector3[] ConvertPomlGeometryPositionsAttributeToUnityPositions(PomlGeometryPositionsAttribute geometryPositions)
        {
            switch (geometryPositions.CoordinateSystem)
            {
                case PomlGeometryPositionsAttribute.CoordinateSystemType.Relative:
                    return geometryPositions.RelativePositions.Select(x => CoordinateUtility.ToUnityCoordinate(x)).ToArray();

                case PomlGeometryPositionsAttribute.CoordinateSystemType.Geodetic:
                    var firstVertex = geometryPositions.GeodeticPositions.FirstOrDefault();
                    return geometryPositions.GeodeticPositions.Select(x => GeodeticToRelative(x, firstVertex)).ToArray();

                default:
                    return Array.Empty<Vector3>();
            }
        }

        private static Mesh ConvertPolygonGeometryToMesh(PolygonGeometry polygon, PomlGeometryPositionsAttribute geometryVertices)
        {
            var vertices = ConvertPomlGeometryPositionsAttributeToUnityPositions(geometryVertices);

            var indices = polygon.Indices;
            var triangles = new int[indices.Length];
            for (int i = 0; i < indices.Length / 3; i += 1)
            {
                triangles[i * 3] = indices[i * 3];
                // Swap 2nd and 3rd
                triangles[i * 3 + 1] = indices[i * 3 + 2];
                triangles[i * 3 + 2] = indices[i * 3 + 1];
            }

            var indexFormat = vertices.Length <= 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;

            var mesh = new Mesh()
            {
                indexFormat = indexFormat,
                vertices = vertices,
                triangles = triangles,
            };
            return mesh;
        }

        private static Vector3 GeodeticToRelative(PomlGeodeticPosition target, PomlGeodeticPosition origin)
        {
            return GeographicCoordinateConversion.GeodeticToEnu(
                target.Latitude, target.Longitude, target.EllipsoidalHeight,
                origin.Latitude, origin.Longitude, origin.EllipsoidalHeight)
                .ToUnityVector();
        }
    }
}
