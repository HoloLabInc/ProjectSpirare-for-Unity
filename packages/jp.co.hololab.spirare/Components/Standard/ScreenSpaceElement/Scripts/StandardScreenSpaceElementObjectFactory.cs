using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    [CreateAssetMenu(menuName = "Spirare/StandardScreenSpaceElementObjectFactory")]
    public class StandardScreenSpaceElementObjectFactory : ScreenSpaceElementObjectFactory
    {
        [SerializeField]
        private ScreenSpaceElementComponent screenSpaceElementComponentPrefab;

        public override GameObject CreateObject(PomlScreenSpaceElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            var screenSpaceElementComponent = Instantiate(screenSpaceElementComponentPrefab, parentTransform);
            screenSpaceElementComponent.name = "screen-space";

            // screenSpaceElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            // screenSpaceElementComponent.Initialize(element, loadOptions);
            // _ = screenSpaceElementComponent.UpdateGameObject();

            return screenSpaceElementComponent.gameObject;
        }
    }
}
