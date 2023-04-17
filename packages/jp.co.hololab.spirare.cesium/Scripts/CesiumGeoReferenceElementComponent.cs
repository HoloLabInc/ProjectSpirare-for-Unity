using CesiumForUnity;

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
            GeoReferenceElement = geoReferenceElement;

            anchor = gameObject.AddComponent<CesiumGlobeAnchor>();
            UpdateAnchorPose();

            return this;
        }

        private void UpdateAnchorPose()
        {
            if (anchor != null)
            {
                var longitude = GeoReferenceElement.Longitude;
                var latitude = GeoReferenceElement.Latitude;
                var height = GeoReferenceElement.EllipsoidalHeight;
                anchor.SetPositionLongitudeLatitudeHeight(longitude, latitude, height);

                var enuRotation = CoordinateUtility.ToUnityCoordinate(GeoReferenceElement.EnuRotation);
                gameObject.transform.rotation = enuRotation;
            }
        }
    }
}
