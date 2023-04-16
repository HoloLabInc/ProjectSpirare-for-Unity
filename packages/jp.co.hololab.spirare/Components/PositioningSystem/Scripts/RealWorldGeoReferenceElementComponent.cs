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
                        worldCoordinateOrigin.GeodeticPosition = new GeodeticPosition(
                            GeoReferenceElement.Latitude,
                            GeoReferenceElement.Longitude,
                            GeoReferenceElement.EllipsoidalHeight);

                        var wb = CoordinateManager.Instance.LatestWorldBinding;
                        worldCoordinateOrigin.BindCoordinates(wb);
                    }
                };
            }
        }

        internal RealWorldGeoReferenceElementComponent Initialize(PomlGeoReferenceElement geoReferenceElement)
        {
            GeoReferenceElement = geoReferenceElement;

            var wco = gameObject.AddComponent<WorldCoordinateOrigin>();
            wco.GeodeticPosition = new GeodeticPosition(geoReferenceElement.Latitude, geoReferenceElement.Longitude, geoReferenceElement.EllipsoidalHeight);
            wco.EnuRotation = CoordinateUtility.ToUnityCoordinate(geoReferenceElement.EnuRotation);
            worldCoordinateOrigin = wco;

            return this;
        }
    }
}
