using System;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class SwitchingModelElementObjectFactory : ModelElementObjectFactory
    {
        [Serializable]
        public class FactoryPair
        {
            public string extension;
            public ModelElementObjectFactory modelElementObjectFactory;
        }

        [SerializeField]
        private ModelElementObjectFactory defaultModelElementObjectFactory;

        [SerializeField]
        private List<FactoryPair> factories;

        public override GameObject CreateObject(PomlModelElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
        {
            var extension = element.GetSrcFileExtension();
            if (string.IsNullOrEmpty(extension))
            {
                return defaultModelElementObjectFactory.CreateObject(element, loadOptions, parentTransform);
            }

            foreach (var factoryPair in factories)
            {
                if (factoryPair.extension == extension)
                {
                    return factoryPair.modelElementObjectFactory.CreateObject(element, loadOptions, parentTransform);
                }
            }

            Debug.LogWarning($"{extension} not supported");

            var go = new GameObject("model");
            go.transform.SetParent(parentTransform, false);
            return go;
        }
    }
}
