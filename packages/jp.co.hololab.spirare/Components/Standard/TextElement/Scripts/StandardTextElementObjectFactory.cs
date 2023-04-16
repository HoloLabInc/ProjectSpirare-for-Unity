using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace HoloLab.Spirare
{
    public class StandardTextElementObjectFactory : TextElementObjectFactory
    {
        [SerializeField]
        private TextElementComponent builtInTextElementComponentPrefab;

        [SerializeField]
        private TextElementComponent urpTextElementComponentPrefab;

        public override GameObject CreateObject(PomlTextElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            var renderPipeline = GraphicsSettings.currentRenderPipeline;

            TextElementComponent textElementComponentPrefab;
            if (renderPipeline == null)
            {
                textElementComponentPrefab = builtInTextElementComponentPrefab;
            }
            else
            {
                textElementComponentPrefab = urpTextElementComponentPrefab;
            }

            var textElementComponent = Instantiate(textElementComponentPrefab, parentTransform);
            textElementComponent.gameObject.name = "text";

            textElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            textElementComponent.Initialize(element, loadOptions);
            _ = textElementComponent.UpdateGameObject();

            return textElementComponent.gameObject;
        }
    }
}
