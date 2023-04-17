using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloLab.Spirare;
using HoloLab.Spirare.Wasm.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class EnumUtilityTests
{
    [TestCase(0, PomlDisplayType.Visible)]
    [TestCase(1, PomlDisplayType.None)]
    [TestCase(2, PomlDisplayType.Occlusion)]
    public void TryToEnum_Success(int value, PomlDisplayType expectedDisplayType)
    {
        var result = EnumUtility.TryToEnum(value, out PomlDisplayType displayType);
        Assert.That(result, Is.True);
        Assert.That(displayType, Is.EqualTo(expectedDisplayType));
    }

    [TestCase(-1)]
    [TestCase(100)]
    public void TryToEnum_FailToConvert(int value)
    {
        var result = EnumUtility.TryToEnum(value, out PomlDisplayType displayType);
        Assert.That(result, Is.False);
    }
}
