using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    public class CesiumGeoReferenceElementComponentFactory : GeoReferenceElementComponentFactory
    {
        public override GeoReferenceElementComponent AddComponent(GameObject gameObject, PomlGeoReferenceElement geoReferenceElement)
        {
            var geoReferenceComponent = gameObject.AddComponent<CesiumGeoReferenceElementComponent>();
            geoReferenceComponent.Initialize(geoReferenceElement);
            return geoReferenceComponent;
        }
    }
}
