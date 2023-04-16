using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class StandardImageElementObjectFactory : ImageElementObjectFactory
    {
        [SerializeField]
        private ImageElementComponent imageElementComponentPrefab;

        public override GameObject CreateObject(PomlImageElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            var imageElementComponent = Instantiate(imageElementComponentPrefab, parentTransform);
            imageElementComponent.name = "image";

            imageElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            imageElementComponent.Initialize(element, loadOptions);
            _ = imageElementComponent.UpdateGameObject();

            return imageElementComponent.gameObject;
        }
    }
}
