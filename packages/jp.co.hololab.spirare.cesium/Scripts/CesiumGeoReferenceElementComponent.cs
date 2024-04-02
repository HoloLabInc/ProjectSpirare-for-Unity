using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    public sealed class CesiumGeoReferenceElementComponent : GeoReferenceElementComponent
    {
        private CesiumGlobeAnchor anchor;

        private void Start()
        {
            if (TryGetComponent<PomlObjectElementComponent>(out var elementComponent))
            {
                elementComponent.OnElementUpdated += _ =>
                {
                    UpdateAnchorPose();
                };
            }
        }

        internal CesiumGeoReferenceElementComponent Initialize(PomlGeoReferenceElement geoReferenceElement)
        {
            base.Initialize(geoReferenceElement);

            GeoReferenceElement = geoReferenceElement;
            anchor = gameObject.AddComponent<CesiumGlobeAnchor>();

            UpdateAnchorPose();
            OnElementUpdated += PomlElementComponent_OnElementUpdated;

            return this;
        }

        private void PomlElementComponent_OnElementUpdated(PomlElement element)
        {
            UpdateAnchorPose();
        }

        private void UpdateAnchorPose()
        {
            if (anchor == null)
            {
                return;
            }

            var longitude = GeoReferenceElement.Longitude;
            var latitude = GeoReferenceElement.Latitude;
            var height = GeoReferenceElement.EllipsoidalHeight;
            anchor.longitudeLatitudeHeight = new double3(longitude, latitude, height);

            var geoReferenceRotation = CoordinateUtility.ToUnityCoordinate(GeoReferenceElement.EnuRotation);
            var objectElementRotation = CoordinateUtility.ToUnityCoordinate(GeoReferenceElement.Parent?.Rotation ?? Quaternion.identity);
            var enuRotation = geoReferenceRotation * objectElementRotation;

            anchor.rotationEastUpNorth = enuRotation;
        }
    }
}
