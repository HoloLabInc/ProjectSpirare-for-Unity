using UnityEngine;

namespace HoloLab.Spirare
{
    public class RealWorldSpaceReferenceElementComponentFactory : SpaceReferenceElementComponentFactory
    {
        public override SpaceReferenceElementComponent AddComponent(GameObject gameObject, PomlSpaceReferenceElement spaceReferenceElement)
        {
            var pec = gameObject.AddComponent<RealWorldSpaceReferenceElementComponent>();
            pec.Initialize(spaceReferenceElement);
            return pec;
        }
    }
}
