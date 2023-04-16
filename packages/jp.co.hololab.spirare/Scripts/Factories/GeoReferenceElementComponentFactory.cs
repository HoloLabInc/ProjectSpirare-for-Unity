using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class GeoReferenceElementComponentFactory : ScriptableObject
    {
        public abstract GeoReferenceElementComponent AddComponent(GameObject gameObject, PomlGeoReferenceElement geoReferenceElement);
    }
}
