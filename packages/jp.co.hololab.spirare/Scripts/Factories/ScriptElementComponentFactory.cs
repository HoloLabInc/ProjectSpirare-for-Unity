using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class ScriptElementComponentFactory : ScriptableObject
    {
        public abstract PomlElementComponent AddComponent(GameObject gameObject, PomlScriptElement scriptElement, PomlObjectElementComponent parentElementComponent, PomlComponent pomlComponent);
    }
}
