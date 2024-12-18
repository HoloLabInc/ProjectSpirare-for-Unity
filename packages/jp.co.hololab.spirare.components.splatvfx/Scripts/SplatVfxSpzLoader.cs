using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;
using SplatVfx;
using UnityEngine.VFX;
using System.IO.Compression;
using System.IO;
using Unity.Mathematics;

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

            var (result, data) = await CreateSplatDataAsync(fetchResult.Data);
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

        private static async Task<(CreateSplatDataResultType Result, SplatData SplatData)> CreateSplatDataAsync(byte[] spzBytes)
        {
            var (result, spzBody) = await Task.Run(() =>
            {
                using var inputFileStream = new MemoryStream(spzBytes);
                using var gzipStream = new GZipStream(inputFileStream, CompressionMode.Decompress);
                using var binaryReader = new BinaryReader(gzipStream);

                if (SpzUtils.TryReadHeader(binaryReader, out var header) == false)
                {
                    return (CreateSplatDataResultType.InvalidHeader, null);
                }

                if (IsValidHeader(header) == false)
                {
                    return (CreateSplatDataResultType.InvalidHeader, null);
                }

                if (SpzUtils.TryReadBody(binaryReader, header, out var spzBody) == false)
                {
                    return (CreateSplatDataResultType.InvalidBody, null);
                }

                return (CreateSplatDataResultType.Success, spzBody);
            });

            if (result != CreateSplatDataResultType.Success)
            {
                return (result, null);
            }

            var splatData = ScriptableObject.CreateInstance<SplatData>();
            splatData.PositionArray = spzBody.Positions;
            splatData.AxisArray = spzBody.Axis;
            splatData.ColorArray = spzBody.Colors;

            return (CreateSplatDataResultType.Success, splatData);
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

        private static class SpzUtils
        {
            public static bool TryReadHeader(BinaryReader binaryReader, out SpzHeader header)
            {
                try
                {
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

            public static bool TryReadBody(BinaryReader binaryReader, SpzHeader header, out SpzBody body)
            {
                try
                {
                    var positions = ReadPositions(binaryReader, header);
                    var colors = ReadColors(binaryReader, header);
                    var axes = ReadAxes(binaryReader, header);

                    body = new SpzBody()
                    {
                        Positions = positions,
                        Colors = colors,
                        Axis = axes
                    };
                    return true;
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
                static int Read24bInt(BinaryReader binaryReader, byte[] bytes)
                {
                    bytes[0] = binaryReader.ReadByte();
                    bytes[1] = binaryReader.ReadByte();
                    bytes[2] = binaryReader.ReadByte();
                    bytes[3] = (bytes[2] & 0x80) == 0x80 ? (byte)0xff : (byte)0x00;

                    return BitConverter.ToInt32(bytes, 0);
                }

                var x = Read24bInt(binaryReader, bytes) * scale;
                var y = Read24bInt(binaryReader, bytes) * scale;
                var z = Read24bInt(binaryReader, bytes) * scale;

                return new Vector3(x, y, -z);
            }

            private static Color[] ReadColors(BinaryReader binaryReader, SpzHeader header)
            {
                static float ReadColorByte(BinaryReader binaryReader)
                {
                    var SH_C0 = 0.282f;
                    var value = binaryReader.ReadByte();

                    // The following implementation references Babylon.js.
                    var color = Math.Clamp((value / 255f - SH_C0) * (1 + SH_C0 * 4), 0f, 1f);
                    return color;
                }

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

            private static float[] ReadAlphas(BinaryReader binaryReader, SpzHeader header)
            {
                var alphas = new float[header.NumPoints];
                for (var i = 0; i < header.NumPoints; i++)
                {
                    alphas[i] = binaryReader.ReadByte() / 255f;
                }

                return alphas;
            }

            private static Vector3[] ReadAxes(BinaryReader binaryReader, SpzHeader header)
            {
                var scales = ReadScales(binaryReader, header);

                var axes = new Vector3[header.NumPoints * 3];
                for (var i = 0; i < header.NumPoints; i++)
                {
                    var quaternion = ReadQuaternion(binaryReader);
                    var scale = scales[i];
                    axes[i * 3 + 0] = math.mul(quaternion, math.float3(scale.x, 0, 0));
                    axes[i * 3 + 1] = math.mul(quaternion, math.float3(0, scale.y, 0));
                    axes[i * 3 + 2] = math.mul(quaternion, math.float3(0, 0, scale.z));
                }

                return axes;
            }

            private static Vector3[] ReadScales(BinaryReader binaryReader, SpzHeader header)
            {
                static float ReadScale(BinaryReader binaryReader)
                {
                    var value = binaryReader.ReadByte();
                    return Mathf.Exp(value / 16f - 10);
                }

                var scales = new Vector3[header.NumPoints];
                for (var i = 0; i < header.NumPoints; i++)
                {
                    var x = ReadScale(binaryReader);
                    var y = ReadScale(binaryReader);
                    var z = ReadScale(binaryReader);
                    scales[i] = new Vector3(x, y, z);
                }

                return scales;
            }

            private static quaternion ReadQuaternion(BinaryReader binaryReader)
            {
                static float ReadQuaternionValue(BinaryReader binaryReader)
                {
                    var value = binaryReader.ReadByte();
                    return value / 127.5f - 1f;
                }

                var x = ReadQuaternionValue(binaryReader);
                var y = ReadQuaternionValue(binaryReader);
                var z = ReadQuaternionValue(binaryReader);
                var w = math.sqrt(math.max(0f, 1 - x * x - y * y - z * z));

                return new quaternion(x, y, -z, -w);
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

