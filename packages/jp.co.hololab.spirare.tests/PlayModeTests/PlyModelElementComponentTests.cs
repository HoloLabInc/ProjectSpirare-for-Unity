using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HoloLab.Spirare;
using HoloLab.Spirare.Pcx;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
// using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class PlyModelElementComponentTests
{
    private PlyModelElementObjectFactory factory
    {
        get
        {
            var factoryPath = "Packages/jp.co.hololab.spirare/Components/PCX/ScriptableObjects/PlyModelElementObjectFactory.asset";
            var factory = (PlyModelElementObjectFactory)AssetDatabase.LoadAssetAtPath(factoryPath, typeof(PlyModelElementObjectFactory));
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

    private string modelDataPath = Path.GetFullPath("Packages/jp.co.hololab.spirare.tests/PlayModeTests/TestData/pointcloud_binary.ply");

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
    public async Task ModelObject_IsVisible()
    {
        var element = new PomlModelElement()
        {
            Display = PomlDisplayType.Visible,
            Src = modelDataPath
        };

        var (go, _, _) = await CreateObjectAsync(element, normalLoadOptions);
        AssertThatMeshIsVisible(go);
    }

    [Test]
    public async Task ModelObject_IsInvisible()
    {
        var element = new PomlModelElement()
        {
            Display = PomlDisplayType.None,
            Src = modelDataPath
        };

        var (go, _, _) = await CreateObjectAsync(element, normalLoadOptions);
        AssertThatMeshIsInvisible(go);
    }

    /*
    [Test]
    public async Task ModelObject_IsOcclusion()
    {
        var element = new PomlModelElement()
        {
            Display = PomlDisplayType.Occlusion,
            Src = modelDataPath
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        SpirareTestUtils.AssertThatMeshIsOcclusion(go, loaderSettings.occlusionMaterial);
    }
    */

    [TestCase(PomlDisplayType.None)]
    // [TestCase(PomlDisplayType.Occlusion)]
    public async Task ModelObject_ToVisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlModelElement()
        {
            Display = initialDisplayType,
            Src = modelDataPath
        };

        var (go, objectElementComponent, modelElementComponent) = await CreateObjectAsync(element, normalLoadOptions);

        element.Display = PomlDisplayType.Visible;
        objectElementComponent.InvokeElementUpdated();

        await WaitUntilModelIsLoaded(modelElementComponent);

        AssertThatMeshIsVisible(go);
    }

    /*
    [TestCase(PomlDisplayType.Visible)]
    [TestCase(PomlDisplayType.None)]
    public async Task ModelObject_ToOcclusion(PomlDisplayType initialDisplayType)
    {
        var element = new PomlModelElement()
        {
            Display = initialDisplayType,
            Src = modelDataPath
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        var objectElementComponent = go.GetComponent<PomlObjectElementComponent>();

        Assert.That(objectElementComponent, Is.Not.Null);

        element.Display = PomlDisplayType.Occlusion;
        objectElementComponent.InvokeElementUpdated();

        // wait until loading has completed
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsOcclusion(go, loaderSettings.occlusionMaterial);
    }
    */

    [TestCase(PomlDisplayType.Visible)]
    // [TestCase(PomlDisplayType.Occlusion)]
    public async Task ModelObject_ToInvisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlModelElement()
        {
            Display = initialDisplayType,
            Src = modelDataPath
        };

        var (go, objectElementComponent, modelElementComponent) = await CreateObjectAsync(element, normalLoadOptions);

        element.Display = PomlDisplayType.None;
        objectElementComponent.InvokeElementUpdated();

        await WaitUntilModelIsLoaded(modelElementComponent);

        AssertThatMeshIsInvisible(go);
    }

    [Test]
    public async Task ModelObject_ParentHasChangedVisible_ChangeVisible()
    {
        var parentElement = new PomlEmptyElement()
        {
            Display = PomlDisplayType.None,
        };

        var modelElement = new PomlModelElement()
        {
            Parent = parentElement,
            Display = PomlDisplayType.Visible,
            Src = modelDataPath,
        };

        parentElement.Children = new PomlElement[] { modelElement };

        var parentComponent = CreateEmptyElementObject(parentElement);

        var (go, _, modelElementComponent) = await CreateObjectAsync(modelElement, normalLoadOptions, parentComponent.transform);

        AssertThatMeshIsInvisible(go);

        // Change parent to visible
        parentElement.Display = PomlDisplayType.Visible;
        parentComponent.InvokeElementUpdated();

        await WaitUntilModelIsLoaded(modelElementComponent);

        AssertThatMeshIsVisible(go);
    }

    private async Task<(GameObject GameObject, PomlObjectElementComponent PomlObjectElementComponent, ModelElementComponent ModelElementComponent)>
        CreateObjectAsync(PomlModelElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
    {
        var go = factory.CreateObject(element, loadOptions, parentTransform);

        var objectElementComponent = go.GetComponent<PomlObjectElementComponent>();
        var modelElementComponent = go.GetComponent<ModelElementComponent>();

        Assert.That(objectElementComponent, Is.Not.Null);
        Assert.That(modelElementComponent, Is.Not.Null);

        await WaitUntilModelIsLoaded(modelElementComponent);

        return (go, objectElementComponent, modelElementComponent);
    }

    private PomlObjectElementComponent CreateEmptyElementObject(PomlElement element)
    {
        var go = new GameObject();
        var pomlObjectElementComponent = go.AddComponent<PomlObjectElementComponent>();
        pomlObjectElementComponent.Initialize(element);
        return pomlObjectElementComponent;
    }

    private static void AssertThatMeshIsVisible(GameObject go)
    {
        var pointCloudRenderer = go.GetComponentInChildren<PointCloudRenderer>();
        Assert.That(pointCloudRenderer.enabled, Is.True);
        Assert.That(pointCloudRenderer.sourceData, Is.Not.Null);
    }

    private static void AssertThatMeshIsInvisible(GameObject go)
    {
        var pointCloudRenderer = go.GetComponentInChildren<PointCloudRenderer>();
        Assert.That(pointCloudRenderer.enabled, Is.False);
    }

    private static async Task WaitUntilModelIsLoaded(ModelElementComponent modelElementComponent, int timeoutMilliseconds = 5000)
    {
        var timeoutController = new TimeoutController();

        await UniTask.WaitUntil(
            () =>
            {
                switch (modelElementComponent.LoadingStatus)
                {
                    case PomlElementLoadingStatus.Loaded:
                    case PomlElementLoadingStatus.DataFetchError:
                    case PomlElementLoadingStatus.LoadError:
                        return true;
                    default:
                        return false;
                }
            },
            cancellationToken: timeoutController.Timeout(timeoutMilliseconds));
    }
}
