using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Pcx
{
    // add create menu
    [CreateAssetMenu(menuName = "Spirare/PlyModelElementObjectFactory")]
    public class PlyModelElementObjectFactory : ModelElementObjectFactory
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

            var modelElementComponent = go.AddComponent<PlyModelElementComponent>();
            modelElementComponent.Initialize(element, loadOptions);
            _ = modelElementComponent.UpdateGameObject();

            return go;
        }
    }
}
