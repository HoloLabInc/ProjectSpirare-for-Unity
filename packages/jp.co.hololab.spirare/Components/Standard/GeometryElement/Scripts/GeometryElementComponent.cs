using HoloLab.PositioningTools.GeographicCoordinate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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

        /*
        private static Vector3[] ConvertLineGeometryToPoints(LineGeometry line)
        {
            switch (line.PositionType)
            {
                case PositionType.Relative:
                    return new Vector3[2]
                    {
                        CoordinateUtility.ToUnityCoordinate(line.Start),
                        CoordinateUtility.ToUnityCoordinate(line.End),
                    };
                case PositionType.GeoLocation:
                    var gStart = line.StartGeoLocation;
                    var gEnd = line.EndGeoLocation;

                    var endPosEnu = GeographicCoordinateConversion.GeodeticToEnu(
                        gEnd.Latitude, gEnd.Longitude, gEnd.EllipsoidalHeight,
                        gStart.Latitude, gStart.Longitude, gStart.EllipsoidalHeight);
                    return new Vector3[2]
                    {
                        Vector3.zero,
                        endPosEnu.ToUnityVector(),
                    };
                default:
                    return Array.Empty<Vector3>();
            }
        }
        */

        private static Vector3[] ConvertPomlGeometryVerticesAttributeToUnityVertices(PomlGeometryVerticesAttribute vertices)
        {
            switch (vertices.CoordinateSystem)
            {
                case PomlGeometryVerticesAttribute.CoordinateSystemType.Relative:
                    return vertices.RelativePositions.Select(x => CoordinateUtility.ToUnityCoordinate(x)).ToArray();
                /*
                return new Vector3[2]
                {
                    CoordinateUtility.ToUnityCoordinate(line.Start),
                    CoordinateUtility.ToUnityCoordinate(line.End),
                };
                */
                case PomlGeometryVerticesAttribute.CoordinateSystemType.Geodetic:
                    var firstVertex = vertices.GeodeticPositions.FirstOrDefault();
                    return vertices.GeodeticPositions.Select(x => GeodeticToRelative(x, firstVertex)).ToArray();

                /*
                var gStart = line.StartGeoLocation;
                var gEnd = line.EndGeoLocation;

                var endPosEnu = GeographicCoordinateConversion.GeodeticToEnu(
                    gEnd.Latitude, gEnd.Longitude, gEnd.EllipsoidalHeight,
                    gStart.Latitude, gStart.Longitude, gStart.EllipsoidalHeight);
                return new Vector3[2]
                {
                    Vector3.zero,
                    endPosEnu.ToUnityVector(),
                };
                */
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

        private static void AddGeoreferenceElementComponentWhenGeodetic(GameObject targetObject, PomlGeometryVerticesAttribute geometryVertices,
            GeoReferenceElementComponentFactory geoReferenceElementComponentFactory)
        {
            if (geometryVertices.CoordinateSystem == PomlGeometryVerticesAttribute.CoordinateSystemType.Geodetic)
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

        private static GameObject CreateLine(LineGeometry line, GeoReferenceElementComponentFactory geoReferenceElementComponentFactory, Transform parent, RenderType renderType, Material baseMaterial)
        {
            var lineObj = new GameObject("line");
            lineObj.transform.SetParent(parent, worldPositionStays: false);

            if (TryParserGeometryVertices(line.Vertices, out var geometryVertices) == false)
            {
                return lineObj;
            }

            AddGeoreferenceElementComponentWhenGeodetic(lineObj, geometryVertices, geoReferenceElementComponentFactory);
            var points = ConvertPomlGeometryVerticesAttributeToUnityVertices(geometryVertices);

            /*

            if (geometryVertices.CoordinateSystem == PomlGeometryVerticesAttribute.CoordinateSystemType.Geodetic)
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
                    geoReferenceElementComponentFactory.AddComponent(lineObj, geoReference);
                }
            }
            */

            /*
            if (line.PositionType == PositionType.GeoLocation)
            {
                var gStart = line.StartGeoLocation;
                var geoReference = new PomlGeoReferenceElement()
                {
                    Latitude = gStart.Latitude,
                    Longitude = gStart.Longitude,
                    EllipsoidalHeight = gStart.EllipsoidalHeight
                };
                geoReferenceElementComponentFactory.AddComponent(lineObj, geoReference);
            }
            */

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
                        mesh.SetIndices(new int[] { 0, 1 }, MeshTopology.Lines, 0);
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

            if (TryParserGeometryVertices(polygon.Vertices, out var geometryVertices) == false)
            {
                return polygonObj;
            }

            AddGeoreferenceElementComponentWhenGeodetic(polygonObj, geometryVertices, geoReferenceElementComponentFactory);
            /*
            if (polygon.PositionType == PositionType.GeoLocation)
            {
                if (polygon.GeodeticVertices.Length > 0)
                {
                    var firstVertex = polygon.GeodeticVertices[0];
                    var geoReference = new PomlGeoReferenceElement()
                    {
                        Latitude = firstVertex.Latitude,
                        Longitude = firstVertex.Longitude,
                        EllipsoidalHeight = firstVertex.EllipsoidalHeight
                    };
                    geoReferenceElementComponentFactory.AddComponent(polygonObj, geoReference);
                }
            }
            */

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

        private static Mesh ConvertPolygonGeometryToMesh(PolygonGeometry polygon, PomlGeometryVerticesAttribute geometryVertices)
        {
            var vertices = ConvertPomlGeometryVerticesAttributeToUnityVertices(geometryVertices);

            /*
        Vector3[] vertices;
        switch (polygon.PositionType)
        {
            case PositionType.Relative:
                vertices = polygon.Vertices.Select(x => CoordinateUtility.ToUnityCoordinate(x)).ToArray();
                break;
            case PositionType.GeoLocation:
                if (polygon.GeodeticVertices.Length == 0)
                {
                    return new Mesh();
                }

                var firstVertex = polygon.GeodeticVertices[0];
                vertices = polygon.GeodeticVertices.Select(x =>
                    GeographicCoordinateConversion.GeodeticToEnu(
                        x.Latitude, x.Longitude, x.EllipsoidalHeight,
                        firstVertex.Latitude, firstVertex.Longitude, firstVertex.EllipsoidalHeight)
                    .ToUnityVector()
                ).ToArray();
                break;
            default:
                return new Mesh();
        }
            */

            var indices = polygon.Indices;
            var triangles = new int[indices.Length];
            for (int i = 0; i < indices.Length / 3; i += 1)
            {
                triangles[i * 3] = indices[i * 3];
                // Swap 2nd and 3rd
                triangles[i * 3 + 1] = indices[i * 3 + 2];
                triangles[i * 3 + 2] = indices[i * 3 + 1];
            }

            var mesh = new Mesh()
            {
                vertices = vertices,
                triangles = triangles
            };
            return mesh;
        }

        private static bool TryParserGeometryVertices(string attribute, out PomlGeometryVerticesAttribute geometryVertices)
        {
            attribute = attribute.Trim();
            if (attribute.StartsWith("geodetic:"))
            {

            }

            geometryVertices = PomlGeometryVerticesAttribute.CreateUnkown();
            return true;
        }
    }

    public class PomlGeometryVerticesAttribute
    {
        public enum CoordinateSystemType
        {
            Unknown = 0,
            Relative,
            Geodetic
        }

        public CoordinateSystemType CoordinateSystem { get; }

        public Vector3[] RelativePositions { get; }

        public PomlGeodeticPosition[] GeodeticPositions { get; }

        protected PomlGeometryVerticesAttribute(
            CoordinateSystemType coordinateSystem,
            Vector3[] relativePositions,
            PomlGeodeticPosition[] geodeticPositions)
        {
            CoordinateSystem = coordinateSystem;
            RelativePositions = relativePositions;
            GeodeticPositions = geodeticPositions;
        }

        public static PomlGeometryVerticesAttribute CreateUnkown()
        {
            return new PomlGeometryVerticesAttribute(CoordinateSystemType.Unknown, null, null);
        }

        public static PomlGeometryVerticesAttribute CreateRelative(Vector3[] positions)
        {
            return new PomlGeometryVerticesAttribute(CoordinateSystemType.Relative, positions, null);
        }
    }
}
