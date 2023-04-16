using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class PomlLoaderTests
{
    private PomlLoaderSettings loaderSettings => SpirareTestUtils.DefaultLoaderSettings;

    private string testDataDir => Path.Combine("PlayModeTests", "TestData");
    private string modelPath => Path.Combine(testDataDir, "apple.glb");


    [SetUp]
    public void SetUp()
    {
        SpirareTestUtils.CreateMainCamera();
    }

    [Test]
    public async Task LoadNestedElements()
    {
        var pomlLoader = new PomlLoader(loaderSettings, PomlLoadOptions.DisplayModeType.Normal);

        var poml = @$"
<poml>
    <scene>
        <element id=""root"">
            <element id=""element0"" />
            <model id=""model0"" src=""{modelPath}"" />
            <text id=""text0"" text=""text"" />
            <audio id=""audio0"" />
            <image id=""image0"" />
            <video id=""video0"" />
            <geometry id=""geometry0"" />
            <cesium3dtiles id=""cesium3dtiles0"" />
        </element>
    </scene>
</poml> ";

        var pomlPath = "Packages/jp.co.hololab.spirare.tests/test.poml";
        var pomlFullPath = Path.GetFullPath(pomlPath);
        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, pomlFullPath);

        pomlComponent.TryGetElementById("root", out var rootElement);

        {
            var result = pomlComponent.TryGetElementById("model0", out var element);
            Assert.That(result, Is.True);

            AssertThatParentEquals(element.Component, rootElement.Component);
        }
        {
            var result = pomlComponent.TryGetElementById("element0", out var element);
            Assert.That(result, Is.True);

            AssertThatParentEquals(element.Component, rootElement.Component);
        }
        {
            var result = pomlComponent.TryGetElementById("text0", out var element);
            Assert.That(result, Is.True);

            AssertThatParentEquals(element.Component, rootElement.Component);
        }
        {
            var result = pomlComponent.TryGetElementById("audio0", out var element);
            Assert.That(result, Is.True);

            AssertThatParentEquals(element.Component, rootElement.Component);
        }
        {
            var result = pomlComponent.TryGetElementById("image0", out var element);
            Assert.That(result, Is.True);

            AssertThatParentEquals(element.Component, rootElement.Component);
        }
        {
            var result = pomlComponent.TryGetElementById("video0", out var element);
            Assert.That(result, Is.True);

            AssertThatParentEquals(element.Component, rootElement.Component);
        }
        {
            var result = pomlComponent.TryGetElementById("geometry0", out var element);
            Assert.That(result, Is.True);

            AssertThatParentEquals(element.Component, rootElement.Component);
        }
    }

    private static void AssertThatParentEquals(Component component, Component parentComponent)
    {
        Assert.That(component.transform.parent, Is.EqualTo(parentComponent.transform));
    }

    [Test]
    public async Task DisplayAttribute_NormalMode()
    {
        var pomlLoader = new PomlLoader(loaderSettings, PomlLoadOptions.DisplayModeType.Normal);

        var poml = @$"
<poml>
    <scene>
        <model id=""model0"" src=""{modelPath}"" display=""visible"" ar-display=""none"" />
        <model id=""model1"" src=""{modelPath}"" display=""none"" ar-display=""visible"" />
        <model id=""model2"" src=""{modelPath}"" display=""occlusion"" ar-display=""none"" />
    </scene>
</poml> ";

        var pomlPath = "Packages/jp.co.hololab.spirare.tests/test.poml";
        var pomlFullPath = Path.GetFullPath(pomlPath);
        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, pomlFullPath);
        await Task.Delay(100);

        {
            var result = pomlComponent.TryGetElementById("model0", out var element);
            Assert.That(result, Is.True);
            SpirareTestUtils.AssertThatMeshIsVisible(element.Component.gameObject, loaderSettings.occlusionMaterial);
        }
        {
            var result = pomlComponent.TryGetElementById("model1", out var element);
            Assert.That(result, Is.True);
            SpirareTestUtils.AssertThatMeshIsInvisible(element.Component.gameObject);
        }
        {
            var result = pomlComponent.TryGetElementById("model2", out var element);
            Assert.That(result, Is.True);
            SpirareTestUtils.AssertThatMeshIsOcclusion(element.Component.gameObject, loaderSettings.occlusionMaterial);
        }
    }

    [Test]
    public async Task DisplayAttribute_ARMode()
    {
        var pomlLoader = new PomlLoader(loaderSettings, PomlLoadOptions.DisplayModeType.AR);

        var poml = @$"
<poml>
    <scene>
        <model id=""model0"" src=""{modelPath}"" display=""none"" ar-display=""visible""/>
        <model id=""model1"" src=""{modelPath}"" display=""visible"" ar-display=""none""/>
        <model id=""model2"" src=""{modelPath}"" display=""none"" ar-display=""occlusion""/>
    </scene>
</poml> ";

        var pomlPath = "Packages/jp.co.hololab.spirare.tests/test.poml";
        var pomlFullPath = Path.GetFullPath(pomlPath);
        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, pomlFullPath);
        await Task.Delay(100);

        {
            var result = pomlComponent.TryGetElementById("model0", out var element);
            Assert.That(result, Is.True);
            SpirareTestUtils.AssertThatMeshIsVisible(element.Component.gameObject, loaderSettings.occlusionMaterial);
        }
        {
            var result = pomlComponent.TryGetElementById("model1", out var element);
            Assert.That(result, Is.True);
            SpirareTestUtils.AssertThatMeshIsInvisible(element.Component.gameObject);
        }
        {
            var result = pomlComponent.TryGetElementById("model2", out var element);
            Assert.That(result, Is.True);
            SpirareTestUtils.AssertThatMeshIsOcclusion(element.Component.gameObject, loaderSettings.occlusionMaterial);
        }
    }

    [Test]
    public async Task LoadXmlAsync_Transform()
    {
        var pomlLoader = new PomlLoader(loaderSettings, PomlLoadOptions.DisplayModeType.Normal);

        var rotation = Quaternion.Euler(10, 20, 30);
        var spirareRotation = CoordinateUtility.ToSpirareCoordinate(rotation);
        var rotationString = $"{spirareRotation.x} {spirareRotation.y} {spirareRotation.z} {spirareRotation.w}";

        var poml = @$"
<poml>
    <scene>
        <element id=""element0"" position=""1 2 3"" rotation=""{rotationString}"" scale=""4 5 6"" />
    </scene>
</poml>";

        var pomlPath = "Packages/jp.co.hololab.spirare.tests/test.poml";
        var pomlFullPath = Path.GetFullPath(pomlPath);
        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, pomlFullPath);

        {
            var result = pomlComponent.TryGetElementById("element0", out var element);
            Assert.That(result, Is.True);

            var transform = element.Component.transform;
            Assert.That(transform.localPosition, Is.EqualTo(CoordinateUtility.ToUnityCoordinate(1, 2, 3)));
            Assert.That(Quaternion.Angle(transform.localRotation, rotation), Is.LessThan(0.001));
            Assert.That(transform.localScale, Is.EqualTo(CoordinateUtility.ToUnityCoordinate(4, 5, 6, false)));
        }
    }
}
