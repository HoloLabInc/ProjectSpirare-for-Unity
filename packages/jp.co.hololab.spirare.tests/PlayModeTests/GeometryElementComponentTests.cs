using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    private static readonly IList<PomlGeometry> TestGeometries = new List<PomlGeometry>()
    {
        new LineGeometry()
        {
            Vertices = "0 0 0 1 1 1"
        },
        new PolygonGeometry()
        {
            Vertices = "0 0 0 0 0 1 0 1 0",
            Indices = new int[] { 0, 1, 2 }
        },
    };

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

    [TestCaseSource(nameof(TestGeometries))]
    public async Task GeometryObject_IsVisible(PomlGeometry pomlGeometry)
    {
        var element = new PomlGeometryElement()
        {
            Display = PomlDisplayType.Visible,
        };
        element.Geometries.Add(pomlGeometry);

        var go = await CreateObjectAsync(element, normalLoadOptions);

        SpirareTestUtils.AssertThatMeshIsVisible(go, loaderSettings.occlusionMaterial);
    }

    [TestCaseSource(nameof(TestGeometries))]
    public async Task GeometryObject_IsInvisible(PomlGeometry pomlGeometry)
    {
        var element = new PomlGeometryElement()
        {
            Display = PomlDisplayType.None,
        };
        element.Geometries.Add(pomlGeometry);

        var go = await CreateObjectAsync(element, normalLoadOptions);

        SpirareTestUtils.AssertThatMeshIsInvisible(go);
    }

    private static IEnumerable<object[]> ToVisibleTestCases()
    {
        var initialDisplayType = new PomlDisplayType[]
        {
            PomlDisplayType.None
        };

        return initialDisplayType.SelectMany(
            displayType => TestGeometries,
            (displayType, geometry) => new object[] { displayType, geometry });
    }

    [TestCaseSource(nameof(ToVisibleTestCases))]
    //PomlDisplayType.None)]
    // [TestCase(PomlDisplayType.Occlusion)]
    public async Task LineGeometryObject_ToVisible(PomlDisplayType initialDisplayType, PomlGeometry pomlGeometry)
    {
        var element = new PomlGeometryElement()
        {
            Display = initialDisplayType,
        };
        element.Geometries.Add(pomlGeometry);

        var go = await CreateObjectAsync(element, normalLoadOptions);
        var objectElementComponent = go.GetComponent<PomlObjectElementComponent>();

        Assert.That(objectElementComponent, Is.Not.Null);

        element.Display = PomlDisplayType.Visible;
        objectElementComponent.InvokeElementUpdated();

        // wait until loading has completed
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsVisible(go, loaderSettings.occlusionMaterial);
    }

    private static IEnumerable<object[]> ToInvisibleTestCases()
    {
        var initialDisplayType = new PomlDisplayType[]
        {
            PomlDisplayType.Visible
        };

        return initialDisplayType.SelectMany(
            displayType => TestGeometries,
            (displayType, geometry) => new object[] { displayType, geometry });
    }

    // [TestCase(PomlDisplayType.Visible)]
    // [TestCase(PomlDisplayType.Occlusion)]
    [TestCaseSource(nameof(ToInvisibleTestCases))]
    public async Task LineGeometryObject_ToInvisible(PomlDisplayType initialDisplayType, PomlGeometry pomlGeometry)
    {
        var element = new PomlGeometryElement()
        {
            Display = initialDisplayType,
        };
        element.Geometries.Add(pomlGeometry);

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
