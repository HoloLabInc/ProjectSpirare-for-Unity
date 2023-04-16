using HoloLab.PositioningTools.GeographicCoordinate;
using System;
using System.Collections.Generic;
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
    }
}
