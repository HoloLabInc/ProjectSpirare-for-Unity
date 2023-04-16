using UnityEngine;

namespace HoloLab.Spirare
{
    public class RealWorldGeoReferenceElementComponentFactory : GeoReferenceElementComponentFactory
    {
        public override GeoReferenceElementComponent AddComponent(GameObject gameObject, PomlGeoReferenceElement geoReferenceElement)
        {
            var elementComponent = gameObject.AddComponent<RealWorldGeoReferenceElementComponent>();
            elementComponent.Initialize(geoReferenceElement);
            return elementComponent;
        }
    }
}
