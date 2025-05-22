using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace HoloLab.Spirare
{
    public class StandardTextElementObjectFactory : TextElementObjectFactory
    {
        [SerializeField]
        [FormerlySerializedAs("builtInTextElementComponentPrefab")]
        private TextElementComponent textElementComponentPrefab;

        public override GameObject CreateObject(PomlTextElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            var pipeline = RenderPipelineUtility.GetRenderPipelineType();
            var unlitMaterialType = pipeline switch
            {
                RenderPipelineType.BuiltIn => UnlitMaterialType.Unlit_Color,
                RenderPipelineType.URP => UnlitMaterialType.UniversalRenderPipeline_Unlit,
                _ => UnlitMaterialType.Unlit_Color,
            };

            var textElementComponent = Instantiate(textElementComponentPrefab, parentTransform);
            textElementComponent.gameObject.name = "text";

            textElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            textElementComponent.Initialize(element, loadOptions);
            textElementComponent.UnlitMaterialType = unlitMaterialType;
            _ = textElementComponent.UpdateGameObject();

            return textElementComponent.gameObject;
        }
    }
}
