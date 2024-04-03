using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using SplatVfx;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.VFX.Utility;
using UnityEngine.VFX;

namespace HoloLab.Spirare.Components.SplatVfx
{
    internal class SplatVfxPlyLoader
    {
        private SplatVfxPlyParser parser = new SplatVfxPlyParser();

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

        public async Task<(bool Success, GameObject SplatObject)> LoadAsync(Transform parent, string src, VisualEffect splatPrefab, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            // Data fetching
            var fetchResult = await FetchData(src, onLoadingStatusChanged);

            if (fetchResult.Success == false)
            {
                return (false, null);
            }

            var parseResult = parser.TryParseSplatData(fetchResult.Data);

            switch (parseResult.Error)
            {
                case SplatVfxPlyParser.ParseErrorType.None:

                    // var data = CreateSplatData(fetchResult.Data);
                    var data = CreateSplatData(parseResult.SplatData);

                    InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);
                    var visualEffect = UnityEngine.Object.Instantiate(splatPrefab);
                    var splatObject = visualEffect.gameObject;

                    var binderBase = splatObject.AddComponent<VFXPropertyBinder>();
                    var binder = binderBase.AddPropertyBinder<VFXSplatDataBinder>();
                    binder.SplatData = data;

                    splatObject.transform.SetParent(parent, worldPositionStays: false);

                    InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);

                    return (true, splatObject);

                case SplatVfxPlyParser.ParseErrorType.InvalidHeader:
                    //TODO
                    return (false, null);

                default:
                    return (false, null);
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

#pragma warning disable CS0649

        private struct ReadData
        {
            public float px, py, pz;
            public float sx, sy, sz;
            public byte r, g, b, a;
            public byte rw, rx, ry, rz;
        }

#pragma warning restore CS0649

        private static (Vector3[] position, Vector3[] axis, Color[] color) LoadDataArrays(byte[] splatBytes)
        {
            // read text
            // splatBytes.Rea

            var bytes = new Span<byte>(splatBytes);
            var count = bytes.Length / 32;

            var source = MemoryMarshal.Cast<byte, ReadData>(bytes);

            var position = new Vector3[count];
            var axis = new Vector3[count * 3];
            var color = new Color[count];

            for (var i = 0; i < count; i++)
                ParseReadData(source[i],
                              out position[i],
                              out axis[i * 3],
                              out axis[i * 3 + 1],
                              out axis[i * 3 + 2],
                              out color[i]);

            return (position, axis, color);
        }

        [BurstCompile]
        private static void ParseReadData(in ReadData src,
                           out Vector3 position,
                           out Vector3 axis1,
                           out Vector3 axis2,
                           out Vector3 axis3,
                           out Color color)
        {
            var rv = (math.float4(src.rx, src.ry, src.rz, src.rw) - 128) / 128;
            var q = math.quaternion(rv.x, -rv.y, rv.z, -rv.w);
            position = math.float3(src.px, -src.py, src.pz);
            axis1 = math.mul(q, math.float3(src.sx, 0, 0));
            axis2 = math.mul(q, math.float3(0, src.sy, 0));
            axis3 = math.mul(q, math.float3(0, 0, src.sz));
            color = (Vector4)math.float4(src.r, src.g, src.b, src.a) / 255;
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
