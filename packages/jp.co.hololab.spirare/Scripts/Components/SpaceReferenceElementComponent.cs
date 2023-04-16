using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class SpaceReferenceElementComponent : PomlElementComponent
    {
        public PomlSpaceReferenceElement SpaceReferenceElement { get; protected set; }
        public override PomlElement PomlElement => SpaceReferenceElement;
    }
}
