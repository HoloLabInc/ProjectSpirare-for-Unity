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
            var pipeline = RenderPipelineUtility.GetRenderPipelineType();
            var unlitMaterialType = pipeline switch
            {
                RenderPipelineType.BuiltIn => UnlitMaterialType.Unlit_Color,
                RenderPipelineType.URP => UnlitMaterialType.UniversalRenderPipeline_Unlit,
                _ => UnlitMaterialType.Unlit_Color,
            };

            var imageElementComponent = Instantiate(imageElementComponentPrefab, parentTransform);
            imageElementComponent.name = "image";

            imageElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            imageElementComponent.Initialize(element, loadOptions);
            imageElementComponent.UnlitMaterialType = unlitMaterialType;
            _ = imageElementComponent.UpdateGameObject();

            return imageElementComponent.gameObject;
        }
    }
}
