using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;
using SplatVfx;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.VFX;

namespace HoloLab.Spirare.Components.SplatVfx
{
    internal class SplatVfxSplatLoader : SplatLoaderBase
    {
        public async Task<(bool Success, GameObject SplatObject)> LoadAsync(Transform parent, string src, VisualEffect splatPrefab,
            Action<LoadingStatus> onLoadingStatusChanged = null,
            CancellationToken cancellationToken = default)
        {
            // Data fetching
            var fetchResult = await FetchData(src, onLoadingStatusChanged);

            if (fetchResult.Success == false || cancellationToken.IsCancellationRequested)
            {
                return (false, null);
            }

            var data = CreateSplatData(fetchResult.Data);
            InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);
            var splatObject = SplatVfxUtil.InstantiateSplatVfx(splatPrefab, data, parent);
            InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);

            return (true, splatObject);
        }

        private static SplatData CreateSplatData(byte[] splatBytes)
        {
            var data = ScriptableObject.CreateInstance<SplatData>();

            var arrays = LoadDataArrays(splatBytes);
            data.PositionArray = arrays.position;
            data.AxisArray = arrays.axis;
            data.ColorArray = arrays.color;
            data.ReleaseGpuResources();

            return data;
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
    }
}
