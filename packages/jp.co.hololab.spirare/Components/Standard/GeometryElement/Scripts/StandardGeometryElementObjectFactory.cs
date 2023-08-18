using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace HoloLab.Spirare
{
    public class StandardGeometryElementObjectFactory : GeometryElementObjectFactory
    {
        [SerializeField]
        private GeometryElementComponent geometryElementComponentPrefab;

        [SerializeField]
        private Material builtInLineMaterial;

        [SerializeField]
        private Material builtInPolygonMaterial;

        [SerializeField]
        private Material urpLineMaterial;

        [SerializeField]
        private Material urpPolygonMaterial;

        public override GameObject CreateObject(
            PomlGeometryElement element,
            GeoReferenceElementComponentFactory geoReferenceElementComponentFactory,
            PomlLoadOptions loadOptions,
            Transform parentTransform = null)
        {
            Material lineMaterial;
            Material polygonMaterial;

            var renderPipeline = GraphicsSettings.currentRenderPipeline;
            if (renderPipeline == null)
            {
                // Built-in pipeline
                lineMaterial = builtInLineMaterial;
                polygonMaterial = builtInPolygonMaterial;
            }
            else
            {
                lineMaterial = urpLineMaterial;
                polygonMaterial = urpPolygonMaterial;
            }

            var geometryElementComponent = Instantiate(geometryElementComponentPrefab, parentTransform);
            geometryElementComponent.name = "geometry";

            geometryElementComponent.GetComponent<PomlObjectElementComponent>().Initialize(element);
            geometryElementComponent.Initialize(element, geoReferenceElementComponentFactory, loadOptions, lineMaterial, polygonMaterial);
            _ = geometryElementComponent.UpdateGameObject();

            return geometryElementComponent.gameObject;
        }
    }
}
