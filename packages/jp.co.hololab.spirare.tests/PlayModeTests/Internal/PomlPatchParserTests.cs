using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PomlPatchParserTests
{
    [Test]
    public void TryParse_PatchAdd()
    {
        var json = @"
{
    ""operation"": ""add"",
    ""target"": {
        ""id"": ""test""
    },
    ""tag"": ""model"",
    ""attributes"": {
        ""src"": ""http://example.com/test""
    }
}";

        var result = PomlPatchParser.TryParse(json, out var patch);
        Assert.That(result, Is.True);

        Assert.That(patch.Operation, Is.EqualTo(PomlPatch.PomlPatchOperation.Add));

        var patchAdd = patch as PomlPatchAdd;
        Assert.That(patchAdd, Is.Not.Null);
        Assert.That(patchAdd.Target.Id, Is.EqualTo("test"));

        Assert.That(patchAdd.Element, Is.Not.Null);
        Assert.That(patchAdd.Element.Tag, Is.EqualTo("model"));
    }

    [Test]
    public void TryParse_PatchUpdate()
    {
        var json = @"
{
    ""operation"": ""update"",
    ""target"": {
        ""tag"": ""scene""
    },
    ""attributes"": {
        ""src"": ""http://example.com/test""
    }
}";

        var result = PomlPatchParser.TryParse(json, out var patch);
        Assert.That(result, Is.True);

        Assert.That(patch.Operation, Is.EqualTo(PomlPatch.PomlPatchOperation.Update));

        var patchAdd = patch as PomlPatchUpdate;
        Assert.That(patchAdd, Is.Not.Null);
        Assert.That(patchAdd.Target.Id, Is.EqualTo("test"));
    }
}
