using GLTFast;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace HoloLab.Spirare
{
    internal class GltfImportCacheManager
    {
        private readonly CacheManager<GltfImport> cacheManagerForDefaultMaterial = new CacheManager<GltfImport>();

        private readonly Dictionary<Material, CacheManager<GltfImport>> cacheManagerDictionaryForCustomMaterials
            = new Dictionary<Material, CacheManager<GltfImport>>();

        public async UniTask<(bool Success, GltfImport GltfImport)> GetGltfImportAsync(string url, Material material)
        {
            if (material == null)
            {
                return await cacheManagerForDefaultMaterial.GetValueAsync(url);
            }
            else if (cacheManagerDictionaryForCustomMaterials.TryGetValue(material, out var cacheManager))
            {
                return await cacheManager.GetValueAsync(url);
            }
            return (false, null);
        }

        public bool GenerateCreationTask(string url, Material material)
        {
            if (material == null)
            {
                return cacheManagerForDefaultMaterial.GenerateCreationTask(url);
            }

            if (cacheManagerDictionaryForCustomMaterials.TryGetValue(material, out var cacheManager) == false)
            {
                cacheManager = new CacheManager<GltfImport>();
                cacheManagerDictionaryForCustomMaterials[material] = cacheManager;
            }
            return cacheManager.GenerateCreationTask(url);
        }

        public void CompleteCreationTask(string url, Material material, GltfImport gltfImport)
        {
            if (material == null)
            {
                cacheManagerForDefaultMaterial.CompleteCreationTask(url, gltfImport);
            }
            else if (cacheManagerDictionaryForCustomMaterials.TryGetValue(material, out var cacheManager))
            {
                cacheManager.CompleteCreationTask(url, gltfImport);
            }
        }

        public void CancelCreationTask(string url, Material material)
        {
            if (material == null)
            {
                cacheManagerForDefaultMaterial.CancelCreationTask(url);
            }
            else if (cacheManagerDictionaryForCustomMaterials.TryGetValue(material, out var cacheManager))
            {
                cacheManager.CancelCreationTask(url);
            }
        }

        public void RemoveCache(string url, Material material)
        {
            if (material == null)
            {
                cacheManagerForDefaultMaterial.RemoveCache(url);
            }
            else if (cacheManagerDictionaryForCustomMaterials.TryGetValue(material, out var cacheManager))
            {
                cacheManager.RemoveCache(url);
            }
        }

        public void ClearCache()
        {
            cacheManagerForDefaultMaterial.ClearCache();

            foreach (var cacheManagerPair in cacheManagerDictionaryForCustomMaterials)
            {
                cacheManagerPair.Value.ClearCache();
            }
            cacheManagerDictionaryForCustomMaterials.Clear();
        }
    }
}
