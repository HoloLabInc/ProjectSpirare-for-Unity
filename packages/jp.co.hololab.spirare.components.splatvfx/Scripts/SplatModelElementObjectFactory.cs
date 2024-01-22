using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace HoloLab.Spirare.Components.SplatVfx
{
    public class SplatModelElementObjectFactory : ModelElementObjectFactory
    {
        [SerializeField]
        private VisualEffect splatPrefab;

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
            modelElementComponent.Initialize(element, loadOptions, splatPrefab);
            _ = modelElementComponent.UpdateGameObject();

            return go;
        }
    }
}
