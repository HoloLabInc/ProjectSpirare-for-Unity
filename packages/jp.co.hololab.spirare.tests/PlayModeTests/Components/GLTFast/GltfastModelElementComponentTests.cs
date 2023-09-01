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

public class GltfastModelElementComponentTests
{
    private GltfastModelElementObjectFactory factory
    {
        get
        {
            var factoryPath = "Packages/jp.co.hololab.spirare/Components/GLTFast/ScriptableObjects/GltfastModelElementObjectFactory.asset";
            var factory = (GltfastModelElementObjectFactory)AssetDatabase.LoadAssetAtPath(factoryPath, typeof(GltfastModelElementObjectFactory));
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

    private string modelDataPath = Path.GetFullPath("Packages/jp.co.hololab.spirare.tests/PlayModeTests/TestData/apple.glb");

    [SetUp]
    public void SetUp()
    {
        SpirareTestUtils.CreateMainCamera();
    }

    [Test]
    public async Task ModelObject_IsVisible()
    {
        var element = new PomlModelElement()
        {
            Display = PomlDisplayType.Visible,
            Src = modelDataPath
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        SpirareTestUtils.AssertThatMeshIsVisible(go, loaderSettings.occlusionMaterial);
    }

    [Test]
    public async Task ModelObject_IsInvisible()
    {
        var element = new PomlModelElement()
        {
            Display = PomlDisplayType.None,
            Src = modelDataPath
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        SpirareTestUtils.AssertThatMeshIsInvisible(go);
    }

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

    [TestCase(PomlDisplayType.None)]
    [TestCase(PomlDisplayType.Occlusion)]
    public async Task ModelObject_ToVisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlModelElement()
        {
            Display = initialDisplayType,
            Src = modelDataPath
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

    [TestCase(PomlDisplayType.Visible)]
    [TestCase(PomlDisplayType.Occlusion)]
    public async Task ModelObject_ToInvisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlModelElement()
        {
            Display = initialDisplayType,
            Src = modelDataPath
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

        var go = await CreateObjectAsync(modelElement, normalLoadOptions, parentComponent.transform);
        var objectElementComponent = go.GetComponent<PomlObjectElementComponent>();

        Assert.That(objectElementComponent, Is.Not.Null);
        SpirareTestUtils.AssertThatMeshIsInvisible(go);

        // Change parent to visible
        parentElement.Display = PomlDisplayType.Visible;
        parentComponent.InvokeElementUpdated();

        // wait until loading has completed
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsVisible(go, loaderSettings.occlusionMaterial);
    }

    [Test]
    public async Task ModelObject_LoadingStatusChangedEventInvoked()
    {
        var element = new PomlModelElement()
        {
            Display = PomlDisplayType.Visible,
            Src = modelDataPath
        };

        var go = factory.CreateObject(element, normalLoadOptions);
        var modelElementComponent = go.GetComponent<ModelElementComponent>();
        Assert.That(modelElementComponent.LoadingStatus, Is.EqualTo(PomlElementLoadingStatus.DataFetching));

        var loadingStatusList = new List<PomlElementLoadingStatus>();
        modelElementComponent.LoadingStatusChanged += (status) =>
        {
            loadingStatusList.Add(status);
        };

        await WaitUntilModelIsLoaded(modelElementComponent);

        var expectedStatusList = new[]
        {
            PomlElementLoadingStatus.Loading,
            PomlElementLoadingStatus.Loaded
        };
        Assert.That(loadingStatusList, Is.EquivalentTo(expectedStatusList));
    }

    [Test]
    public async Task ModelObject_DataFethErrorThrownWhenSrcIsInvalid()
    {
        var element = new PomlModelElement()
        {
            Display = PomlDisplayType.Visible,
            Src = "invalidPath"
        };

        var go = factory.CreateObject(element, normalLoadOptions);
        var modelElementComponent = go.GetComponent<ModelElementComponent>();
        Assert.That(modelElementComponent.LoadingStatus, Is.EqualTo(PomlElementLoadingStatus.DataFetching));

        var loadingStatusList = new List<PomlElementLoadingStatus>();
        modelElementComponent.LoadingStatusChanged += (status) =>
        {
            loadingStatusList.Add(status);
        };

        var expectedStatusList = new[]
        {
            PomlElementLoadingStatus.DataFetchError,
        };

        await WaitUntilModelIsLoaded(modelElementComponent);

        Assert.That(loadingStatusList, Is.EquivalentTo(expectedStatusList));
    }

    private async Task<GameObject> CreateObjectAsync(PomlModelElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
    {
        var go = factory.CreateObject(element, loadOptions, parentTransform);
        await Task.Delay(100);
        return go;
    }

    private PomlObjectElementComponent CreateEmptyElementObject(PomlElement element)
    {
        var go = new GameObject();
        var pomlObjectElementComponent = go.AddComponent<PomlObjectElementComponent>();
        pomlObjectElementComponent.Initialize(element);
        return pomlObjectElementComponent;
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
