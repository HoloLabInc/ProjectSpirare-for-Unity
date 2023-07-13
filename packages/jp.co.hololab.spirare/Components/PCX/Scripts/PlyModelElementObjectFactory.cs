using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Pcx
{
    public class PlyModelElementObjectFactory : ModelElementObjectFactory
    {
        [SerializeField]
        private PlyModelElementComponent plyModelElementComponentPrefab;

        public override GameObject CreateObject(PomlModelElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            var plyModelElementComponent = Instantiate(plyModelElementComponentPrefab, parentTransform);
            plyModelElementComponent.name = "model";

            plyModelElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            plyModelElementComponent.Initialize(element, loadOptions);
            _ = plyModelElementComponent.UpdateGameObject();

            return plyModelElementComponent.gameObject;
        }
    }
}
