using System.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace HoloLab.Spirare
{
    internal class GltfastGlbLoaderInstanceReference
    {
        private readonly Dictionary<string, HashSet<GameObject>> defaultMaterialInstances
            = new Dictionary<string, HashSet<GameObject>>();

        public void AddInstance(string src, Material material, GameObject instance)
        {
            if (material == null)
            {
                if (defaultMaterialInstances.TryGetValue(src, out var instances) == false)
                {
                    instances = new HashSet<GameObject>();
                    defaultMaterialInstances[src] = instances;
                }

                instances.Add(instance);
            }
        }

        public void RemoveInstance(string src, Material material, GameObject instance)
        {
            if (material == null)
            {
                if (defaultMaterialInstances.TryGetValue(src, out var instances))
                {
                    instances.Remove(instance);
                }
            }
        }

        public int GetInstanceCount(string src, Material material)
        {
            if (material == null)
            {
                if (defaultMaterialInstances.TryGetValue(src, out var instances) == false)
                {
                    return 0;
                }
                return instances.Count;
            }

            return 0;
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
        private readonly GltfastGlbLoaderInstanceReference instanceReference = new GltfastGlbLoaderInstanceReference();

        public async Task<(bool Success, GameObject glbObject)> LoadAsync(Transform parent, string src, Material material = null, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            var glbObject = new GameObject("Glb Instance");
            var glbInstance = glbObject.AddComponent<GltfastGlbInstance>();
            glbInstance.Initialize(this, src, material);
            instanceReference.AddInstance(src, material, glbObject);

            // Search cache
            var cacheResult = await gltfImportCacheManager.GetGltfImportAsync(src, material);
            if (cacheResult.Success)
            {
                return await InstantiateModel(glbInstance, parent, cacheResult.GltfImport, onLoadingStatusChanged);
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
                UnityEngine.Object.Destroy(glbObject);
                return (false, null);
            }

            // Model loading
            var loadResult = await LoadModel(fetchResult.Data, material, onLoadingStatusChanged);

            if (loadResult.Success == false)
            {
                if (creationTaskGenerated)
                {
                    gltfImportCacheManager.CancelCreationTask(src, material);
                }
                UnityEngine.Object.Destroy(glbObject);
                return (false, null);
            }

            var gltfImport = loadResult.gltfImport;

            if (creationTaskGenerated)
            {
                gltfImportCacheManager.CompleteCreationTask(src, material, gltfImport);
            }

            // Model instantiating
            return await InstantiateModel(glbInstance, parent, gltfImport, onLoadingStatusChanged);
        }

        internal void RemoveInstanceReference(GltfastGlbInstance glbInstance)
        {
            var src = glbInstance.Src;
            var material = glbInstance.Material;

            instanceReference.RemoveInstance(src, material, glbInstance.gameObject);
            var referenceCount = instanceReference.GetInstanceCount(src, material);

            if (referenceCount == 0)
            {
                // Dispose GltfImport and remove cache
                var gltfImport = glbInstance.GltfImport;
                gltfImportCacheManager.RemoveCache(src, material);
                gltfImport.Dispose();
            }
        }

        internal void ClearGltfImportCache()
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

        private static async UniTask<(bool Success, GameObject gltfObject)> InstantiateModel(GltfastGlbInstance glbInstance, Transform parent, GltfImport gltfImport, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            if (glbInstance == null)
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
                return (false, null);
            }

            glbInstance.transform.SetParent(parent, false);
            glbInstance.SetGltfImport(gltfImport);

            if (parent == null)
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
                UnityEngine.Object.Destroy(glbInstance.gameObject);
                return (false, null);
            }

            InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);
            var instantiationResult = await gltfImport.InstantiateMainSceneAsync(glbInstance.transform, CancellationToken.None);
            if (instantiationResult)
            {
                InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);
                return (true, glbInstance.gameObject);
            }
            else
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
                UnityEngine.Object.Destroy(glbInstance.gameObject);
                return (false, null);
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
