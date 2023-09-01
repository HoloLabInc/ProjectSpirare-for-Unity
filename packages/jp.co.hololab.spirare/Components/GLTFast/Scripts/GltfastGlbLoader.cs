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

        public UniTaskCompletionSource<T> GenerateCreationTask(string key)
        {
            var taskCompletionSource = new UniTaskCompletionSource<T>();

            try
            {
                cacheCompletionSourceDictionary.TryAdd(key, taskCompletionSource);
                return taskCompletionSource;
            }
            catch (Exception) { }

            return null;
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
        /*
        private readonly Dictionary<string, GltfImport> cacheDictionaryForDefaultMaterial
            = new Dictionary<string, GltfImport>();
        */

        private readonly Dictionary<Material, Dictionary<string, GltfImport>> cacheDictionaryForCustomMaterial
            = new Dictionary<Material, Dictionary<string, GltfImport>>();

        /*
        private readonly Dictionary<string, UniTask<GltfImport>> creationTaskDictionaryForDefaultMaterial
            = new Dictionary<string, UniTask<GltfImport>>();
        */

        private readonly Dictionary<Material, Dictionary<string, UniTask<GltfImport>>> creationTaskDictionaryForCustomMaterial
            = new Dictionary<Material, Dictionary<string, UniTask<GltfImport>>>();

        public async UniTask<(bool Success, GltfImport GltfImport)> GetGltfImportAsync(string url, Material material)
        {
            // Find cache
            if (material == null)
            {
                /*
                if (cacheDictionaryForDefaultMaterial.TryGetValue(url, out var gltfImport))
                {
                    return (true, gltfImport);
                };
                */
                return await cacheManagerForDefaultMaterial.GetValueAsync(url);
            }
            else if (cacheDictionaryForCustomMaterial.TryGetValue(material, out var customMaterialDictionary))
            {
                if (customMaterialDictionary.TryGetValue(url, out var gltfImport))
                {
                    return (true, gltfImport);
                }
            }

            // Find GltfImport creation task
            if (material == null)
            {
                /*
                if (creationTaskDictionaryForDefaultMaterial.TryGetValue(url, out var creationTask))
                {
                    var gltfImport = await creationTask;
                    if (gltfImport != null)
                    {
                        return (true, gltfImport);
                    }
                }
                */
            }
            else if (creationTaskDictionaryForCustomMaterial.TryGetValue(material, out var customMaterialDictionary))
            {
                if (customMaterialDictionary.TryGetValue(url, out var creationTask))
                {
                    var gltfImport = await creationTask;
                    if (gltfImport != null)
                    {
                        return (true, gltfImport);
                    }
                }
            }

            return (false, null);
        }

        /*
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
        */

        public UniTaskCompletionSource<GltfImport> GenerateCreationTask(string url, Material material)
        {
            if (material == null)
            {
                return cacheManagerForDefaultMaterial.GenerateCreationTask(url);
            }
            return null;
        }

        public void CompleteCreationTask(string url, Material material, GltfImport gltfImport)
        {
            if (material == null)
            {
                cacheManagerForDefaultMaterial.CompleteCreationTask(url, gltfImport);
                return;
            }
        }

        public void CancelCreationTask(string url, Material material)
        {
            if (material == null)
            {
                cacheManagerForDefaultMaterial.CancelCreationTask(url);
                return;
            }
        }

        /*
        public UniTaskCompletionSource<GltfImport> SetCreationTask(string url, Material material)
        {
            var downloadTaskSource = new UniTaskCompletionSource<GltfImport>();

            if (material == null)
            {
                try
                {
                    creationTaskDictionaryForDefaultMaterial.Add(url, downloadTaskSource.Task);
                    return downloadTaskSource;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                // TODO
            }
            return null;
        }
        */

        public void ClearCache()
        {
            cacheManagerForDefaultMaterial.ClearCache();
            //cacheDictionaryForDefaultMaterial.Clear();
            //cacheDictionaryForCustomMaterial.Clear();
            // TODO clear task
        }

        /*
        public void CompleteCreationTask(string url, Material material, UniTaskCompletionSource<GltfImport> creationTaskSource, GltfImport gltfImport)
        {
            creationTaskSource?.TrySetResult(gltfImport);

            if (material == null)
            {
                creationTaskDictionaryForDefaultMaterial.Remove(url);
            }
            else
            {
                // TODO

            }
        }
        */
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

            var creationTaskSource = gltfImportCacheManager.GenerateCreationTask(src, material);

            // Data fetching
            InvokeLoadingStatusChanged(LoadingStatus.DataFetching, onLoadingStatusChanged);

            var result = await SpirareHttpClient.Instance.GetByteArrayAsync(src, enableCache: true);
            if (result.Success == false)
            {
                //gltfImportCacheManager.CompleteCreationTask(src, material, creationTaskSource, null);
                gltfImportCacheManager.CancelCreationTask(src, material);
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
                //gltfImportCacheManager.CompleteCreationTask(src, material, creationTaskSource, null);
                gltfImportCacheManager.CancelCreationTask(src, material);
                InvokeLoadingStatusChanged(LoadingStatus.ModelLoadError, onLoadingStatusChanged);
                return;
            }

            // Save cache
            gltfImportCacheManager.CompleteCreationTask(src, material, gltfImport);
            // gltfImportCacheManager.AddValue(src, material, gltfImport);

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
