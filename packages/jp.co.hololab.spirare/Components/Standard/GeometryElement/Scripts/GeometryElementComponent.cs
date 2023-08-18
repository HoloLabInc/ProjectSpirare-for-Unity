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

        private readonly List<GameObject> geometryObjects = new List<GameObject>();

        public void Initialize(PomlGeometryElement element, GeoReferenceElementComponentFactory geoReferenceElementComponentFactory, PomlLoadOptions loadOptions)
        {
            base.Initialize(element, loadOptions);
            this.geoReferenceElementComponentFactory = geoReferenceElementComponentFactory;
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
                            var lineObject = CreateLine(line, geoReferenceElementComponentFactory, transform, renderType);
                            geometryObjects.Add(lineObject);
                        }
                        break;
                    case PomlGeometryType.Polygon:
                        if (geometry is PolygonGeometry polygon)
                        {
                            var polygonObject = CreatePolygon(polygon, geoReferenceElementComponentFactory, transform);
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

        private static GameObject CreateLine(LineGeometry line, GeoReferenceElementComponentFactory geoReferenceElementComponentFactory, Transform parent, RenderType renderType)
        {
            var lineObj = new GameObject("line");
            lineObj.transform.SetParent(parent);

            var points = ConvertLineGeometryToPoints(line);
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

            switch (renderType)
            {
                case RenderType.MeshRenderer:
                    {
                        var meshRenderer = lineObj.AddComponent<MeshRenderer>();

                        var material = new Material(Shader.Find("Unlit/Color"));
                        material.color = line.Color;
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

                        var material = new Material(Shader.Find("Unlit/Color"));
                        material.color = line.Color;
                        lineRenderer.material = material;

                        lineRenderer.SetPositions(points);
                    }
                    break;
            }
            return lineObj;
        }

        private GameObject CreatePolygon(PolygonGeometry polygon, GeoReferenceElementComponentFactory geoReferenceElementComponentFactory, Transform parent)
        {
            var polygonObj = new GameObject("polygon");
            polygonObj.transform.SetParent(parent);

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

            var meshFilter = polygonObj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = ConvertPolygonGeometryToMesh(polygon);

            var meshRenderer = polygonObj.AddComponent<MeshRenderer>();

            return polygonObj;
        }

        private static Mesh ConvertPolygonGeometryToMesh(PolygonGeometry polygon)
        {
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

                    var firstPoint = polygon.GeodeticVertices[0];
                    vertices = polygon.GeodeticVertices.Select(x =>
                        GeographicCoordinateConversion.GeodeticToEnu(
                            x.Latitude, x.Longitude, x.EllipsoidalHeight,
                            firstPoint.Latitude, x.Longitude, x.EllipsoidalHeight)
                        .ToUnityVector()
                    ).ToArray();
                    break;
                default:
                    return new Mesh();
            }

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
    }
}
