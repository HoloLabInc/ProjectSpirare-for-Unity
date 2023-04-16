using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class StandardGeometryElementObjectFactory : GeometryElementObjectFactory
    {
        [SerializeField]
        private GeometryElementComponent geometryElementComponentPrefab;

        public override GameObject CreateObject(
            PomlGeometryElement element,
            GeoReferenceElementComponentFactory geoReferenceElementComponentFactory,
            PomlLoadOptions loadOptions,
            Transform parentTransform = null)
        {
            var geometryElementComponent = Instantiate(geometryElementComponentPrefab, parentTransform);
            geometryElementComponent.name = "geometry";

            geometryElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            geometryElementComponent.Initialize(element, geoReferenceElementComponentFactory, loadOptions);
            _ = geometryElementComponent.UpdateGameObject();

            return geometryElementComponent.gameObject;
        }
    }
}
