using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
        var floatArray = PomlParserUtility.ParseAsDoubleArray(text);
        Assert.That(floatArray, Is.EquivalentTo(expected));
    }
}
