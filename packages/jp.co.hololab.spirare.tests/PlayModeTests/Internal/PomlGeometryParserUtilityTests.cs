using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static HoloLab.Spirare.PomlGeometryPositionsAttribute;

[TestFixture]
public class PomlGeometryParserUtilityTests
{
    private static IEnumerable<object[]> ParseAsGeometryPositionsAttribute_Relative_TestCases()
    {
        return new List<object[]>
                    {
                        new object[]{"1,2,3", new Vector3[] { new Vector3(1, 2, 3) } },
                        new object[]{"1 2 -3", new Vector3[] { new Vector3(1, 2, -3) } },
                        new object[]{" 1.25 2.5 -3 ", new Vector3[] { new Vector3(1.25f, 2.5f, -3) } },
                        new object[]{" 1, 2,-3   4 ", new Vector3[] { new Vector3(1, 2, -3) } },
                        new object[]{"relative:1,2,3", new Vector3[] { new Vector3(1, 2, 3) } },
                        new object[]{"  relative  :1 2 3", new Vector3[] { new Vector3(1, 2, 3) } },
                        new object[]{"relative:   1 2 -3", new Vector3[] { new Vector3(1, 2, -3) } },
                        new object[]{"RelaTive:1 2 3", new Vector3[] { new Vector3(1, 2, 3) } },
                    };
    }

    [TestCaseSource(nameof(ParseAsGeometryPositionsAttribute_Relative_TestCases))]
    public void ParseAsGeometryPositionsAttribute_Relative(string text, Vector3[] expected)
    {
        var positionsAttribute = PomlGeometryParserUtility.ParseAsGeometryPositionsAttribute(text);
        Assert.That(positionsAttribute.CoordinateSystem, Is.EqualTo(CoordinateSystemType.Relative));
        Assert.That(positionsAttribute.RelativePositions, Is.EquivalentTo(expected));
    }

    private static IEnumerable<object[]> ParseAsGeometryPositionsAttribute_Geodetic_TestCases()
    {
        return new List<object[]>
                    {
                        new object[]{"geodetic:1,2,3 4  -5.5 , 6", new PomlGeodeticPosition[] {
                            new PomlGeodeticPosition(1, 2, 3),
                            new PomlGeodeticPosition(4, -5.5, 6),
                        } },
                        new object[]{"  GeoDetic  :1 2 3", new PomlGeodeticPosition[] { new PomlGeodeticPosition(1, 2, 3) } },
                    };
    }

    [TestCaseSource(nameof(ParseAsGeometryPositionsAttribute_Geodetic_TestCases))]
    public void ParseAsGeometryPositionsAttribute_Geodetic(string text, PomlGeodeticPosition[] expected)
    {
        var positionsAttribute = PomlGeometryParserUtility.ParseAsGeometryPositionsAttribute(text);
        Assert.That(positionsAttribute.CoordinateSystem, Is.EqualTo(CoordinateSystemType.Geodetic));
        Assert.That(positionsAttribute.GeodeticPositions, Is.EquivalentTo(expected));
    }

    private static IEnumerable<object[]> ParseAsGeometryPositionsAttribute_Unknown_TestCases()
    {
        return new List<object[]>
                    {
                        new object[]{"x:1,2,3"},
                        new object[]{"::1,2,3"},
                    };
    }

    [TestCaseSource(nameof(ParseAsGeometryPositionsAttribute_Unknown_TestCases))]
    public void ParseAsGeometryPositionsAttribute_Unknown(string text)
    {
        var positionsAttribute = PomlGeometryParserUtility.ParseAsGeometryPositionsAttribute(text);
        Assert.That(positionsAttribute.CoordinateSystem, Is.EqualTo(CoordinateSystemType.Unknown));
    }
}
