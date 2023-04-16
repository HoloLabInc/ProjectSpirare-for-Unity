using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class StandardVideoElementObjectFactory : VideoElementObjectFactory
    {
        [SerializeField]
        private VideoElementComponent videoElementComponentPrefab;

        public override GameObject CreateObject(PomlVideoElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            var videoElementComponent = Instantiate(videoElementComponentPrefab, parentTransform);
            videoElementComponent.name = "video";

            videoElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            videoElementComponent.Initialize(element, loadOptions);
            _ = videoElementComponent.UpdateGameObject();

            return videoElementComponent.gameObject;
        }
    }
}
