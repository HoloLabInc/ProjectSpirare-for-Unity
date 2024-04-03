using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
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

        public class SplatData
        {
            public Vector3[] Positions;
            public Vector3[] Axes;
            public Color[] Colors;
        }

        public (SplatData SplatData, ParseErrorType Error) TryParseSplatData(byte[] splatBytes)
        {
            using (var memoryStream = new MemoryStream(splatBytes))
            {
                using (var streamReader = new StreamReader(memoryStream))
                {
                    var headerResult = TryReadDataHeader(streamReader, out var header);
                    if (headerResult == false)
                    {
                        return (null, ParseErrorType.InvalidHeader);
                    }

                    var bodyResult = TryReadBody(memoryStream, header, out var body);
                    if (bodyResult == false)
                    {
                        return (null, ParseErrorType.InvalidBody);
                    }

                    return (body, ParseErrorType.None);
                }
            }
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

        private bool TryReadBody(Stream stream, DataHeader header, out SplatData data)
        {
            var reader = new BinaryReader(stream);

            var count = header.vertexCount;

            var positions = new Vector3[count];
            var axis = new Vector3[count * 3];
            var color = new Color[count];

            for (var i = 0; i < header.vertexCount; i++)
            {
                float x = 0, y = 0, z = 0;
                float r = 0, g = 0, b = 0, a = 0;
                float rx = 0, ry = 0, rz = 0, rw = 1;
                float scaleX = 0, scaleY = 0, scaleZ = 0;

                const double SH_C0 = 0.28209479177387814;

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
                            r = (float)(0.5 + SH_C0 * ReadAsFloat(reader, prop.Type));
                            break;
                        case DataProperty.DC1:
                            g = (float)(0.5 + SH_C0 * ReadAsFloat(reader, prop.Type));
                            break;
                        case DataProperty.DC2:
                            b = (float)(0.5 + SH_C0 * ReadAsFloat(reader, prop.Type));
                            break;
                        case DataProperty.DC3:
                            a = (float)(0.5 + SH_C0 * ReadAsFloat(reader, prop.Type));
                            break;
                        case DataProperty.Opacity:
                            a = (float)(1 / (1 + Math.Exp(-ReadAsFloat(reader, prop.Type))));
                            break;
                        case DataProperty.Rot0:
                            rw = ReadAsFloat(reader, prop.Type);
                            break;
                        case DataProperty.Rot1:
                            rx = ReadAsFloat(reader, prop.Type);
                            break;
                        case DataProperty.Rot2:
                            ry = ReadAsFloat(reader, prop.Type);
                            break;
                        case DataProperty.Rot3:
                            rz = ReadAsFloat(reader, prop.Type);
                            break;
                        case DataProperty.Scale0:
                            scaleX = math.exp(ReadAsFloat(reader, prop.Type));
                            break;
                        case DataProperty.Scale1:
                            scaleY = math.exp(ReadAsFloat(reader, prop.Type));
                            break;
                        case DataProperty.Scale2:
                            scaleZ = math.exp(ReadAsFloat(reader, prop.Type));
                            break;
                        default:
                            SkipData(reader, prop.Type);
                            break;
                    }
                }

                positions[i] = new Vector3(x, -y, z);
                var q = math.quaternion(rx, -ry, rz, -rw);
                var axis1 = math.mul(q, math.float3(scaleX, 0, 0));
                var axis2 = math.mul(q, math.float3(0, scaleY, 0));
                var axis3 = math.mul(q, math.float3(0, 0, scaleZ));

                axis[3 * i + 0] = axis1;
                axis[3 * i + 1] = axis2;
                axis[3 * i + 2] = axis3;

                color[i] = new Color(r, g, b, a);
            }

            data = new SplatData
            {
                Positions = positions,
                Axes = axis,
                Colors = color
            };
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
            DC3,
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
            // TODO
            return dataPropertyString switch
            {
                "x" => DataProperty.X,
                "y" => DataProperty.Y,
                "z" => DataProperty.Z,
                "f_dc_0" => DataProperty.DC0,
                "f_dc_1" => DataProperty.DC1,
                "f_dc_2" => DataProperty.DC2,
                "f_dc_3" => DataProperty.DC3,
                "scale_0" => DataProperty.Scale0,
                "scale_1" => DataProperty.Scale1,
                "scale_2" => DataProperty.Scale2,
                "rot_0" => DataProperty.Rot0,
                "rot_1" => DataProperty.Rot1,
                "rot_2" => DataProperty.Rot2,
                "rot_3" => DataProperty.Rot3,
                "opacity" => DataProperty.Opacity,

                _ => DataProperty.Unknown
            };
        }
    }
}

