using SplatVfx;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace HoloLab.Spirare.Components.SplatVfx
{
    internal class SplatVfxPlyLoader : SplatLoaderBase
    {
        private readonly SplatVfxPlyParser parser = new SplatVfxPlyParser();

        public enum LoadErrorType
        {
            None,
            UnknownError,
            DataFetchError,
            InvalidHeader,
            InvalidBody,
            Cancelled
        }

        public async Task<(LoadErrorType Error, GameObject SplatObject)> LoadAsync(Transform parent, string src, VisualEffect splatPrefab,
            Action<LoadingStatus> onLoadingStatusChanged = null,
            CancellationToken cancellationToken = default)
        {
            // Data fetching
            var fetchResult = await FetchData(src, onLoadingStatusChanged);

            if (fetchResult.Success == false)
            {
                return (LoadErrorType.DataFetchError, null);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return (LoadErrorType.Cancelled, null);
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
    }
}

