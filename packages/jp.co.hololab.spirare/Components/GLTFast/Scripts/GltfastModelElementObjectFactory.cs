using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class GltfastModelElementObjectFactory : ModelElementObjectFactory
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

            var modelElementComponent = go.AddComponent<GltfastModelElementComponent>();
            modelElementComponent.Initialize(element, loadOptions);
            _ = modelElementComponent.UpdateGameObject();

            return go;
        }
    }
}
