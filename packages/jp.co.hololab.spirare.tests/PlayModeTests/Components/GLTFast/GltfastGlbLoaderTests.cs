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
        var gltfastGlbLoader = new GltfastGlbLoader();
        await gltfastGlbLoader.LoadAsync(go.transform, modelDataPath, onLoadingStatusChanged: status =>
        {
            loadingStatusList.Add(status);
        });

        var expectedStatusList = new List<GltfastGlbLoader.LoadingStatus>()
        {
            GltfastGlbLoader.LoadingStatus.DataFetching,
            GltfastGlbLoader.LoadingStatus.ModelLoading,
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

        var gltfastGlbLoader = new GltfastGlbLoader();
        await gltfastGlbLoader.LoadAsync(go.transform, invalidSrc, onLoadingStatusChanged: status =>
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

        var gltfastGlbLoader = new GltfastGlbLoader();
        await gltfastGlbLoader.LoadAsync(go.transform, invalidModelDataPath, onLoadingStatusChanged: status =>
        {
            loadingStatusList.Add(status);
        });

        var expectedStatusList = new List<GltfastGlbLoader.LoadingStatus>()
        {
            GltfastGlbLoader.LoadingStatus.DataFetching,
            GltfastGlbLoader.LoadingStatus.ModelLoading,
            GltfastGlbLoader.LoadingStatus.ModelLoadError,
        };
        Assert.That(loadingStatusList, Is.EquivalentTo(expectedStatusList));

        GameObject.DestroyImmediate(go);
    }

    [Test]
    public async Task LoadAsync_MeshIsSharedWithSameUrlObjects()
    {
        var gltfastGlbLoader = new GltfastGlbLoader();

        var go1 = new GameObject("model1");
        var loadTask1 = gltfastGlbLoader.LoadAsync(go1.transform, modelDataPath);

        var go2 = new GameObject("model2");
        var loadTask2 = gltfastGlbLoader.LoadAsync(go2.transform, modelDataPath);

        await Task.WhenAll(loadTask1, loadTask2);

        var meshFilter1 = go1.GetComponentInChildren<MeshFilter>();
        var meshFilter2 = go2.GetComponentInChildren<MeshFilter>();
        Assert.That(meshFilter1.sharedMesh, Is.EqualTo(meshFilter2.sharedMesh));

        GameObject.DestroyImmediate(go1);
        GameObject.DestroyImmediate(go2);
    }

    [Test]
    public async Task LoadAsync_MeshIsSharedWithSameUrlAndMaterialObjects()
    {
        var gltfastGlbLoader = new GltfastGlbLoader();

        var material = new Material(Shader.Find("Standard"));
        var go1 = new GameObject("model1");
        var loadTask1 = gltfastGlbLoader.LoadAsync(go1.transform, modelDataPath, material);

        var go2 = new GameObject("model2");
        var loadTask2 = gltfastGlbLoader.LoadAsync(go2.transform, modelDataPath, material);

        await Task.WhenAll(loadTask1, loadTask2);

        var meshFilter1 = go1.GetComponentInChildren<MeshFilter>();
        var meshFilter2 = go2.GetComponentInChildren<MeshFilter>();
        Assert.That(meshFilter1.sharedMesh, Is.EqualTo(meshFilter2.sharedMesh));

        GameObject.DestroyImmediate(go1);
        GameObject.DestroyImmediate(go2);
    }
}
