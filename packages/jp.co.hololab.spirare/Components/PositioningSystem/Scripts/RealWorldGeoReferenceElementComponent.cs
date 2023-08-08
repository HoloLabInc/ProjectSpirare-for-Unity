using HoloLab.PositioningTools.CoordinateSystem;
using HoloLab.PositioningTools.GeographicCoordinate;
using UnityEngine;

namespace HoloLab.Spirare
{
    public sealed class RealWorldGeoReferenceElementComponent : GeoReferenceElementComponent
    {
        private WorldCoordinateOrigin worldCoordinateOrigin;

#if !POSITIONING_TOOLS_0_2_0_OR_NEWER
        private CoordinateManager coordinateManager;
#endif

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

#if !POSITIONING_TOOLS_0_2_0_OR_NEWER
            coordinateManager = CoordinateManager.Instance;
#endif

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
#if !POSITIONING_TOOLS_0_2_0_OR_NEWER
            BindCoordinates();
#endif
        }

#if !POSITIONING_TOOLS_0_2_0_OR_NEWER
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
#endif

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
