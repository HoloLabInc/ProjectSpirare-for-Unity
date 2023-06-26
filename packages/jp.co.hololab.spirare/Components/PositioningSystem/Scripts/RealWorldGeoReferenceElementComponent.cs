using HoloLab.PositioningTools.CoordinateSystem;
using HoloLab.PositioningTools.GeographicCoordinate;
using System;
using UnityEngine;

namespace HoloLab.Spirare
{
    public sealed class RealWorldGeoReferenceElementComponent : GeoReferenceElementComponent
    {
        private WorldCoordinateOrigin worldCoordinateOrigin;
        private CoordinateManager coordinateManager;

        private void Start()
        {
            // For update via websocket
            if (TryGetComponent<PomlObjectElementComponent>(out var elementComponent))
            {
                elementComponent.OnElementUpdated += _ =>
                {
                    UpdateGameObject();
                };
            }
        }

        internal RealWorldGeoReferenceElementComponent Initialize(PomlGeoReferenceElement geoReferenceElement)
        {
            base.Initialize(geoReferenceElement);

            GeoReferenceElement = geoReferenceElement;
            worldCoordinateOrigin = gameObject.AddComponent<WorldCoordinateOrigin>();

            coordinateManager = CoordinateManager.Instance;

            UpdateGameObject();
            OnElementUpdated += PomlElementComponent_UpdateGameObject;

            return this;
        }

        private void PomlElementComponent_UpdateGameObject(PomlElement element)
        {
            UpdateGameObject();
        }

        private void UpdateGameObject()
        {
            UpdateWorldCoordinateOrigin();
            BindCoordinates();
        }

        private void BindCoordinates()
        {
            if (coordinateManager != null && worldCoordinateOrigin != null)
            {
                var worldBinding = coordinateManager.LatestWorldBinding;
                if (worldBinding != null)
                {
                    worldCoordinateOrigin.BindCoordinates(worldBinding);
                }
            }
        }

        private void UpdateWorldCoordinateOrigin()
        {
            if (worldCoordinateOrigin == null)
            {
                return;
            }

            var geodeticPosition = new GeodeticPosition(GeoReferenceElement.Latitude, GeoReferenceElement.Longitude, GeoReferenceElement.EllipsoidalHeight);
            worldCoordinateOrigin.GeodeticPosition = geodeticPosition;

            var geoReferenceRotation = CoordinateUtility.ToUnityCoordinate(GeoReferenceElement.EnuRotation);
            var objectElementRotation = CoordinateUtility.ToUnityCoordinate(GeoReferenceElement.Parent?.Rotation ?? Quaternion.identity);
            var enuRotation = geoReferenceRotation * objectElementRotation;

            worldCoordinateOrigin.EnuRotation = enuRotation;
        }
    }
}
