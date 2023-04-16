using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class SpaceReferenceElementComponentFactory : ScriptableObject
    {
        public abstract SpaceReferenceElementComponent AddComponent(GameObject gameObject, PomlSpaceReferenceElement referenceElement);
    }
}
