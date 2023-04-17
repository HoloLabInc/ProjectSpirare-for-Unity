using System;
using System.Threading.Tasks;
using HoloLab.Spirare;
using HoloLab.Spirare.Wasm.Core;
using HoloLab.Spirare.Wasm.Core.Spirare;
using HoloLab.Spirare.Wasm.Tests;
using NUnit.Framework;

public class ElementAttributeImplementationTests
{
    private PomlLoaderSettings loaderSettings => SpirareTestUtils.DefaultLoaderSettings;

    private const int selfObjectDescriptor = (int)SpecialElementDescriptor.SelfObject;

    [SetUp]
    public void SetUp()
    {
        SpirareTestUtils.CreateMainCamera();
    }

    [Test]
    public async Task GetDisplayTest()
    {
        using var mem = new ByteMemory(1024);
        var memoryPtr = mem.Ptr;
        var memoryLength = (uint)mem.Length;

        var poml = @$"
<poml>
    <scene>
        <element id=""element0"" />
        <element id=""element1"" display=""none"" />
        <element id=""element2"" display=""occlusion"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        {
            var api = CreateApiImpl(pomlComponent, "element0");
            var ptr = 0;
            api.get_display(memoryPtr, memoryLength, selfObjectDescriptor, ptr);
            AssertThatDisplayIsWritten(memoryPtr, memoryLength, ptr, PomlDisplayType.Visible);
        }
        {
            var api = CreateApiImpl(pomlComponent, "element1");
            var ptr = 0;
            api.get_display(memoryPtr, memoryLength, selfObjectDescriptor, ptr);
            AssertThatDisplayIsWritten(memoryPtr, memoryLength, ptr, PomlDisplayType.None);
        }
        {
            var api = CreateApiImpl(pomlComponent, "element2");
            var ptr = 0;
            api.get_display(memoryPtr, memoryLength, selfObjectDescriptor, ptr);
            AssertThatDisplayIsWritten(memoryPtr, memoryLength, ptr, PomlDisplayType.Occlusion);
        }
    }

    [Test]
    public async Task SetDisplayTest()
    {
        using var mem = new ByteMemory(1024);
        var memoryPtr = mem.Ptr;
        var memoryLength = (uint)mem.Length;

        var poml = @$"
<poml>
    <scene>
        <element id=""element0"" display=""none""/>
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        pomlComponent.TryGetElementById("element0", out var element);
        Assert.That(element.Element.Display, Is.EqualTo(PomlDisplayType.None));

        var api = CreateApiImpl(pomlComponent, "element0");

        {
            var display = (int)PomlDisplayType.Visible;
            api.set_display(memoryPtr, memoryLength, selfObjectDescriptor, display);
            Assert.That(element.Element.Display, Is.EqualTo(PomlDisplayType.Visible));
        }
        {
            var display = (int)PomlDisplayType.None;
            api.set_display(memoryPtr, memoryLength, selfObjectDescriptor, display);
            Assert.That(element.Element.Display, Is.EqualTo(PomlDisplayType.None));
        }
        {
            var display = (int)PomlDisplayType.Occlusion;
            api.set_display(memoryPtr, memoryLength, selfObjectDescriptor, display);
            Assert.That(element.Element.Display, Is.EqualTo(PomlDisplayType.Occlusion));
        }
    }

    [Test]
    public async Task GetPositionTest()
    {
        using var mem = new ByteMemory(1024);
        var memoryPtr = mem.Ptr;
        var memoryLength = (uint)mem.Length;

        var poml = @$"
<poml>
    <scene>
        <element id=""element0"" position=""1 2 3"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        {
            var api = CreateApiImpl(pomlComponent, "element0");
            var ptr = 0;
            api.get_position(memoryPtr, memoryLength, selfObjectDescriptor, ptr);
            AssertThatFloatIsWritten(memoryPtr, memoryLength, ptr, 1);
            AssertThatFloatIsWritten(memoryPtr, memoryLength, ptr + 4, 2);
            AssertThatFloatIsWritten(memoryPtr, memoryLength, ptr + 8, 3);
        }
    }

    [Test]
    public async Task SetPositionTest()
    {
        using var mem = new ByteMemory(1024);
        var memoryPtr = mem.Ptr;
        var memoryLength = (uint)mem.Length;

        var poml = @$"
<poml>
    <scene>
        <element id=""element0"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        pomlComponent.TryGetElementById("element0", out var element);
        var elementAttributeImplementation = CreateApiImpl(pomlComponent, "element0");

        {
            var positionPtr = 0;

            MemoryHelper.TryWriteArray(memoryPtr, memoryLength, positionPtr, BitConverter.GetBytes(1f));
            MemoryHelper.TryWriteArray(memoryPtr, memoryLength, positionPtr + 4, BitConverter.GetBytes(2f));
            MemoryHelper.TryWriteArray(memoryPtr, memoryLength, positionPtr + 8, BitConverter.GetBytes(-3f));

            elementAttributeImplementation.set_position(memoryPtr, memoryLength, selfObjectDescriptor, positionPtr);
            Assert.That(element.Element.Position, Is.EqualTo(new UnityEngine.Vector3(1, 2, -3)));
        }
    }

    private void AssertThatDisplayIsWritten(IntPtr memoryPtr, uint memoryLength, int displayPtr, PomlDisplayType expectedDisplay)
    {
        var result = MemoryHelper.TryRead(memoryPtr, memoryLength, displayPtr, out PomlDisplayType display);
        Assert.That(result, Is.True);
        Assert.That(display, Is.EqualTo(expectedDisplay));
    }

    private void AssertThatFloatIsWritten(IntPtr memoryPtr, uint memoryLength, int ptr, float expectedValue)
    {
        var result = MemoryHelper.TryRead(memoryPtr, memoryLength, ptr, out float value);
        Assert.That(result, Is.True);
        Assert.That(value, Is.EqualTo(expectedValue));
    }

    private SpirareApiImpl CreateApiImpl(PomlComponent pomlComponent, string id)
    {
        var result = pomlComponent.TryGetElementById(id, out var element);
        Assert.That(result, Is.True);
        Assert.That(element.Component is PomlObjectElementComponent, Is.True);

        var elementDescriptorHelper = new ElementDescriptorHelper(element.Component as PomlObjectElementComponent, null, pomlComponent);
        var api = new SpirareApiImpl(elementDescriptorHelper);
        return api;
    }

    private async Task<PomlComponent> LoadPomlAsync(string poml)
    {
        var pomlLoader = new PomlLoader(loaderSettings, PomlLoadOptions.DisplayModeType.Normal);
        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, "");

        return pomlComponent;
    }
}
