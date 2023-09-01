using System.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace HoloLab.Spirare
{
    internal class CacheManager<T>
    {
        private readonly ConcurrentDictionary<string, T> cacheDictionary
            = new ConcurrentDictionary<string, T>();

        private readonly ConcurrentDictionary<string, UniTaskCompletionSource<T>> cacheCompletionSourceDictionary
            = new ConcurrentDictionary<string, UniTaskCompletionSource<T>>();

        public async UniTask<(bool Success, T Value)> GetValueAsync(string key)
        {
            if (cacheDictionary.TryGetValue(key, out var value))
            {
                return (true, value);
            };

            if (cacheCompletionSourceDictionary.TryGetValue(key, out var completionSource))
            {
                try
                {
                    var result = await completionSource.Task;
                    return (true, result);
                }
                catch (Exception) { }
            }

            return (false, default);
        }

        public bool GenerateCreationTask(string key)
        {
            var taskCompletionSource = new UniTaskCompletionSource<T>();

            try
            {
                if (cacheCompletionSourceDictionary.TryAdd(key, taskCompletionSource))
                {
                    return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        public void CompleteCreationTask(string key, T value)
        {
            if (cacheCompletionSourceDictionary.TryGetValue(key, out var completionSource))
            {
                completionSource.TrySetResult(value);
                cacheCompletionSourceDictionary.TryRemove(key, out _);
            }
        }

        public void CancelCreationTask(string key)
        {
            if (cacheCompletionSourceDictionary.TryGetValue(key, out var completionSource))
            {
                completionSource.TrySetCanceled();
                cacheCompletionSourceDictionary.TryRemove(key, out _);
            }
        }

        public void ClearCache()
        {
            cacheDictionary.Clear();

            foreach (var completionPair in cacheCompletionSourceDictionary)
            {
                completionPair.Value.TrySetCanceled();
            }
            cacheCompletionSourceDictionary.Clear();
        }
    }

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

            if (creationTaskGenerated)
            {
                if (loadResult.Success)
                {
                    gltfImportCacheManager.CompleteCreationTask(src, material, loadResult.gltfImport);
                }
                else
                {
                    gltfImportCacheManager.CancelCreationTask(src, material);
                }
            }

            // Model instantiating
            if (loadResult.Success)
            {
                await InstantiateModel(go, loadResult.gltfImport, onLoadingStatusChanged);
            }
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
