using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PomlParserUtilityTests
{
    private static IEnumerable<object[]> ParseAsFloatArrayTestCases()
    {
        return new List<object[]>
                    {
                        new object[]{"1", new float[] { 1 } },
                        new object[]{"1 2 -3", new float[] { 1, 2, -3 } },
                        new object[]{" 1.25 2.5 -3 ", new float[] { 1.25f, 2.5f, -3 } },
                        new object[]{" 1, 2,-3   4 ", new float[] { 1, 2, -3, 4 } },
                        new object[]{" ,   ,  -1,,,,2,3,", new float[] { -1, 2, 3 } },
                        new object[]{"  ,  -1,2,a,3", new float[] { -1, 2 } },
                    };
    }

    [TestCaseSource(nameof(ParseAsFloatArrayTestCases))]
    public void ParseAsFloatArray(string text, float[] expected)
    {
        var floatArray = PomlParserUtility.ParseAsFloatArray(text);
        Assert.That(floatArray, Is.EquivalentTo(expected));
    }

    private static IEnumerable<object[]> ParseAsDoubleArrayTestCases()
    {
        return new List<object[]>
                    {
                        new object[]{"1", new double[] { 1 } },
                        new object[]{"1 2 -3", new double[] { 1, 2, -3 } },
                        new object[]{" 1.25 2.5 -3 ", new double[] { 1.25, 2.5, -3 } },
                        new object[]{" 1, 2,-3   4 ", new double[] { 1, 2, -3, 4 } },
                        new object[]{" ,   ,  -1,,,,2,3,", new double[] { -1, 2, 3 } },
                        new object[]{"  ,  -1,2,a,3", new double[] { -1, 2 } },
                    };
    }

    [TestCaseSource(nameof(ParseAsDoubleArrayTestCases))]
    public void ParseAsDoubleArray(string text, double[] expected)
    {
        var doubleArray = PomlParserUtility.ParseAsDoubleArray(text);
        Assert.That(doubleArray, Is.EquivalentTo(expected));
    }

    private static IEnumerable<object[]> ParseAsVector3ArrayTestCases()
    {
        return new List<object[]>
                    {
                        new object[]{"1", new Vector3[] { } },
                        new object[]{"1 2 -3", new Vector3[] { new Vector3(1, 2, -3) } },
                        new object[]{" 1.25 2.5 -3 ", new Vector3[] { new Vector3(1.25f, 2.5f, -3) } },
                        new object[]{" 1, 2,-3   4 ", new Vector3[] { new Vector3(1, 2, -3) } },
                        new object[]{" ,   ,  -1,,,,2,3,", new Vector3[] { new Vector3(-1, 2, 3) } },
                        new object[]{"  ,  -1,2,a,3", new Vector3[] { } },
                        new object[]{"1,2,3,4,5,6", new Vector3[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6) } },
                    };
    }

    [TestCaseSource(nameof(ParseAsVector3ArrayTestCases))]
    public void ParseAsVector3Array(string text, Vector3[] expected)
    {
        var vector3Array = PomlParserUtility.ParseAsVector3Array(text);
        Assert.That(vector3Array, Is.EquivalentTo(expected));
    }

    private static IEnumerable<object[]> ParseAsGeodeticPositionArrayTestCases()
    {
        return new List<object[]>
                    {
                        new object[]{"1 2 3", new PomlGeodeticPosition[] { new PomlGeodeticPosition(1, 2, 3) } },
                        new object[]{"1 2.25 -3.5", new PomlGeodeticPosition[] { new PomlGeodeticPosition(1, 2.25, -3.5) } },
                        new object[]{"1,2 3   4,,,5 , 6", new PomlGeodeticPosition[]
                        {
                            new PomlGeodeticPosition(1, 2, 3),
                            new PomlGeodeticPosition(4, 5, 6)
                        } },
                        new object[]{"1 2 3 4", new PomlGeodeticPosition[] { new PomlGeodeticPosition(1, 2, 3) } },
                    };
    }

    [TestCaseSource(nameof(ParseAsGeodeticPositionArrayTestCases))]
    public void ParseAsGeodeticPositionArray(string text, PomlGeodeticPosition[] expected)
    {
        var geodeticPositionArray = PomlParserUtility.ParseAsGeodeticPositionArray(text);
        Assert.That(geodeticPositionArray, Is.EquivalentTo(expected));
    }
}
