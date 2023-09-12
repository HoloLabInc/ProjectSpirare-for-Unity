using UnityEngine;
using System.Collections.Generic;

namespace HoloLab.Spirare
{
    internal class GltfastGlbInstanceReference
    {
        private readonly SpecificMaterialInstanceReference defaultMaterialReference
             = new SpecificMaterialInstanceReference();

        private readonly Dictionary<Material, SpecificMaterialInstanceReference> referenceDictionary
             = new Dictionary<Material, SpecificMaterialInstanceReference>();

        public void AddInstance(string src, Material material, GameObject instance)
        {
            if (material == null)
            {
                defaultMaterialReference.AddInstance(src, instance);
            }
            else
            {
                if (referenceDictionary.TryGetValue(material, out var oneMaterialReference) == false)
                {
                    oneMaterialReference = new SpecificMaterialInstanceReference();
                    referenceDictionary.Add(material, oneMaterialReference);
                }

                oneMaterialReference.AddInstance(src, instance);
            }
        }

        public void RemoveInstance(string src, Material material, GameObject instance)
        {
            if (material == null)
            {
                defaultMaterialReference.RemoveInstance(src, instance);
            }
            else
            {
                if (referenceDictionary.TryGetValue(material, out var referece))
                {
                    referece.RemoveInstance(src, instance);
                }
            }
        }

        public int GetInstanceCount(string src, Material material)
        {
            if (material == null)
            {
                return defaultMaterialReference.GetInstanceCount(src);
            }
            else if (referenceDictionary.TryGetValue(material, out var referece))
            {
                return referece.GetInstanceCount(src);
            }
            else
            {
                return 0;
            }
        }

        private class SpecificMaterialInstanceReference
        {
            private readonly Dictionary<string, HashSet<GameObject>> instanceSetDictionary
                = new Dictionary<string, HashSet<GameObject>>();

            public void AddInstance(string src, GameObject instance)
            {
                if (instanceSetDictionary.TryGetValue(src, out var instanceSet) == false)
                {
                    instanceSet = new HashSet<GameObject>();
                    instanceSetDictionary[src] = instanceSet;
                }

                instanceSet.Add(instance);
            }

            public void RemoveInstance(string src, GameObject instance)
            {
                if (instanceSetDictionary.TryGetValue(src, out var instanceSet))
                {
                    instanceSet.Remove(instance);
                }
            }

            public int GetInstanceCount(string src)
            {
                if (instanceSetDictionary.TryGetValue(src, out var instanceSet) == false)
                {
                    return 0;
                }
                return instanceSet.Count;
            }
        }
    }
}
