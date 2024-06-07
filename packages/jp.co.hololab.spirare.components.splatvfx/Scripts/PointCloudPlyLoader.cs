using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Components.SplatVfx
{
    internal class PointCloudPlyLoader
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

        public async Task<(bool Success, GameObject SplatObject)> LoadAsync(Transform parent, string src, PointCloudPlyComponent pointCloudPrefab,
            Action<LoadingStatus> onLoadingStatusChanged = null,
            CancellationToken cancellationToken = default)
        {
            if (pointCloudPrefab == null)
            {
                return (false, null);
            }

            // Data fetching
            var fetchResult = await FetchData(src, onLoadingStatusChanged);

            if (fetchResult.Success == false || cancellationToken.IsCancellationRequested)
            {
                return (false, null);
            }

            InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);

            var plyComponent = UnityEngine.Object.Instantiate(pointCloudPrefab);
            plyComponent.transform.SetParent(parent, false);
            plyComponent.LoadPlyFromFile(fetchResult.Filepath);

            InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);

            return (true, plyComponent.gameObject);
        }

        private static async UniTask<(bool Success, string Filepath)> FetchData(string src, Action<LoadingStatus> onLoadingStatusChanged)
        {
            InvokeLoadingStatusChanged(LoadingStatus.DataFetching, onLoadingStatusChanged);

            if (src.StartsWith("file://"))
            {
                var filepath = SpirareHttpClient.ConvertFileScemeUrlToFilePath(src);
                return (true, filepath);
            }

            var result = await SpirareHttpClient.Instance.DownloadToFileAsync(src, enableCache: true);
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
}

