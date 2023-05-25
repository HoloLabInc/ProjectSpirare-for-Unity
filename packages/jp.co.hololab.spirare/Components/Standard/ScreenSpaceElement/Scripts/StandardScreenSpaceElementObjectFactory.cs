using UnityEngine;

namespace HoloLab.Spirare
{
    public class StandardScreenSpaceElementObjectFactory : ScreenSpaceElementObjectFactory
    {
        [SerializeField]
        private ScreenSpaceElementComponent screenSpaceElementComponentPrefab;

        public override GameObject CreateObject(PomlScreenSpaceElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            var screenSpaceElementComponent = Instantiate(screenSpaceElementComponentPrefab, parentTransform);
            screenSpaceElementComponent.name = "screen-space";
            screenSpaceElementComponent.Initialize(element, loadOptions);

            return screenSpaceElementComponent.gameObject;
        }
    }
}
