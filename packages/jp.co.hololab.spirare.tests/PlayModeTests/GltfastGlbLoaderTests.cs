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

public class GltfastGlbLoaderTests
{
    private string modelDataPath = Path.GetFullPath("Packages/jp.co.hololab.spirare.tests/PlayModeTests/TestData/apple.glb");
    private string invalidModelDataPath = Path.GetFullPath("Packages/jp.co.hololab.spirare.tests/PlayModeTests/TestData/image.png");

    [SetUp]
    public void SetUp()
    {
        // SpirareTestUtils.CreateMainCamera();
    }

    [Test]
    public async Task LoadingStatusChangesWhenModelIsBeingLoaded()
    {
        var go = new GameObject("modelParent");

        var loadingStatusList = new List<GltfastGlbLoader.LoadingStatus>();
        await GltfastGlbLoader.LoadAsync(go, modelDataPath, onLoadingStatusChanged: status =>
        {
            loadingStatusList.Add(status);
        });

        var expectedStatusList = new List<GltfastGlbLoader.LoadingStatus>()
        {
            GltfastGlbLoader.LoadingStatus.DataFetching,
            GltfastGlbLoader.LoadingStatus.DataLoading,
            GltfastGlbLoader.LoadingStatus.ModelInstantiating,
            GltfastGlbLoader.LoadingStatus.Loaded,
        };
        Assert.That(loadingStatusList, Is.EquivalentTo(expectedStatusList));

        GameObject.DestroyImmediate(go);
    }

    [Test]
    public async Task DataFetchErrorThrownWhenSrcNotFound()
    {
        var go = new GameObject("modelParent");
        var invalidSrc = "invalid.glb";

        var loadingStatusList = new List<GltfastGlbLoader.LoadingStatus>();

        await GltfastGlbLoader.LoadAsync(go, invalidSrc, onLoadingStatusChanged: status =>
        {
            loadingStatusList.Add(status);
        });

        var expectedStatusList = new List<GltfastGlbLoader.LoadingStatus>()
        {
            GltfastGlbLoader.LoadingStatus.DataFetching,
            GltfastGlbLoader.LoadingStatus.DataFetchError,
        };
        Assert.That(loadingStatusList, Is.EquivalentTo(expectedStatusList));

        GameObject.DestroyImmediate(go);
    }

    [Test]
    public async Task DataLoadErrorThrownWhenSrcFileIsInvalid()
    {
        var go = new GameObject("modelParent");

        var loadingStatusList = new List<GltfastGlbLoader.LoadingStatus>();

        await GltfastGlbLoader.LoadAsync(go, invalidModelDataPath, onLoadingStatusChanged: status =>
        {
            loadingStatusList.Add(status);
        });

        var expectedStatusList = new List<GltfastGlbLoader.LoadingStatus>()
        {
            GltfastGlbLoader.LoadingStatus.DataFetching,
            GltfastGlbLoader.LoadingStatus.DataLoading,
            GltfastGlbLoader.LoadingStatus.DataLoadError,
        };
        Assert.That(loadingStatusList, Is.EquivalentTo(expectedStatusList));

        GameObject.DestroyImmediate(go);
    }
}
