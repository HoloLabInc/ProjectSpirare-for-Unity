using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HoloLab.Spirare;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
// using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class TextElementComponentTests
{
    private TextElementObjectFactory factory
    {
        get
        {
            var factoryPath = "Packages/jp.co.hololab.spirare/Components/Standard/TextElement/ScriptableObjects/StandardTextElementObjectFactory.asset";
            var factory = (TextElementObjectFactory)AssetDatabase.LoadAssetAtPath(factoryPath, typeof(TextElementObjectFactory));
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
    public async Task TextObject_IsVisible()
    {
        var element = new PomlTextElement("test text")
        {
            Display = PomlDisplayType.Visible,
            BackgroundColor = Color.black,
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);

        SpirareTestUtils.AssertThatMeshIsVisible(go, loaderSettings.occlusionMaterial);
        var text = go.GetComponentInChildren<TMP_Text>();
        Assert.That(text.text, Is.EqualTo("test text"));
    }

    [Test]
    public async Task TextObject_IsInvisible()
    {
        var element = new PomlTextElement("test text")
        {
            Display = PomlDisplayType.None,
            BackgroundColor = Color.black,
        };

        var go = await CreateObjectAsync(element, normalLoadOptions);
        await UniTask.Delay(100);

        SpirareTestUtils.AssertThatMeshIsInvisible(go);
    }

    [Test]
    public async Task TextObject_CreatedUnderInactiveObject()
    {
        var parent = new GameObject("parent");
        parent.SetActive(false);

        var element = new PomlTextElement("test text")
        {
            Display = PomlDisplayType.Visible,
            BackgroundColor = Color.black,
        };

        var go = await CreateObjectAsync(element, normalLoadOptions, parent.transform);
        await UniTask.Delay(100);

        parent.SetActive(true);

        SpirareTestUtils.AssertThatMeshIsVisible(go, loaderSettings.occlusionMaterial);
        var text = go.GetComponentInChildren<TMP_Text>();
        Assert.That(text.text, Is.EqualTo("test text"));
    }

    [Test]
    public async Task TextObject_BackplateSizeIsUpdatedWhenObjectIsActive()
    {
        var element = new PomlTextElement("test text")
        {
            Display = PomlDisplayType.Visible,
            BackgroundColor = Color.black,
        };

        var parent = new GameObject("parent");
        parent.SetActive(false);
        var go = await CreateObjectAsync(element, normalLoadOptions, parent.transform);

        var go2 = await CreateObjectAsync(element, normalLoadOptions);

        await UniTask.Delay(100);

        var backPlate = go.transform.Find("BackPlate").gameObject;
        var backPlate2 = go2.transform.Find("BackPlate").gameObject;

        // The size of backPlate is zero when object is inactive
        Assert.That(backPlate.transform.localScale, Is.EqualTo(Vector3.zero));

        parent.SetActive(true);
        await UniTask.Delay(100);

        Assert.That(backPlate.transform.localScale, Is.EqualTo(backPlate2.transform.localScale));
    }

    [TestCase(PomlDisplayType.None)]
    // [TestCase(PomlDisplayType.Occlusion)]
    public async Task TextObject_ToVisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlTextElement("test text")
        {
            Display = PomlDisplayType.None,
            BackgroundColor = Color.black,
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
    public async Task TextObject_ToInvisible(PomlDisplayType initialDisplayType)
    {
        var element = new PomlTextElement("test text")
        {
            Display = PomlDisplayType.None,
            BackgroundColor = Color.black,
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

    private async Task<GameObject> CreateObjectAsync(PomlTextElement element, PomlLoadOptions loadOptions, Transform parentTransform = null)
    {
        var go = factory.CreateObject(element, loadOptions, parentTransform);
        await Task.Delay(100);
        return go;
    }
}
