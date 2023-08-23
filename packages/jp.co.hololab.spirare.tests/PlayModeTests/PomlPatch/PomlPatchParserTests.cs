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
    public void TryParse_ReturnFalseWhenJsonIsInvalid()
    {
        var json = @"{";

        var result = PomlPatchParser.TryParse(json, out var patches);
        Assert.That(result, Is.False);
    }

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

        var result = PomlPatchParser.TryParse(json, out var patches);
        Assert.That(result, Is.True);

        Assert.That(patches.Length, Is.EqualTo(1));
        var patch = patches[0];
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

        var result = PomlPatchParser.TryParse(json, out var patches);
        Assert.That(result, Is.True);

        Assert.That(patches.Length, Is.EqualTo(1));
        var patch = patches[0];
        Assert.That(patch.Operation, Is.EqualTo(PomlPatch.PomlPatchOperation.Update));

        var patchAdd = patch as PomlPatchUpdate;
        Assert.That(patchAdd, Is.Not.Null);
        Assert.That(patchAdd.Target.Tag, Is.EqualTo("scene"));
    }

    [Test]
    public void TryParse_MultiplePatch()
    {
        var json = @"
[
    {
        ""operation"": ""add"",
        ""target"": {
            ""id"": ""test""
        },
        ""tag"": ""model"",
        ""attributes"": {
            ""src"": ""http://example.com/test""
        }
    },
    {
        ""operation"": ""update"",
        ""target"": {
            ""id"": ""test""
        },
    },
]";

        var result = PomlPatchParser.TryParse(json, out var patches);
        Assert.That(result, Is.True);

        Assert.That(patches.Length, Is.EqualTo(2));
        Assert.That(patches[0].Operation, Is.EqualTo(PomlPatch.PomlPatchOperation.Add));
        Assert.That(patches[1].Operation, Is.EqualTo(PomlPatch.PomlPatchOperation.Update));
    }

    [Test]
    public void TryParse_TargetIsNullWhenTargetIsUndefinedInJson()
    {
        var json = @"
{
    ""operation"": ""add"",
    ""tag"": ""model"",
    ""attributes"": {
        ""src"": ""http://example.com/test""
    }
}";

        var result = PomlPatchParser.TryParse(json, out var patches);
        Assert.That(result, Is.True);

        Assert.That(patches.Length, Is.EqualTo(1));
        Assert.That(patches[0].Target, Is.Null);
    }
}
