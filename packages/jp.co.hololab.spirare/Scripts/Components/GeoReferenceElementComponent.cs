using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class GeoReferenceElementComponent : PomlElementComponent
    {
        public PomlGeoReferenceElement GeoReferenceElement { get; protected set; }
        public override PomlElement PomlElement => GeoReferenceElement;
    }
}
