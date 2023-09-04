using System.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;

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

        private readonly GltfImportCacheManager gltfImportCacheManager = new GltfImportCacheManager();

        public async Task LoadAsync(GameObject go, string src, Material material = null, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            // Search cache
            var cacheResult = await gltfImportCacheManager.GetGltfImportAsync(src, material);
            if (cacheResult.Success)
            {
                await InstantiateModel(go, cacheResult.GltfImport, onLoadingStatusChanged);
                return;
            }

            var creationTaskGenerated = gltfImportCacheManager.GenerateCreationTask(src, material);

            // Data fetching
            var fetchResult = await FetchData(src, onLoadingStatusChanged);

            if (fetchResult.Success == false)
            {
                if (creationTaskGenerated)
                {
                    gltfImportCacheManager.CancelCreationTask(src, material);
                }
                return;
            }

            // Model loading
            var loadResult = await LoadModel(fetchResult.Data, material, onLoadingStatusChanged);

            if (loadResult.Success == false)
            {
                if (creationTaskGenerated)
                {
                    gltfImportCacheManager.CancelCreationTask(src, material);
                }
                return;
            }

            var gltfImport = loadResult.gltfImport;

            if (creationTaskGenerated)
            {
                gltfImportCacheManager.CompleteCreationTask(src, material, gltfImport);
            }

            // Model instantiating
            await InstantiateModel(go, gltfImport, onLoadingStatusChanged);
        }

        public void ClearGltfImportCache()
        {
            gltfImportCacheManager.ClearCache();
        }

        private static async UniTask<(bool Success, byte[] Data)> FetchData(string src, Action<LoadingStatus> onLoadingStatusChanged)
        {
            InvokeLoadingStatusChanged(LoadingStatus.DataFetching, onLoadingStatusChanged);

            var result = await SpirareHttpClient.Instance.GetByteArrayAsync(src, enableCache: true);
            if (result.Success)
            {
                return (true, result.Data);
            }
            else
            {
                InvokeLoadingStatusChanged(LoadingStatus.DataFetchError, onLoadingStatusChanged);
                Debug.LogWarning($"Failed to get model data: {src}");

                return (false, null);
            }
        }

        private static async UniTask<(bool Success, GltfImport gltfImport)> LoadModel(byte[] data, Material material, Action<LoadingStatus> onLoadingStatusChanged)
        {
            InvokeLoadingStatusChanged(LoadingStatus.ModelLoading, onLoadingStatusChanged);

            IMaterialGenerator materialGenerator = null;
            if (material != null)
            {
                materialGenerator = new OcclusionMaterialGenerator(material);
            }

            var gltfImport = new GltfImport(materialGenerator: materialGenerator);
            var loadResult = await gltfImport.LoadGltfBinary(data);
            if (loadResult == false)
            {
                gltfImport.Dispose();
                gltfImport = null;

                InvokeLoadingStatusChanged(LoadingStatus.ModelLoadError, onLoadingStatusChanged);
            }

            return (loadResult, gltfImport);
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
