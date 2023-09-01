using System.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;

namespace HoloLab.Spirare
{
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

        public Dictionary<string, GltfImport> gltfImportCacheDictionary = new Dictionary<string, GltfImport>();

        public async Task LoadAsync(GameObject go, string src, Material material = null, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            // Search cache
            if (gltfImportCacheDictionary.TryGetValue(src, out var gltfImportCache))
            {
                // Instantiation
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);
                var instantiationResult2 = await gltfImportCache.InstantiateMainSceneAsync(go.transform, CancellationToken.None);
                if (instantiationResult2)
                {
                    InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);
                }
                else
                {
                    InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
                }
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
            gltfImportCacheDictionary[src] = gltfImport;

            // Model instantiating
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
