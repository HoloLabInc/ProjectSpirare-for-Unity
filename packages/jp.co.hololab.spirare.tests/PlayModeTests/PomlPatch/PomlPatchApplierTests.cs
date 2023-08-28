using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PomlPatchApplierTests
{
    private PomlLoaderSettings loaderSettings => SpirareTestUtils.DefaultLoaderSettings;

    [Test]
    public async Task ApplyPomlPatch_UpdateTextElement()
    {
        var poml = @"
<poml>
    <scene>
        <text id=""text0"" text=""text0"" />
        <text id=""text1"" text=""text1"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        var pomlPatch = @"
{
    ""operation"": ""update"",
    ""target"": {
        ""id"": ""text1""
    },
    ""attributes"": {
        ""text"": ""updated_text"",
        ""position"": {
            ""x"": 1,
            ""y"": 2,
            ""z"": 3
        }
    }
}";

        var applier = new PomlPatchApplier(pomlComponent, null, null, "");
        await applier.ApplyPomlPatchAsync(pomlPatch);

        pomlComponent.TryGetPomlElementComponentById("text1", out var elementComponent);
        var pomlElement = elementComponent.PomlElement as PomlTextElement;
        Assert.That(pomlElement.Text, Is.EqualTo("updated_text"));
        Assert.That(pomlElement.Position, Is.EqualTo(new Vector3(1, 2, 3)));

        Object.Destroy(pomlComponent.gameObject);
    }

    [Test]
    public async Task ApplyPomlPatch_AddElementToScene()
    {
        var poml = @"
<poml>
    <scene>
        <text id=""text0"" text=""text0"" />
        <text id=""text1"" text=""text1"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        var pomlPatch = @"
{
    ""operation"": ""add"",
    ""target"": {
        ""tag"": ""scene""
    },
    ""tag"": ""model"",
    ""attributes"": {
        ""id"": ""model0"",
        ""src"": ""http://example.com/test.glb"",
        ""position"": {
            ""x"": 1,
            ""y"": 2,
            ""z"": 3
        }
    }
}";

        var applier = new PomlPatchApplier(pomlComponent, null, null, "");
        await applier.ApplyPomlPatchAsync(pomlPatch);

        pomlComponent.TryGetPomlElementComponentById("model0", out var elementComponent);
        var pomlModelElement = elementComponent.PomlElement as PomlModelElement;
        Assert.That(pomlModelElement.Src, Is.EqualTo("http://example.com/test.glb"));
        Assert.That(pomlModelElement.Position, Is.EqualTo(new Vector3(1, 2, 3)));

        Assert.That(elementComponent.gameObject, Is.Not.Null);

        Object.Destroy(pomlComponent.gameObject);
    }

    [Test]
    public async Task ApplyPomlPatch_AddNestedElementToScene()
    {
        var poml = @"
<poml>
    <scene>
        <text id=""text0"" text=""text0"" />
        <text id=""text1"" text=""text1"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        var pomlPatch = @"
{
    ""operation"": ""add"",
    ""target"": {
        ""tag"": ""scene""
    },
    ""tag"": ""element"",
    ""children"": [
        {
        ""tag"" : ""model"",
        ""attributes"":
            {
                ""id"": ""model0"",
                ""src"": ""http://example.com/test.glb"",
                ""position"": {
                    ""x"": 1,
                    ""y"": 2,
                    ""z"": 3
                }
            }
        }       
    ]   
}";

        var applier = new PomlPatchApplier(pomlComponent, null, null, "");
        await applier.ApplyPomlPatchAsync(pomlPatch);

        pomlComponent.TryGetPomlElementComponentById("model0", out var elementComponent);
        var pomlModelElement = elementComponent.PomlElement as PomlModelElement;
        Assert.That(pomlModelElement.Src, Is.EqualTo("http://example.com/test.glb"));
        Assert.That(pomlModelElement.Position, Is.EqualTo(new Vector3(1, 2, 3)));

        Assert.That(elementComponent.gameObject, Is.Not.Null);

        Object.Destroy(pomlComponent.gameObject);
    }

    [Test]
    public async Task ApplyPomlPatch_AddElementToElement()
    {
        var poml = @"
<poml>
    <scene>
        <text id=""text0"" text=""text0"" />
        <text id=""text1"" text=""text1"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);

        var pomlPatch = @"
{
    ""operation"": ""add"",
    ""target"": {
        ""id"": ""text1""
    },
    ""tag"": ""model"",
    ""attributes"": {
        ""id"": ""model0"",
        ""src"": ""http://example.com/test.glb"",
        ""position"": {
            ""x"": 1,
            ""y"": 2,
            ""z"": 3
        }
    }
}";

        var applier = new PomlPatchApplier(pomlComponent, null, null, "");
        await applier.ApplyPomlPatchAsync(pomlPatch);

        pomlComponent.TryGetPomlElementComponentById("model0", out var elementComponent);
        var pomlModelElement = elementComponent.PomlElement as PomlModelElement;
        Assert.That(pomlModelElement.Src, Is.EqualTo("http://example.com/test.glb"));
        Assert.That(pomlModelElement.Position, Is.EqualTo(new Vector3(1, 2, 3)));

        Assert.That(elementComponent.gameObject, Is.Not.Null);

        var parentTransform = elementComponent.transform.parent;
        var parentComponent = parentTransform.GetComponentInParent<PomlElementComponent>();
        Assert.That(parentComponent.PomlElement.Id, Is.EqualTo("text1"));

        Object.Destroy(pomlComponent.gameObject);
    }

    [Test]
    public async Task ApplyPomlPatch_RemoveElement()
    {
        var poml = @"
<poml>
    <scene>
        <text id=""text0"" text=""text0"" />
        <text id=""text1"" text=""text1"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);
        Assert.That(pomlComponent.transform.childCount, Is.EqualTo(2));

        var pomlPatch = @"
{
    ""operation"": ""remove"",
    ""target"": {
        ""id"": ""text1""
    }
}";

        var applier = new PomlPatchApplier(pomlComponent, null, null, "");
        await applier.ApplyPomlPatchAsync(pomlPatch);

        // Wait until destroy completed
        await UniTask.DelayFrame(1);

        Assert.That(pomlComponent.transform.childCount, Is.EqualTo(1));
        var result = pomlComponent.TryGetPomlElementComponentById("text1", out _);
        Assert.That(result, Is.False);

        Object.Destroy(pomlComponent.gameObject);
    }

    [Test]
    public async Task ApplyPomlPatch_RemoveElementMultipleTimes()
    {
        var poml = @"
<poml>
    <scene>
        <text id=""text0"" text=""text0"" />
        <text id=""text1"" text=""text1"" />
    </scene>
</poml> ";

        var pomlComponent = await LoadPomlAsync(poml);
        Assert.That(pomlComponent.transform.childCount, Is.EqualTo(2));

        var pomlPatch = @"
{
    ""operation"": ""remove"",
    ""target"": {
        ""tag"": ""text""
    }
}";

        var applier = new PomlPatchApplier(pomlComponent, null, null, "");
        await applier.ApplyPomlPatchAsync(pomlPatch);
        await applier.ApplyPomlPatchAsync(pomlPatch);

        // Wait until destroy completed
        await UniTask.DelayFrame(1);

        Assert.That(pomlComponent.transform.childCount, Is.EqualTo(0));

        var result0 = pomlComponent.TryGetPomlElementComponentById("text0", out _);
        Assert.That(result0, Is.False);

        var result1 = pomlComponent.TryGetPomlElementComponentById("text1", out _);
        Assert.That(result1, Is.False);

        Object.Destroy(pomlComponent.gameObject);
    }



    private async Task<PomlComponent> LoadPomlAsync(string poml)
    {
        var pomlLoader = new PomlLoader(loaderSettings, PomlLoadOptions.DisplayModeType.Normal);
        return await pomlLoader.LoadXmlAsync(poml, "");
    }
}

