using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
// using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class PomlComponentTests
{
    private PomlLoader pomlLoader => new PomlLoader(SpirareTestUtils.DefaultLoaderSettings, PomlLoadOptions.DisplayModeType.Normal);

    [SetUp]
    public void SetUp()
    {
        SpirareTestUtils.CreateMainCamera();
    }

    [Test]
    public async Task TryGetElementById()
    {
        var poml = @"
<poml>
    <scene>
        <element id=""element0"" />
    </scene>
</poml>";

        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, "");

        {
            var result = pomlComponent.TryGetPomlElementComponentById("element0", out var elementComponent);
            Assert.That(result, Is.True);
            Assert.That(elementComponent.PomlElement.Id, Is.EqualTo("element0"));
        }
        {
            var result = pomlComponent.TryGetPomlElementComponentById("non-match-id", out var _);
            Assert.That(result, Is.False);
        }
    }

    [Test]
    public async Task TryGetElementById_CannotGetDisposedElement()
    {
        var poml = @"
<poml>
    <scene>
        <element id=""element0"" />
    </scene>
</poml>";

        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, "");

        {
            var result = pomlComponent.TryGetPomlElementComponentById("element0", out var elementComponent);
            Assert.That(result, Is.True);
            Assert.That(elementComponent.PomlElement.Id, Is.EqualTo("element0"));

            Object.Destroy(elementComponent.gameObject);
            await UniTask.Yield();
        }

        {
            var result = pomlComponent.TryGetPomlElementComponentById("element0", out var _);
            Assert.That(result, Is.False);
        }
    }

    [Test]
    public async Task GetAllElements()
    {
        var poml = @"
<poml>
    <scene>
        <element id=""element0"">
            <element id=""element1"" />
        </element>
        <element>
        </element>
        <text text=""test"">
        </text>
    </scene>
</poml>";

        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, "");

        var elements = pomlComponent.GetAllPomlElementComponents();

        // TODO Should also be able to fetch the scene root?
        Assert.That(elements.Count, Is.EqualTo(4));
        Assert.That(elements.Count(x => x.PomlElement.ElementType == PomlElementType.Element), Is.EqualTo(3));
    }

    public async Task ElementDescriptorTest()
    {
        var poml = @"
<poml>
    <scene>
        <element id=""element0"" />
        <text id=""text0"" />
    </scene>
</poml>";

        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, "");

        {
            var result1 = pomlComponent.TryGetElementById("element0", out var elemComponent1, out var elemDescr);
            Assert.That(result1, Is.True);
            Assert.That(elemComponent1.PomlElement.ElementType, Is.EqualTo(PomlElementType.Element));

            var result2 = pomlComponent.TryGetElementByDescriptor(elemDescr, out var elemComponent2);
            Assert.That(result2, Is.True);
            Assert.That(elemComponent2.PomlElement.Id, Is.EqualTo("element0"));

            Assert.That(elemComponent1, Is.EqualTo(elemComponent2));
        }

        {
            var result1 = pomlComponent.TryGetElementById("text0", out var elemComponent1, out var elemDescr);
            Assert.That(result1, Is.True);
            Assert.That(elemComponent1.PomlElement.ElementType, Is.EqualTo(PomlElementType.Text));

            var result2 = pomlComponent.TryGetElementByDescriptor(elemDescr, out var elemComponent2);
            Assert.That(result2, Is.True);
            Assert.That(elemComponent2.PomlElement.Id, Is.EqualTo("text0"));

            Assert.That(elemComponent1, Is.EqualTo(elemComponent2));
        }

        {
            var result = pomlComponent.TryGetPomlElementComponentById("non-match-id", out _);
            Assert.That(result, Is.False);
        }
    }
}
