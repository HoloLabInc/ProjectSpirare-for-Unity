using System.Threading.Tasks;
using HoloLab.Spirare;
using HoloLab.Spirare.Wasm.Core;
using HoloLab.Spirare.Wasm.Core.Spirare;
using HoloLab.Spirare.Wasm.Tests;
using NUnit.Framework;

public class SpecificElementImplementationTests
{
    private PomlLoaderSettings loaderSettings => SpirareTestUtils.DefaultLoaderSettings;

    private const int selfObjectDescriptor = (int)SpecialElementDescriptor.SelfObject;

    [SetUp]
    public void SetUp()
    {
        SpirareTestUtils.CreateMainCamera();
    }

    [Test]
    public async Task GetTextLenTest()
    {
        using var mem = new ByteMemory(1024);
        var memoryPtr = mem.Ptr;
        var memoryLength = (uint)mem.Length;

        var poml = @$"
<poml>
    <scene>
        <text id=""text0"" text=""sample text"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        {
            var api = CreateApiImpl(pomlComponent, "text0");

            var ptr = 0;
            var errorCode = api.get_text_len(memoryPtr, memoryLength, selfObjectDescriptor, ptr);
            Assert.That(errorCode, Is.EqualTo((int)Errno.Success));

            var result = MemoryHelper.TryRead(memoryPtr, memoryLength, ptr, out int length);
            Assert.That(result, Is.True);
            Assert.That(length, Is.EqualTo(11));
        }
    }

    [Test]
    public async Task GetText_BufferSizeIsGreaterThanTextSize_TextAndNullCharactorIsWritten()
    {
        using var mem = ByteMemory.CreateFilledWith(1024, 0xFF);
        var memoryPtr = mem.Ptr;
        var memoryLength = (uint)mem.Length;

        var poml = @$"
<poml>
    <scene>
        <text id=""text0"" text=""sample text"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        {
            var api = CreateApiImpl(pomlComponent, "text0");

            var ptr = 0;
            var errorCode = api.get_text(memoryPtr, memoryLength, selfObjectDescriptor, ptr, 12);
            Assert.That(errorCode, Is.EqualTo((int)Errno.Success));

            {
                var result = MemoryHelper.TryReadUtf8(memoryPtr, memoryLength, ptr, 11, out var text);
                Assert.That(result, Is.True);
                Assert.That(text, Is.EqualTo("sample text"));
            }

            {
                var result = MemoryHelper.TryRead(memoryPtr, memoryLength, 10, out byte value);
                Assert.That(result, Is.True);
                Assert.That(value, Is.EqualTo((byte)'t'));
            }

            {
                var result = MemoryHelper.TryRead(memoryPtr, memoryLength, 11, out byte value);
                Assert.That(result, Is.True);
                Assert.That(value, Is.EqualTo(0));
            }
        }
    }

    [Test]
    public async Task GetText_BufferSizeEqualsTextSize_NullCharactorIsNotWritten()
    {
        using var mem = ByteMemory.CreateFilledWith(1024, 0xFF);
        var memoryPtr = mem.Ptr;
        var memoryLength = (uint)mem.Length;

        var poml = @$"
<poml>
    <scene>
        <text id=""text0"" text=""sample text"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        {
            var api = CreateApiImpl(pomlComponent, "text0");

            var ptr = 0;
            var errorCode = api.get_text(memoryPtr, memoryLength, selfObjectDescriptor, ptr, 11);
            Assert.That(errorCode, Is.EqualTo((int)Errno.Success));

            {
                var result = MemoryHelper.TryReadUtf8(memoryPtr, memoryLength, ptr, 11, out var text);
                Assert.That(result, Is.True);
                Assert.That(text, Is.EqualTo("sample text"));
            }

            {
                var result = MemoryHelper.TryRead(memoryPtr, memoryLength, 10, out byte value);
                Assert.That(result, Is.True);
                Assert.That(value, Is.EqualTo((byte)'t'));
            }

            {
                var result = MemoryHelper.TryRead(memoryPtr, memoryLength, 11, out byte value);
                Assert.That(result, Is.True);
                Assert.That(value, Is.EqualTo(255));
            }
        }
    }

    [Test]
    public async Task GetText_InsufficientBufferSize()
    {
        using var mem = new ByteMemory(1024);
        var memoryPtr = mem.Ptr;
        var memoryLength = (uint)mem.Length;

        var poml = @$"
<poml>
    <scene>
        <text id=""text0"" text=""sample text"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        {
            var api = CreateApiImpl(pomlComponent, "text0");

            var ptr = 0;
            var errorCode = api.get_text(memoryPtr, memoryLength, selfObjectDescriptor, ptr, 10);
            Assert.That(errorCode, Is.EqualTo((int)Errno.InsufficientBufferSize));
        }
    }

    [Test]
    public async Task SetText_Success()
    {
        using var mem = new ByteMemory(1024);
        var memoryPtr = mem.Ptr;
        var memoryLength = (uint)mem.Length;

        var poml = @$"
<poml>
    <scene>
        <text id=""text0"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        pomlComponent.TryGetElementById("text0", out var element);
        var api = CreateApiImpl(pomlComponent, "text0");

        {
            var textPtr = 0;
            uint writePtr = 0;
            var text = "sample text";

            MemoryHelper.TryWriteUtf8(memoryPtr, memoryLength, text, ref writePtr);
            var errorCode = api.set_text(memoryPtr, memoryLength, selfObjectDescriptor, textPtr, text.Length);
            Assert.That(errorCode, Is.EqualTo((int)Errno.Success));

            var textElement = element.Element as PomlTextElement;
            Assert.That(textElement.Text, Is.EqualTo(text));
        }
    }

    [Test]
    public async Task SetText_InvalidElementType()
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
        var api = CreateApiImpl(pomlComponent, "element0");

        {
            var textPtr = 0;
            uint writePtr = 0;
            var text = "sample text";

            MemoryHelper.TryWriteUtf8(memoryPtr, memoryLength, text, ref writePtr);
            var errorCode = api.set_text(memoryPtr, memoryLength, selfObjectDescriptor, textPtr, text.Length);
            Assert.That(errorCode, Is.EqualTo((int)Errno.UnsupportedOperation));
        }
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
