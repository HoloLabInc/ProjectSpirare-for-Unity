using System.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace HoloLab.Spirare
{
    internal class GltfImportCacheManager
    {
        private readonly Dictionary<string, GltfImport> cacheDictionaryForDefaultMaterial
            = new Dictionary<string, GltfImport>();

        private readonly Dictionary<Material, Dictionary<string, GltfImport>> cacheDictionaryForCustomMaterial
            = new Dictionary<Material, Dictionary<string, GltfImport>>();

        public bool TryGetValue(string url, Material material, out GltfImport gltfImport)
        {
            if (material == null)
            {
                return cacheDictionaryForDefaultMaterial.TryGetValue(url, out gltfImport);
            }

            if (cacheDictionaryForCustomMaterial.TryGetValue(material, out var customMaterialDictionary))
            {
                return customMaterialDictionary.TryGetValue(url, out gltfImport);
            }

            gltfImport = null;
            return false;
        }

        public void AddValue(string url, Material material, GltfImport gltfImport)
        {
            if (material == null)
            {
                cacheDictionaryForDefaultMaterial[url] = gltfImport;
                return;
            }

            if (cacheDictionaryForCustomMaterial.TryGetValue(material, out var customMaterialDictionary) == false)
            {
                customMaterialDictionary = new Dictionary<string, GltfImport>();
                cacheDictionaryForCustomMaterial.Add(material, customMaterialDictionary);
            }

            customMaterialDictionary[url] = gltfImport;
        }

        public void ClearCache()
        {
            cacheDictionaryForDefaultMaterial.Clear();
            cacheDictionaryForCustomMaterial.Clear();
        }
    }

    internal class GltfastGlbLoader
    {
        public enum LoadingStatus
        {
            None,
            DataFetching,
            ModelLoading,
            ModelInstantiating,
            Loaded,
            DataFetchError,
            ModelLoadError,
            ModelInstantiateError
        }

        private readonly GltfImportCacheManager gltfImportCacheManager = new GltfImportCacheManager();

        public async Task LoadAsync(GameObject go, string src, Material material = null, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            // Search cache
            if (gltfImportCacheManager.TryGetValue(src, material, out var gltfImportCache))
            {
                await InstantiateModel(go, gltfImportCache, onLoadingStatusChanged);
                return;
            }

            // Data fetching
            InvokeLoadingStatusChanged(LoadingStatus.DataFetching, onLoadingStatusChanged);

            var result = await SpirareHttpClient.Instance.GetByteArrayAsync(src, enableCache: true);
            if (result.Success == false)
            {
                InvokeLoadingStatusChanged(LoadingStatus.DataFetchError, onLoadingStatusChanged);
                Debug.LogWarning($"Failed to get model data: {src}");
                return;
            }

            // Model loading
            InvokeLoadingStatusChanged(LoadingStatus.ModelLoading, onLoadingStatusChanged);

            IMaterialGenerator materialGenerator = null;
            if (material != null)
            {
                materialGenerator = new OcclusionMaterialGenerator(material);
            }

            var gltfImport = new GltfImport(materialGenerator: materialGenerator);
            var loadResult = await gltfImport.LoadGltfBinary(result.Data);
            if (loadResult == false)
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelLoadError, onLoadingStatusChanged);
                return;
            }

            // Save cache
            gltfImportCacheManager.AddValue(src, material, gltfImport);

            await InstantiateModel(go, gltfImport, onLoadingStatusChanged);
        }

        public void ClearGltfImportCache()
        {
            gltfImportCacheManager.ClearCache();
        }

        private static async UniTask InstantiateModel(GameObject go, GltfImport gltfImport, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            if (go == null)
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
                return;
            }

            InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);
            var instantiationResult = await gltfImport.InstantiateMainSceneAsync(go.transform, CancellationToken.None);
            if (instantiationResult)
            {
                InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);
            }
            else
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
            }
        }

        private static void InvokeLoadingStatusChanged(LoadingStatus status, Action<LoadingStatus> onLoadingStatusChanged)
        {
            try
            {
                onLoadingStatusChanged?.Invoke(status);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    internal class OcclusionMaterialGenerator : IMaterialGenerator
    {
        private ICodeLogger logger;
        private Material occlusionMaterial;

        public OcclusionMaterialGenerator(Material occlusionMaterial)
        {
            this.occlusionMaterial = occlusionMaterial;
        }

        public Material GenerateMaterial(GLTFast.Schema.Material gltfMaterial, IGltfReadable gltf, bool pointsSupport = false)
        {
            return occlusionMaterial;
        }

        public Material GetDefaultMaterial(bool pointsSupport = false)
        {
            return occlusionMaterial;
        }

        public void SetLogger(ICodeLogger logger)
        {
            this.logger = logger;
        }
    }
}
