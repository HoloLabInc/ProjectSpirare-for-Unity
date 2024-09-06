using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static HoloLab.Spirare.Cesium.PomlBoundsVerticesAttribute;

namespace HoloLab.Spirare.Cesium
{
    internal static class Poml3dTilesParserUtility
    {
        private static readonly Regex verticesAttributesKeyRegex = new Regex(@"^(.*?):", RegexOptions.Compiled);

        public static PomlBoundsVerticesAttribute ParseAsBoundsVerticesAttribute(string text)
        {
            var (coordinateSystem, numberArrayString) = ParseCoordinateSystemType(text);

            switch (coordinateSystem)
            {
                case CoordinateSystemType.Relative:
                    var relativePositions = PomlParserUtility.ParseAsVector2Array(numberArrayString);
                    return CreateRelative(relativePositions);

                case CoordinateSystemType.Geodetic:
                    var geodeticPositions = PomlParserUtility.ParseAsGeodeticPositionArray(numberArrayString, noHeight: true);
                    return CreateGeodetic(geodeticPositions);

                case CoordinateSystemType.Unknown:
                default:
                    return CreateUnkown();
            }
        }

        private static (CoordinateSystemType CoordinateSystem, string NumberArrayString) ParseCoordinateSystemType(string text)
        {
            var match = verticesAttributesKeyRegex.Match(text);
            if (match.Success)
            {
                var key = match.Groups[1].Value.Trim().ToLower();
                var coordinateSystemType = key switch
                {
                    "geodetic" => CoordinateSystemType.Geodetic,
                    "relative" => CoordinateSystemType.Relative,
                    _ => CoordinateSystemType.Unknown,
                };
                var numberArrayString = text.Substring(match.Groups[0].Length);
                return (coordinateSystemType, numberArrayString);
            }
            else
            {
                return (CoordinateSystemType.Relative, text);
            }
        }
    }

    internal class PomlBoundsVerticesAttribute
    {
        public enum CoordinateSystemType
        {
            Unknown = 0,
            Relative,
            Geodetic
        }

        public CoordinateSystemType CoordinateSystem { get; }

        public Vector2[] RelativePositions { get; }

        public PomlGeodeticPosition[] GeodeticPositions { get; }

        protected PomlBoundsVerticesAttribute(
            CoordinateSystemType coordinateSystem,
            Vector2[] relativePositions,
            PomlGeodeticPosition[] geodeticPositions)
        {
            CoordinateSystem = coordinateSystem;
            RelativePositions = relativePositions;
            GeodeticPositions = geodeticPositions;
        }

        public static PomlBoundsVerticesAttribute CreateUnkown()
        {
            return new PomlBoundsVerticesAttribute(CoordinateSystemType.Unknown, null, null);
        }

        public static PomlBoundsVerticesAttribute CreateRelative(Vector2[] positions)
        {
            return new PomlBoundsVerticesAttribute(CoordinateSystemType.Relative, positions, null);
        }

        public static PomlBoundsVerticesAttribute CreateGeodetic(PomlGeodeticPosition[] positions)
        {
            return new PomlBoundsVerticesAttribute(CoordinateSystemType.Geodetic, null, positions);
        }
    }
}

