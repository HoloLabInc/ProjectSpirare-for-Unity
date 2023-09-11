using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    internal static class PomlParserUtility
    {
        public static string[] SplitArrayString(string text)
        {
            var separator = new char[] { ',', ' ' };
            var tokens = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return tokens;
        }

        public static List<float> ParseAsFloatArray(string text)
        {
            var tokens = SplitArrayString(text);
            var values = new List<float>(tokens.Length);

            foreach (var token in tokens)
            {
                if (!float.TryParse(token, out var value))
                {
                    break;
                }
                values.Add(value);
            }
            return values;
        }

        public static List<double> ParseAsDoubleArray(string text)
        {
            var tokens = SplitArrayString(text);
            var values = new List<double>(tokens.Length);

            foreach (var token in tokens)
            {
                if (!double.TryParse(token, out var value))
                {
                    break;
                }
                values.Add(value);
            }
            return values;
        }

        public static Vector3[] ParseAsVector3Array(string text)
        {
            var values = ParseAsFloatArray(text);

            var result = new Vector3[values.Count / 3];
            for (int i = 0; i < result.Length; i++)
            {
                var x = values[i * 3];
                var y = values[i * 3 + 1];
                var z = values[i * 3 + 2];
                result[i] = new Vector3(x, y, z);
            }

            return result;
        }

        public static PomlGeodeticPosition[] ParseAsGeodeticPositionArray(string text)
        {
            var values = ParseAsDoubleArray(text);

            var result = new PomlGeodeticPosition[values.Count / 3];
            for (int i = 0; i < result.Length; i++)
            {
                var longitude = values[i * 3];
                var latitude = values[i * 3 + 1];
                var ellipsoidalHeight = values[i * 3 + 2];
                result[i] = new PomlGeodeticPosition(longitude, latitude, ellipsoidalHeight);
            }
            return result;
        }
    }
}
