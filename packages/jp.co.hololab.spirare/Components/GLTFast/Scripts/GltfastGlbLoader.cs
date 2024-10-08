﻿using System.Threading.Tasks;
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
        private readonly GltfastGlbInstanceReference instanceReference = new GltfastGlbInstanceReference();

        public async Task<(bool Success, GameObject GlbObject)> LoadAsync(Transform parent, string src, Material material = null, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            // Create GameObject
            var glbInstance = CreateGlbInstance(this, src, material);
            instanceReference.AddInstance(src, material, glbInstance.gameObject);

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
                UnityEngine.Object.Destroy(glbInstance.gameObject);
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
                UnityEngine.Object.Destroy(glbInstance.gameObject);
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
                gltfImportCacheManager.RemoveCache(src, material);
                glbInstance.GltfImport?.Dispose();
            }
        }

        internal void ClearGltfImportCache()
        {
            gltfImportCacheManager.ClearAll();
        }

        private static GltfastGlbInstance CreateGlbInstance(GltfastGlbLoader glbLoader, string src, Material material)
        {
            var glbObject = new GameObject("Glb Instance");
            glbObject.hideFlags = HideFlags.HideInHierarchy;

            var glbInstance = glbObject.AddComponent<GltfastGlbInstance>();
            glbInstance.Initialize(glbLoader, src, material);

            return glbInstance;
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

            try
            {
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
            catch (Exception e)
            {
                Debug.LogException(e);
                return (false, null);
            }
        }

        private static async UniTask<(bool Success, GameObject GltfObject)> InstantiateModel(GltfastGlbInstance glbInstance, Transform parent, GltfImport gltfImport, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            if (glbInstance == null)
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
                return (false, null);
            }

            glbInstance.transform.SetParent(parent, false);
            glbInstance.gameObject.hideFlags = HideFlags.None;
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

#if GLTFAST_6_0_0_OR_NEWER
        public Material GenerateMaterial(GLTFast.Schema.MaterialBase gltfMaterial, IGltfReadable gltf, bool pointsSupport = false)
        {
            return new Material(occlusionMaterial);
        }
#else
        public Material GenerateMaterial(GLTFast.Schema.Material gltfMaterial, IGltfReadable gltf, bool pointsSupport = false)
        {
            return new Material(occlusionMaterial);
        }
#endif

        public Material GetDefaultMaterial(bool pointsSupport = false)
        {
            return new Material(occlusionMaterial);
        }

        public void SetLogger(ICodeLogger logger)
        {
            this.logger = logger;
        }
    }
}
