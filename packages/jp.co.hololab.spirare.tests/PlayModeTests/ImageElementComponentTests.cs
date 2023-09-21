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

public class ImageModelElementComponentTests
{
    private ImageElementObjectFactory factory
    {
        get
        {
            var factoryPath = "Packages/jp.co.hololab.spirare/Components/Standard/ImageElement/ScriptableObjects/StandardImageElementObjectFactory.asset";
            var factory = (ImageElementObjectFactory)AssetDatabase.LoadAssetAtPath(factoryPath, typeof(ImageElementObjectFactory));
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

    private string imageDataPath = Path.GetFullPath("Packages/jp.co.hololab.spirare.tests/PlayModeTests/TestData/image.png");

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
    public async Task ImageObject_FrontfaceIsVisible()
    {
        var element = new PomlImageElement()
        {
            Display = PomlDisplayType.Visible,
            Src = imageDataPath
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        var (frontfaceRenderer, backfaceRenderer) = GetFrontAndBackfaceRenderer(go);

        SpirareTestUtils.AssertThatMeshIsVisible(frontfaceRenderer, loaderSettings.occlusionMaterial);
        SpirareTestUtils.AssertThatMeshIsInvisible(backfaceRenderer);
    }

    [TestCase(PomlBackfaceType.Solid)]
    [TestCase(PomlBackfaceType.Visible)]
    [TestCase(PomlBackfaceType.Flipped)]
    public async Task ImageObject_FrontfaceAndBackfaceAreVisible(PomlBackfaceType backfaceType)
    {
        var element = new PomlImageElement()
        {
            Display = PomlDisplayType.Visible,
            Src = imageDataPath,
            Backface = backfaceType
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        var (frontfaceRenderer, backfaceRenderer) = GetFrontAndBackfaceRenderer(go);

        SpirareTestUtils.AssertThatMeshIsVisible(frontfaceRenderer, loaderSettings.occlusionMaterial);
        SpirareTestUtils.AssertThatMeshIsVisible(backfaceRenderer, loaderSettings.occlusionMaterial);
    }

    [TestCase(PomlBackfaceType.None)]
    [TestCase(PomlBackfaceType.Solid)]
    [TestCase(PomlBackfaceType.Visible)]
    [TestCase(PomlBackfaceType.Flipped)]
    public async Task ImageObject_FrontfaceAndBackfaceAreInvisible(PomlBackfaceType backfaceType)
    {
        var element = new PomlImageElement()
        {
            Display = PomlDisplayType.None,
            Src = imageDataPath,
            Backface = backfaceType
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);

        SpirareTestUtils.AssertThatMeshIsInvisible(go);
    }

    [TestCase(PomlDisplayType.None)]
    // [TestCase(PomlDisplayType.Occlusion)]
    public async Task ImageObject_ToVisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlImageElement()
        {
            Display = initialDisplayType,
            Src = imageDataPath
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        var (frontfaceRenderer, backfaceRenderer) = GetFrontAndBackfaceRenderer(go);

        var objectElementComponent = go.GetComponent<PomlObjectElementComponent>();

        Assert.That(objectElementComponent, Is.Not.Null);

        element.Display = PomlDisplayType.Visible;
        objectElementComponent.InvokeElementUpdated();

        // wait until loading has completed
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsVisible(frontfaceRenderer, loaderSettings.occlusionMaterial);
        SpirareTestUtils.AssertThatMeshIsInvisible(backfaceRenderer);
    }

    [TestCase(PomlDisplayType.Visible)]
    // [TestCase(PomlDisplayType.Occlusion)]
    public async Task ModelObject_ToInvisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlImageElement()
        {
            Display = initialDisplayType,
            Src = imageDataPath
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

    private async Task<GameObject> CreateObjectAsync(PomlImageElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
    {
        var go = factory.CreateObject(element, loadOptions, parentTransform);
        await Task.Delay(100);
        return go;
    }

    private (MeshRenderer frontfaceRenderer, MeshRenderer backfaceRenderer) GetFrontAndBackfaceRenderer(GameObject go)
    {
        var meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
        Assert.That(meshRenderers.Length, Is.EqualTo(2));
        var frontfaceRenderer = meshRenderers.First(x => x.transform.GetSiblingIndex() == 0);
        var backfaceRenderer = meshRenderers.First(x => x.transform.GetSiblingIndex() != 0);
        return (frontfaceRenderer, backfaceRenderer);
    }
}
