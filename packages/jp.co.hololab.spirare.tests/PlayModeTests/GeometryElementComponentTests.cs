using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
// using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class GeometryElementComponentTests
{
    private GeometryElementObjectFactory factory
    {
        get
        {
            var factoryPath = "Packages/jp.co.hololab.spirare/Components/Standard/GeometryElement/ScriptableObjects/StandardGeometryElementObjectFactory.asset";
            var factory = (GeometryElementObjectFactory)AssetDatabase.LoadAssetAtPath(factoryPath, typeof(GeometryElementObjectFactory));
            return factory;
        }
    }

    private PomlLoaderSettings loaderSettings => SpirareTestUtils.DefaultLoaderSettings;

    private PomlLoadOptions normalLoadOptions
    {
        get
        {
            return new PomlLoadOptions()
            {
                DisplayMode = PomlLoadOptions.DisplayModeType.Normal,
                OcclusionMaterial = loaderSettings.occlusionMaterial
            };
        }
    }

    private PomlGeometry simpleLineGeometry
    {
        get
        {
            return new LineGeometry()
            {
                Vertices = "0 0 0 1 1 1"
                /*
                PositionType = PositionType.Relative,
                Start = Vector3.zero,
                End = Vector3.one,
                */
            };
        }
    }

    [SetUp]
    public void SetUp()
    {
        SpirareTestUtils.CreateMainCamera();
    }

    [TearDown]
    public void TearDown()
    {
        SpirareTestUtils.ClearScene();
    }

    [Test]
    public async Task GeometryObject_IsVisible()
    {
        var element = new PomlGeometryElement()
        {
            Display = PomlDisplayType.Visible,
        };
        element.Geometries.Add(simpleLineGeometry);

        var go = await CreateObjectAsync(element, normalLoadOptions);

        SpirareTestUtils.AssertThatMeshIsVisible(go, loaderSettings.occlusionMaterial);
    }

    [Test]
    public async Task GeometryObject_IsInvisible()
    {
        var element = new PomlGeometryElement()
        {
            Display = PomlDisplayType.None,
        };
        element.Geometries.Add(simpleLineGeometry);

        var go = await CreateObjectAsync(element, normalLoadOptions);

        SpirareTestUtils.AssertThatMeshIsInvisible(go);
    }

    [TestCase(PomlDisplayType.None)]
    // [TestCase(PomlDisplayType.Occlusion)]
    public async Task GeometryObject_ToVisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlGeometryElement()
        {
            Display = initialDisplayType,
        };
        element.Geometries.Add(simpleLineGeometry);

        var go = await CreateObjectAsync(element, normalLoadOptions);
        var objectElementComponent = go.GetComponent<PomlObjectElementComponent>();

        Assert.That(objectElementComponent, Is.Not.Null);

        element.Display = PomlDisplayType.Visible;
        objectElementComponent.InvokeElementUpdated();

        // wait until loading has completed
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsVisible(go, loaderSettings.occlusionMaterial);
    }

    [TestCase(PomlDisplayType.Visible)]
    // [TestCase(PomlDisplayType.Occlusion)]
    public async Task GeometryObject_ToInvisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlGeometryElement()
        {
            Display = initialDisplayType,
        };
        element.Geometries.Add(simpleLineGeometry);

        var go = await CreateObjectAsync(element, normalLoadOptions);
        var objectElementComponent = go.GetComponent<PomlObjectElementComponent>();

        Assert.That(objectElementComponent, Is.Not.Null);

        element.Display = PomlDisplayType.None;
        objectElementComponent.InvokeElementUpdated();

        // wait until loading has completed
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsInvisible(go);
    }

    private async Task<GameObject> CreateObjectAsync(PomlGeometryElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
    {
        var go = factory.CreateObject(element, null, loadOptions, parentTransform);
        await Task.Delay(100);
        return go;
    }
}
