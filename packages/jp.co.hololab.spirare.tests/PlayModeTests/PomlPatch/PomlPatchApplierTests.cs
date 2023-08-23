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
    public async Task TryParse_PatchUpdate()
    {
        var poml = @"
<poml>
    <scene>
        <text id=""text0"" text=""text0"" />
        <text id=""text1"" text=""text1"" />
    </scene>
</poml> ";

        var pomlLoader = new PomlLoader(loaderSettings, PomlLoadOptions.DisplayModeType.Normal);
        var pomlComponent = await pomlLoader.LoadXmlAsync(poml, "");

        var pomlPatch = @"
{
    ""operation"": ""update"",
    ""target"": {
        ""id"": ""text1""
    },
    ""attributes"": {
        ""text"": ""updated_text""
        ""positions"": {
            ""x"": 1,
            ""y"": 2,
            ""z"": 3
        }
    }
}";

        var applier = new PomlPatchApplier(pomlComponent);
        applier.ApplyPomlPatch(pomlPatch);

        pomlComponent.TryGetElementById("text1", out var element);
        var pomlElement = element.Element as PomlTextElement;
        Assert.That(pomlElement.Text, Is.EqualTo("updated_text"));
        Assert.That(pomlElement.Position, Is.EqualTo(new Vector3(1, 2, 3)));

        Object.Destroy(pomlComponent.gameObject);
    }
}
