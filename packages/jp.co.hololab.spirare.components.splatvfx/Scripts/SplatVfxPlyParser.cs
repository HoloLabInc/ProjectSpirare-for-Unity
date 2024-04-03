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
using System.IO;
using System.Collections.Generic;

namespace HoloLab.Spirare.Components.SplatVfx
{
    internal class SplatVfxPlyParser
    {
        public enum ParseErrorType
        {
            None,
            InvalidHeader,
            InvalidBody
        }

        public (SplatData SplatData, ParseErrorType Error) TryParseSplatData(byte[] splatBytes)
        {
            /*
            var data = ScriptableObject.CreateInstance<SplatData>();

            var arrays = LoadDataArrays(splatBytes);
            data.PositionArray = arrays.position;
            data.AxisArray = arrays.axis;
            data.ColorArray = arrays.color;
            data.ReleaseGpuResources();
            */

            using (var memoryStream = new MemoryStream(splatBytes))
            {
                using (var streamReader = new StreamReader(memoryStream))
                {
                    var headerResult = TryReadDataHeader(streamReader, out var header);

                    if (headerResult == false)
                    {
                        return (null, ParseErrorType.InvalidHeader);
                    }




                    return (null, ParseErrorType.None);
                }
            }


            // return data;
        }

        private static float ReadAsFloat(BinaryReader reader, DataType dataType)
        {
            return dataType switch
            {
                DataType.Int8 => (float)reader.ReadSByte(),
                DataType.UInt8 => (float)reader.ReadByte(),
                DataType.Int16 => (float)reader.ReadInt16(),
                DataType.UInt16 => (float)reader.ReadUInt16(),
                DataType.Int32 => (float)reader.ReadInt32(),
                DataType.UInt32 => (float)reader.ReadUInt32(),
                DataType.Float32 => reader.ReadSingle(),
                DataType.Float64 => (float)reader.ReadDouble(),
                _ => throw new InvalidOperationException()
            };
        }

        private static void SkipData(BinaryReader reader, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Int8:
                case DataType.UInt8:
                    reader.BaseStream.Position += 1;
                    break;

                case DataType.Int16:
                case DataType.UInt16:
                    reader.BaseStream.Position += 2;
                    break;

                case DataType.Int32:
                case DataType.UInt32:
                    reader.BaseStream.Position += 4;
                    break;

                case DataType.Float32:
                    reader.BaseStream.Position += 4;
                    break;

                case DataType.Float64:
                    reader.BaseStream.Position += 8;
                    break;
            }
        }

        private bool TryReadBody(Stream stream, DataHeader header, out (Vector3[] position, Vector3[] axis, Color[] color) body)
        {
            var reader = new BinaryReader(stream);

            var count = header.vertexCount;

            var positions = new Vector3[count];
            var axis = new Vector3[count * 3];
            var color = new Color[count * 3];

            float x = 0, y = 0, z = 0;
            //Byte r = 255, g = 255, b = 255, a = 255;
            float r, g, b, a;

            for (var i = 0; i < header.vertexCount; i++)
            {
                foreach (var prop in header.properties)
                {
                    switch (prop.Property)
                    {
                        case DataProperty.X:
                            x = ReadAsFloat(reader, prop.Type);
                            break;
                        case DataProperty.Y:
                            y = ReadAsFloat(reader, prop.Type);
                            break;
                        case DataProperty.Z:
                            z = ReadAsFloat(reader, prop.Type);
                            break;
                        case DataProperty.DC0:
                            r = ReadAsFloat(reader, prop.Type);
                            break;
                        case DataProperty.DC1:
                            g = ReadAsFloat(reader, prop.Type);
                            break;
                        case DataProperty.DC2:
                            b = ReadAsFloat(reader, prop.Type);
                            break;
                        default:
                            SkipData(reader, prop.Type);
                            break;


                    }
                }

            }

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

        enum DataProperty
        {
            Unknown,
            X,
            Y,
            Z,
            Scale0,
            Scale1,
            Scale2,
            Rot0,
            Rot1,
            Rot2,
            Rot3,
            Opacity,
            DC0,
            DC1,
            DC2,
        }

        private enum DataType
        {
            Unknown,
            Int8,
            UInt8,
            Int16,
            UInt16,
            Int32,
            UInt32,
            Float32,
            Float64,
        }

        private class DataHeader
        {
            public List<(DataProperty Property, DataType Type)> properties = new List<(DataProperty, DataType)>();
            public int vertexCount = -1;
        }

        private bool TryReadDataHeader(StreamReader reader, out DataHeader header)
        {
            header = new DataHeader();
            var readCount = 0;

            // Magic number line ("ply")
            var line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "ply")
            {
                return false;
            }

            // Data format: check if it's binary/little endian.
            line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "format binary_little_endian 1.0")
            {
                Debug.LogWarning($"Not supported format: {line}");
                return false;
            }

            // Read header contents.
            while (true)
            {
                // Read a line and split it with white space.
                line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                readCount += line.Length + 1;
                if (line == "end_header")
                {
                    break;
                }

                var col = line.Split();

                // Element declaration (unskippable)
                if (col[0] == "element")
                {
                    if (col[1] == "vertex")
                    {
                        header.vertexCount = Convert.ToInt32(col[2]);
                    }
                    else
                    {
                        continue;
                    }
                }

                if (col.Length < 3)
                {
                    continue;
                }

                // Property declaration line
                if (col[0] == "property")
                {
                    var dataType = ParseDataType(col[1]);
                    if (dataType == DataType.Unknown)
                    {
                        return false;
                    }

                    var dataProperty = ParseDataProperty(col[2]);

                    header.properties.Add((dataProperty, dataType));
                }
            }

            // Rewind the stream back to the exact position of the reader.
            reader.BaseStream.Position = readCount;

            return true;
        }

        private DataType ParseDataType(string dataTypeString)
        {
            return dataTypeString switch
            {
                "char" => DataType.Int8,
                "int8" => DataType.Int8,
                "uchar" => DataType.UInt8,
                "uint8" => DataType.UInt8,
                "short" => DataType.Int16,
                "int16" => DataType.Int16,
                "ushort" => DataType.UInt16,
                "uint16" => DataType.UInt16,
                "int" => DataType.Int32,
                "int32" => DataType.Int32,
                "uint" => DataType.UInt32,
                "uint32" => DataType.UInt32,
                "float" => DataType.Float32,
                "float32" => DataType.Float32,
                "double" => DataType.Float64,
                "float64" => DataType.Float64,
                _ => DataType.Unknown,
            };
        }

        private DataProperty ParseDataProperty(string dataPropertyString)
        {
            return dataPropertyString switch
            {
                "x" => DataProperty.X,
                "y" => DataProperty.Y,
                "z" => DataProperty.Z,

                _ => DataProperty.Unknown
            };
        }
    }
}

