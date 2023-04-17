using UnityEngine;

namespace HoloLab.Spirare.Wasm.Iwasm
{
    public sealed class IwasmScriptElementComponentFactory : ScriptElementComponentFactory
    {
        public override PomlElementComponent AddComponent(GameObject gameObject, PomlScriptElement scriptElement, PomlObjectElementComponent parentElementComponent, PomlComponent pomlComponent)
        {
            var scriptElementComponent = gameObject.AddComponent<IwasmScriptElementComponent>();
            scriptElementComponent.Initialize(scriptElement, parentElementComponent, pomlComponent);
            return scriptElementComponent;
        }
    }
}
