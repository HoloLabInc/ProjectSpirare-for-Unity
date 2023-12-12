using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Splat
{
    public class SplatModelElementObjectFactory : ModelElementObjectFactory
    {
        public override GameObject CreateObject(PomlModelElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            var go = new GameObject("model");

            if (parentTransform != null)
            {
                go.transform.SetParent(parentTransform, false);
            }

            var pomlObjectElementComponent = go.AddComponent<PomlObjectElementComponent>();
            pomlObjectElementComponent.Initialize(element);

            var modelElementComponent = go.AddComponent<SplatModelElementComponent>();
            modelElementComponent.Initialize(element, loadOptions);
            _ = modelElementComponent.UpdateGameObject();

            return go;
        }
    }
}
