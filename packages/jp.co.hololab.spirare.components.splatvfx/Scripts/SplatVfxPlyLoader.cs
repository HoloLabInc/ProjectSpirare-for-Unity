using Cysharp.Threading.Tasks;
using SplatVfx;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace HoloLab.Spirare.Components.SplatVfx
{
    internal class SplatVfxPlyLoader
    {
        private readonly SplatVfxPlyParser parser = new SplatVfxPlyParser();

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

        public enum LoadErrorType
        {
            None,
            UnknownError,
            DataFetchError,
            InvalidHeader,
            InvalidBody
        }

        public async Task<(LoadErrorType Error, GameObject SplatObject)> LoadAsync(Transform parent, string src, VisualEffect splatPrefab, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            // Data fetching
            var fetchResult = await FetchData(src, onLoadingStatusChanged);

            if (fetchResult.Success == false)
            {
                return (LoadErrorType.DataFetchError, null);
            }

            var parseResult = parser.TryParseSplatData(fetchResult.Data);

            switch (parseResult.Error)
            {
                case SplatVfxPlyParser.ParseErrorType.None:

                    var data = CreateSplatData(parseResult.SplatData);
                    InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);
                    var splatObject = SplatVfxUtil.InstantiateSplatVfx(splatPrefab, data, parent);
                    InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);

                    return (LoadErrorType.None, splatObject);

                case SplatVfxPlyParser.ParseErrorType.InvalidHeader:
                    return (LoadErrorType.InvalidHeader, null);

                case SplatVfxPlyParser.ParseErrorType.InvalidBody:
                    return (LoadErrorType.InvalidBody, null);

                default:
                    return (LoadErrorType.UnknownError, null);
            }
        }

        private static SplatData CreateSplatData(SplatVfxPlyParser.SplatData data)
        {
            var splatData = ScriptableObject.CreateInstance<SplatData>();

            splatData.PositionArray = data.Positions;
            splatData.AxisArray = data.Axes;
            splatData.ColorArray = data.Colors;

            return splatData;
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
