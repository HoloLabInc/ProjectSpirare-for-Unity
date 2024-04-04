using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

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

        public async Task<(bool Success, GameObject SplatObject)> LoadAsync(Transform parent, string src, PointCloudPlyComponent pointCloudPrefab, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            if (pointCloudPrefab == null)
            {
                return (false, null);
            }

            // Data fetching
            var fetchResult = await FetchData(src, onLoadingStatusChanged);

            if (fetchResult.Success == false)
            {
                return (false, null);
            }

            var plyComponent = UnityEngine.Object.Instantiate(pointCloudPrefab);
            plyComponent.transform.SetParent(parent, false);
            plyComponent.LoadPlyFromFile(fetchResult.Filepath);

            return (true, plyComponent.gameObject);
            /*
            var parseResult = parser.TryParseSplatData(fetchResult.Data);

            switch (parseResult.Error)
            {
                case SplatVfxPlyParser.ParseErrorType.None:

                    var data = CreateSplatData(parseResult.SplatData);

                    InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);
                    var visualEffect = UnityEngine.Object.Instantiate(splatPrefab);
                    var splatObject = visualEffect.gameObject;

                    var binderBase = splatObject.AddComponent<VFXPropertyBinder>();
                    var binder = binderBase.AddPropertyBinder<VFXSplatDataBinder>();
                    binder.SplatData = data;

                    splatObject.transform.SetParent(parent, worldPositionStays: false);

                    InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);

                    return (LoadErrorType.None, splatObject);

                case SplatVfxPlyParser.ParseErrorType.InvalidHeader:
                    return (LoadErrorType.InvalidHeader, null);

                case SplatVfxPlyParser.ParseErrorType.InvalidBody:
                    return (LoadErrorType.InvalidBody, null);

                default:
                    return (LoadErrorType.UnknownError, null);
            }
            */
        }

        private static async UniTask<(bool Success, string Filepath)> FetchData(string src, Action<LoadingStatus> onLoadingStatusChanged)
        {
            InvokeLoadingStatusChanged(LoadingStatus.DataFetching, onLoadingStatusChanged);

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

