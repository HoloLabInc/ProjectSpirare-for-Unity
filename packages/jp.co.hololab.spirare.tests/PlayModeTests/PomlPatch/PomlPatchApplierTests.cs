using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        var applier = new PomlPatchApplier(pomlComponent, null, "");
        applier.ApplyPomlPatch(pomlPatch);

        pomlComponent.TryGetElementById("text1", out var element);
        var pomlElement = element.Element as PomlTextElement;
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

        var applier = new PomlPatchApplier(pomlComponent, null, "");
        applier.ApplyPomlPatch(pomlPatch);

        pomlComponent.TryGetElementById("model0", out var element);
        var pomlModelElement = element.Element as PomlModelElement;
        Assert.That(pomlModelElement.Src, Is.EqualTo("http://example.com/test.glb"));
        Assert.That(pomlModelElement.Position, Is.EqualTo(new Vector3(1, 2, 3)));

        Assert.That(element.Component.gameObject, Is.Not.Null);

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

        var applier = new PomlPatchApplier(pomlComponent, null, "");
        applier.ApplyPomlPatch(pomlPatch);

        pomlComponent.TryGetElementById("model0", out var element);
        var pomlModelElement = element.Element as PomlModelElement;
        Assert.That(pomlModelElement.Src, Is.EqualTo("http://example.com/test.glb"));
        Assert.That(pomlModelElement.Position, Is.EqualTo(new Vector3(1, 2, 3)));

        Assert.That(element.Component.gameObject, Is.Not.Null);

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

        var applier = new PomlPatchApplier(pomlComponent, null, "");
        applier.ApplyPomlPatch(pomlPatch);

        pomlComponent.TryGetElementById("model0", out var element);
        var pomlModelElement = element.Element as PomlModelElement;
        Assert.That(pomlModelElement.Src, Is.EqualTo("http://example.com/test.glb"));
        Assert.That(pomlModelElement.Position, Is.EqualTo(new Vector3(1, 2, 3)));

        Assert.That(element.Component.gameObject, Is.Not.Null);

        var parentTransform = element.Component.transform.parent;
        var parentComponent = parentTransform.GetComponentInParent<PomlElementComponent>();
        Assert.That(parentComponent.PomlElement.Id, Is.EqualTo("text1"));

        Object.Destroy(pomlComponent.gameObject);
    }



    private async Task<PomlComponent> LoadPomlAsync(string poml)
    {
        var pomlLoader = new PomlLoader(loaderSettings, PomlLoadOptions.DisplayModeType.Normal);
        return await pomlLoader.LoadXmlAsync(poml, "");
    }
}

