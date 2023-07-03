using System.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;
using System.Threading;
using System;

namespace HoloLab.Spirare
{
    internal static class GltfastGlbLoader
    {
        public enum LoadingStatus
        {
            None,
            DataFetching,
            DataLoading,
            ModelInstantiating,
            Loaded,
            DataFetchError,
            DataLoadError,
            ModelInstantiateError
        }

        public static async Task LoadAsync(GameObject go, string src, Material material = null, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
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
            InvokeLoadingStatusChanged(LoadingStatus.DataLoading, onLoadingStatusChanged);

            IMaterialGenerator materialGenerator = null;
            if (material != null)
            {
                materialGenerator = new OcclusionMaterialGenerator(material);
            }

            var gltfImport = new GltfImport(materialGenerator: materialGenerator);
            var loadResult = await gltfImport.LoadGltfBinary(result.Data);
            if (loadResult == false)
            {
                InvokeLoadingStatusChanged(LoadingStatus.DataLoadError, onLoadingStatusChanged);
                return;
            }

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
