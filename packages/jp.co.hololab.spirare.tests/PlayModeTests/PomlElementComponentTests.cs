using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
// using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class PomlElementComponentTests
{
    private PomlLoader pomlLoader => new PomlLoader(SpirareTestUtils.DefaultLoaderSettings, PomlLoadOptions.DisplayModeType.Normal);

    [SetUp]
    public void SetUp()
    {
        SpirareTestUtils.CreateMainCamera();
    }

    [Test]
    public void InvokeElementUpdated_OnElementUpdatedCalled()
    {
        var element = new PomlEmptyElement()
        {
            Display = PomlDisplayType.Visible
        };

        var go = new GameObject();
        var objectElementComponent = go.AddComponent<PomlObjectElementComponent>();
        objectElementComponent.Initialize(element);

        var onElementUpdatedCalled = false;
        objectElementComponent.OnElementUpdated += _ =>
        {
            onElementUpdatedCalled = true;
        };

        objectElementComponent.InvokeElementUpdated();

        Assert.That(onElementUpdatedCalled, Is.True);
    }

    [Test]
    public void InvokeElementUpdated_OnElementDisplayTypeUpdatedNotCalled()
    {
        var element = new PomlEmptyElement()
        {
            Display = PomlDisplayType.Visible
        };

        var go = new GameObject();
        var objectElementComponent = go.AddComponent<PomlObjectElementComponent>();
        objectElementComponent.Initialize(element);

        var onElementDiplyTypeUpdated = false;
        objectElementComponent.OnElementDisplayTypeUpdated += _ =>
        {
            onElementDiplyTypeUpdated = true;
        };

        objectElementComponent.InvokeElementUpdated();

        Assert.That(onElementDiplyTypeUpdated, Is.False);
    }

    [TestCase(PomlDisplayType.Visible, PomlDisplayType.None)]
    [TestCase(PomlDisplayType.Visible, PomlDisplayType.Occlusion)]
    [TestCase(PomlDisplayType.Occlusion, PomlDisplayType.Visible)]
    [TestCase(PomlDisplayType.Occlusion, PomlDisplayType.None)]
    [TestCase(PomlDisplayType.None, PomlDisplayType.Visible)]
    [TestCase(PomlDisplayType.None, PomlDisplayType.Occlusion)]
    public void InvokeElementUpdated_DisplayChanged_OnElementDisplayTypeUpdatedCalled(PomlDisplayType initialDisplayType, PomlDisplayType updatedDisplayType)
    {
        var element = new PomlEmptyElement()
        {
            Display = initialDisplayType
        };

        var go = new GameObject();
        var objectElementComponent = go.AddComponent<PomlObjectElementComponent>();
        objectElementComponent.Initialize(element);

        var onElementDisplayTypeUpdatedCalled = false;
        objectElementComponent.OnElementDisplayTypeUpdated += _ =>
        {
            onElementDisplayTypeUpdatedCalled = true;
        };

        element.Display = updatedDisplayType;
        objectElementComponent.InvokeElementUpdated();

        Assert.That(onElementDisplayTypeUpdatedCalled, Is.True);
    }

    [TestCase(PomlArDisplayType.Visible, PomlArDisplayType.None)]
    [TestCase(PomlArDisplayType.Visible, PomlArDisplayType.Occlusion)]
    [TestCase(PomlArDisplayType.Occlusion, PomlArDisplayType.Visible)]
    [TestCase(PomlArDisplayType.Occlusion, PomlArDisplayType.None)]
    [TestCase(PomlArDisplayType.None, PomlArDisplayType.Visible)]
    [TestCase(PomlArDisplayType.None, PomlArDisplayType.Occlusion)]
    public void InvokeElementUpdated_ArDisplayChanged_OnElementDisplayTypeUpdatedCalled(PomlArDisplayType initialDisplayType, PomlArDisplayType updatedDisplayType)
    {
        var element = new PomlEmptyElement()
        {
            ArDisplay = initialDisplayType
        };

        var go = new GameObject();
        var objectElementComponent = go.AddComponent<PomlObjectElementComponent>();
        objectElementComponent.Initialize(element);

        var onElementDisplayTypeUpdatedCalled = false;
        objectElementComponent.OnElementDisplayTypeUpdated += _ =>
        {
            onElementDisplayTypeUpdatedCalled = true;
        };

        element.ArDisplay = updatedDisplayType;
        objectElementComponent.InvokeElementUpdated();

        Assert.That(onElementDisplayTypeUpdatedCalled, Is.True);
    }
}
