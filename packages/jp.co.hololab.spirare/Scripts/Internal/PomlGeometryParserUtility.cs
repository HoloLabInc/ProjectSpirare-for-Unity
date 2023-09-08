using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static HoloLab.Spirare.PomlGeometryPositionsAttribute;

namespace HoloLab.Spirare
{

    internal static class PomlGeometryParserUtility
    {
        private static readonly Regex geometryAttributesKeyRegex = new Regex(@"^(.*?):", RegexOptions.Compiled);

        public static PomlGeometryPositionsAttribute ParseAsGeometryPositionsAttribute(string text)
        {
            var (coordinateSystem, numberArrayString) = ParseCoordinateSystemType(text);

            switch (coordinateSystem)
            {
                case CoordinateSystemType.Relative:
                    var relativePositions = PomlParserUtility.ParseAsVector3Array(numberArrayString);
                    return CreateRelative(relativePositions);

                case CoordinateSystemType.Geodetic:
                    var geodeticPositions = PomlParserUtility.ParseAsGeodeticPositionArray(numberArrayString);
                    return CreateGeodetic(geodeticPositions);

                case CoordinateSystemType.Unknown:
                default:
                    return CreateUnkown();
            }
        }

        private static (CoordinateSystemType CoordinateSystem, string NumberArrayString) ParseCoordinateSystemType(string text)
        {
            var match = geometryAttributesKeyRegex.Match(text);
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

    internal class PomlGeometryPositionsAttribute
    {
        public enum CoordinateSystemType
        {
            Unknown = 0,
            Relative,
            Geodetic
        }

        public CoordinateSystemType CoordinateSystem { get; }

        public Vector3[] RelativePositions { get; }

        public PomlGeodeticPosition[] GeodeticPositions { get; }

        protected PomlGeometryPositionsAttribute(
            CoordinateSystemType coordinateSystem,
            Vector3[] relativePositions,
            PomlGeodeticPosition[] geodeticPositions)
        {
            CoordinateSystem = coordinateSystem;
            RelativePositions = relativePositions;
            GeodeticPositions = geodeticPositions;
        }

        public static PomlGeometryPositionsAttribute CreateUnkown()
        {
            return new PomlGeometryPositionsAttribute(CoordinateSystemType.Unknown, null, null);
        }

        public static PomlGeometryPositionsAttribute CreateRelative(Vector3[] positions)
        {
            return new PomlGeometryPositionsAttribute(CoordinateSystemType.Relative, positions, null);
        }

        public static PomlGeometryPositionsAttribute CreateGeodetic(PomlGeodeticPosition[] positions)
        {
            return new PomlGeometryPositionsAttribute(CoordinateSystemType.Geodetic, null, positions);
        }
    }
}
