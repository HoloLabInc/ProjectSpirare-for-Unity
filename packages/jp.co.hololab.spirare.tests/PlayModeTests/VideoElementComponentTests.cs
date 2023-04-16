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

public class VideoElementComponentTests
{
    private VideoElementObjectFactory factory
    {
        get
        {
            var factoryPath = "Packages/jp.co.hololab.spirare/Components/Standard/VideoElement/ScriptableObjects/StandardVideoElementObjectFactory.asset";
            var factory = (VideoElementObjectFactory)AssetDatabase.LoadAssetAtPath(factoryPath, typeof(VideoElementObjectFactory));
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

    private string videoDataPath = Path.GetFullPath("Packages/jp.co.hololab.spirare.tests/PlayModeTests/TestData/white.mp4");

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
    public async Task VideoObject_IsVisible()
    {
        var element = new PomlVideoElement()
        {
            Display = PomlDisplayType.Visible,
            Src = videoDataPath
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsVisible(go, loaderSettings.occlusionMaterial);
    }

    [Test]
    public async Task VideoObject_IsInvisible()
    {
        var element = new PomlVideoElement()
        {
            Display = PomlDisplayType.None,
            Src = videoDataPath
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsInvisible(go);
    }

    [TestCase(PomlDisplayType.None)]
    // [TestCase(PomlDisplayType.Occlusion)]
    public async Task VideoObject_ToVisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlVideoElement()
        {
            Display = initialDisplayType,
            Src = videoDataPath
        };

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
    public async Task VideoObject_ToInvisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlVideoElement()
        {
            Display = initialDisplayType,
            Src = videoDataPath
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        var objectElementComponent = go.GetComponent<PomlObjectElementComponent>();

        Assert.That(objectElementComponent, Is.Not.Null);

        element.Display = PomlDisplayType.None;
        objectElementComponent.InvokeElementUpdated();

        // wait until loading has completed
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsInvisible(go);
    }

    private async Task<GameObject> CreateObjectAsync(PomlVideoElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
    {
        var go = factory.CreateObject(element, loadOptions, parentTransform);
        await Task.Delay(100);
        return go;
    }
}
