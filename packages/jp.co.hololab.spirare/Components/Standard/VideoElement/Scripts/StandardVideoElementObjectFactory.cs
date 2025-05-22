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
            var pipeline = RenderPipelineUtility.GetRenderPipelineType();
            var unlitMaterialType = pipeline switch
            {
                RenderPipelineType.BuiltIn => UnlitMaterialType.Unlit_Color,
                RenderPipelineType.URP => UnlitMaterialType.UniversalRenderPipeline_Unlit,
                _ => UnlitMaterialType.Unlit_Color,
            };

            var videoElementComponent = Instantiate(videoElementComponentPrefab, parentTransform);
            videoElementComponent.name = "video";

            videoElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            videoElementComponent.Initialize(element, loadOptions);
            videoElementComponent.UnlitMaterialType = unlitMaterialType;
            _ = videoElementComponent.UpdateGameObject();

            return videoElementComponent.gameObject;
        }
    }
}
