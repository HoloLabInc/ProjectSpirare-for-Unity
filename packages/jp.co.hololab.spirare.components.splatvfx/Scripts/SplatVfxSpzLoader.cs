using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using SplatVfx;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.VFX;
using System.IO.Compression;
using System.IO;
using Codice.Client.Common.GameUI;
using System.Security.Cryptography;

namespace HoloLab.Spirare.Components.SplatVfx
{
    internal class SplatVfxSpzLoader : SplatLoaderBase
    {
        private enum CreateSplatDataResultType
        {
            Success,
            InvalidHeader,
            InvalidBody,
        }

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

            var result = TryCreateSplatData(fetchResult.Data, out var data);
            if (result != CreateSplatDataResultType.Success)
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelLoadError, onLoadingStatusChanged);
                return (false, null);
            }

            InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);
            var splatObject = SplatVfxUtil.InstantiateSplatVfx(splatPrefab, data, parent);
            InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);

            return (true, splatObject);
        }

        private static CreateSplatDataResultType TryCreateSplatData(byte[] spzBytes, out SplatData splatData)
        {
            using var inputFileStream = new MemoryStream(spzBytes);
            using var gzipStream = new GZipStream(inputFileStream, CompressionMode.Decompress);

            if (SpzUtils.TryReadHeader(gzipStream, out var header) == false)
            {
                splatData = null;
                return CreateSplatDataResultType.InvalidHeader;
            }

            if (IsValidHeader(header) == false)
            {
                splatData = null;
                return CreateSplatDataResultType.InvalidHeader;
            }

            if (SpzUtils.TryReadBody(gzipStream, header, out var spzBody) == false)
            {
                splatData = null;
                return CreateSplatDataResultType.InvalidBody;
            }

            var data = ScriptableObject.CreateInstance<SplatData>();

            var arrays = LoadDataArrays(spzBytes);
            data.PositionArray = arrays.position;
            data.AxisArray = arrays.axis;
            data.ColorArray = arrays.color;
            data.ReleaseGpuResources();

            splatData = null;
            return CreateSplatDataResultType.InvalidHeader;
        }

        private static bool IsValidHeader(SpzHeader header)
        {
            if (header.Magic != 0x5053474e)
            {
                Debug.LogError("Invalid magic number");
                return false;
            }

            if (header.Version != 2)
            {
                Debug.LogError($"Version {header.Version} is not supported");
                return false;
            }

            if (header.Reserved != 0)
            {
                Debug.LogError("Reserved must be 0");
                return false;
            }

            return true;
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

        private static class SpzUtils
        {
            public static bool TryReadHeader(GZipStream gzipStream, out SpzHeader header)
            {
                try
                {
                    using var binaryReader = new BinaryReader(gzipStream);
                    var magic = binaryReader.ReadUInt32();
                    var version = binaryReader.ReadUInt32();
                    var numPoints = binaryReader.ReadUInt32();
                    var shDegree = binaryReader.ReadByte();
                    var fractionalBits = binaryReader.ReadByte();
                    var flags = binaryReader.ReadByte();
                    var reserved = binaryReader.ReadByte();

                    header = new SpzHeader()
                    {
                        Magic = magic,
                        Version = version,
                        NumPoints = numPoints,
                        ShDegree = shDegree,
                        FractionalBits = fractionalBits,
                        Flags = flags,
                        Reserved = reserved
                    };
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    header = null;
                    return false;
                }
            }

            public static bool TryReadBody(GZipStream gzipStream, SpzHeader header, out SpzBody body)
            {
                try
                {
                    using var binaryReader = new BinaryReader(gzipStream);
                    var positions = ReadPositions(binaryReader, header);
                    var colors = ReadColors(binaryReader, header);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    body = null;
                    return false;
                }
            }

            private static Vector3[] ReadPositions(BinaryReader binaryReader, SpzHeader header)
            {
                var positionScale = 1f / (1 << header.FractionalBits);
                var positions = new Vector3[header.NumPoints];
                var bytes = new byte[4];
                for (var i = 0; i < header.NumPoints; i++)
                {
                    positions[i] = ReadPosition(binaryReader, positionScale, bytes);
                }

                return positions;
            }

            private static Vector3 ReadPosition(BinaryReader binaryReader, float scale, byte[] bytes)
            {
                var x = Read24bInt(binaryReader, bytes) * scale;
                var y = Read24bInt(binaryReader, bytes) * scale;
                var z = Read24bInt(binaryReader, bytes) * scale;

                return new Vector3(x, y, z);
            }

            private static int Read24bInt(BinaryReader binaryReader, byte[] bytes)
            {
                bytes[0] = binaryReader.ReadByte();
                bytes[1] = binaryReader.ReadByte();
                bytes[2] = binaryReader.ReadByte();
                bytes[3] = (bytes[2] & 0x80) == 0x80 ? (byte)0xff : (byte)0x00;

                return BitConverter.ToInt32(bytes, 0);
            }

            private static Color[] ReadColors(BinaryReader binaryReader, SpzHeader header)
            {
                var alphas = ReadAlphas(binaryReader, header);

                var colors = new Color[header.NumPoints];
                for (var i = 0; i < header.NumPoints; i++)
                {
                    var r = ReadColorByte(binaryReader);
                    var g = ReadColorByte(binaryReader);
                    var b = ReadColorByte(binaryReader);
                    var color = new Color(r, g, b, alphas[i]);
                    colors[i] = color;
                }

                return colors;
            }

            private static byte ReadColorByte(BinaryReader binaryReader)
            {
                var SH_C0 = 0.282f;
                var value = binaryReader.ReadByte();

                // The following implementation references Babylon.js.
                var color = Math.Clamp((byte)((value / 255f - SH_C0) * (1 + SH_C0 * 4) * 255), (byte)0, (byte)255);
                return color;
            }

            private static byte[] ReadAlphas(BinaryReader binaryReader, SpzHeader header)
            {
                var alphas = new byte[header.NumPoints];
                for (var i = 0; i < header.NumPoints; i++)
                {
                    alphas[i] = binaryReader.ReadByte();
                }

                return alphas;
            }


            private static Vector3[] ReadAxes(BinaryReader binaryReader, SpzHeader header)
            {
                var positions = new Vector3[header.NumPoints * 3];
                var bytes = new byte[4];
                for (var i = 0; i < header.NumPoints; i++)
                {
                    // positions[i] = ReadPosition(binaryReader, positionScale, bytes);
                }

                return positions;
            }


        }

        private class SpzHeader
        {
            public uint Magic;
            public uint Version;
            public uint NumPoints;
            public byte ShDegree;
            public byte FractionalBits;
            public byte Flags;
            public byte Reserved;
        }

        private class SpzBody
        {
            public Vector3[] Positions;
            public Vector3[] Axis;
            public Color[] Colors;
        }
    }
}

