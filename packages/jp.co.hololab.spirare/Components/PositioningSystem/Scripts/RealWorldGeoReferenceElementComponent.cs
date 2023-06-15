using HoloLab.PositioningTools.CoordinateSystem;
using HoloLab.PositioningTools.GeographicCoordinate;
using UnityEngine;

namespace HoloLab.Spirare
{
    public sealed class RealWorldGeoReferenceElementComponent : GeoReferenceElementComponent
    {
        private WorldCoordinateOrigin worldCoordinateOrigin;

        private void Start()
        {
            if (TryGetComponent<PomlObjectElementComponent>(out var elementComponent))
            {
                elementComponent.OnElementUpdated += _ =>
                {
                    if (worldCoordinateOrigin != null)
                    {
                        UpdateGameObject();

                        var wb = CoordinateManager.Instance.LatestWorldBinding;
                        worldCoordinateOrigin.BindCoordinates(wb);
                    }
                };
            }
        }

        internal RealWorldGeoReferenceElementComponent Initialize(PomlGeoReferenceElement geoReferenceElement)
        {
            base.Initialize(geoReferenceElement);

            GeoReferenceElement = geoReferenceElement;
            worldCoordinateOrigin = gameObject.AddComponent<WorldCoordinateOrigin>();

            UpdateGameObject();

            return this;
        }

        private void UpdateGameObject()
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
