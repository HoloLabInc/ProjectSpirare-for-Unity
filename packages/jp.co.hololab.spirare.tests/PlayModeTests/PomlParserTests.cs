using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloLab.Spirare;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PomlParserTests
{
    [Test]
    public void EmptyPoml()
    {
        var xml = @"
<poml>
</poml>";

        var elements = ParseSceneElements(xml);
        Assert.That(elements.Length, Is.EqualTo(0));
    }

    [Test]
    public void SceneTag()
    {
        var xml = @"
<poml>
    <scene id=""test-id""
           ws-recv-url=""wss://example.com:8000"">
    </scene>
</poml>";

        var result = PomlParser.TryParse(xml, "", out var poml);
        Assert.That(result, Is.True);

        var scene = poml.Scene;
        Assert.That(scene.Children.Count, Is.EqualTo(0));

        Assert.That(scene.ElementType, Is.EqualTo(PomlElementType.Scene));
        Assert.That(scene.WsRecvUrl, Is.EqualTo("wss://example.com:8000"));
    }

    [Test]
    public void ElementTag()
    {
        var xml = @"
<poml>
    <scene>
        <element position=""10,20,30""
             scale=""2""
             rotation=""0,0,0,1""
             scale-by-distance=""true""
             min-scale=""1.1""
             max-scale=""3""
             rotation-mode=""vertical-billboard""
             ar-display=""visible""
             id=""test-id""
             web-link=""https://example.com""
             ws-recv-url=""wss://example.com:8000"" >
        </element>
    </scene>
</poml>";

        var elements = ParseSceneElements(xml);
        Assert.That(elements.Length, Is.EqualTo(1));

        var element = elements[0];

        Assert.That(element.ElementType, Is.EqualTo(PomlElementType.Element));
        Assert.That(element.Position, Is.EqualTo(new Vector3(10, 20, 30)));
        Assert.That(element.Rotation, Is.EqualTo(new Quaternion(0, 0, 0, 1)));

        Assert.That(element.ScaleByDistance, Is.EqualTo(1));
        Assert.That(element.MinScale, Is.EqualTo(1.1f * Vector3.one));
        Assert.That(element.MaxScale, Is.EqualTo(3f * Vector3.one));

        Assert.That(element.RotationMode, Is.EqualTo(PomlRotationMode.VerticalBillboard));
        Assert.That(element.ArDisplay, Is.EqualTo(PomlArDisplayType.Visible));

        Assert.That(element.Id, Is.EqualTo("test-id"));
        Assert.That(element.WebLink, Is.EqualTo("https://example.com"));
        Assert.That(element.WsRecvUrl, Is.EqualTo("wss://example.com:8000"));
    }

    [Test]
    public void DisplayAttribute()
    {
        var xml = @"
<poml>
    <scene>
        <element>
        </element>
        <element display=""visible"">
        </element>
        <element display=""none"">
        </element>
        <element display=""occlusion"">
        </element>
    </scene>
</poml>";

        var elements = ParseSceneElements(xml);

        Assert.That(elements[0].Display, Is.EqualTo(PomlDisplayType.Visible));
        Assert.That(elements[1].Display, Is.EqualTo(PomlDisplayType.Visible));
        Assert.That(elements[2].Display, Is.EqualTo(PomlDisplayType.None));
        Assert.That(elements[3].Display, Is.EqualTo(PomlDisplayType.Occlusion));
    }

    [Test]
    public void ArDisplayAttribute()
    {
        var xml = @"
<poml>
    <scene>
        <element>
        </element>
        <element ar-display=""same-as-display"">
        </element>
        <element ar-display=""visible"">
        </element>
        <element ar-display=""none"">
        </element>
        <element ar-display=""occlusion"">
        </element>
    </scene>
</poml>";

        var elements = ParseSceneElements(xml);

        Assert.That(elements[0].ArDisplay, Is.EqualTo(PomlArDisplayType.SameAsDisplay));
        Assert.That(elements[1].ArDisplay, Is.EqualTo(PomlArDisplayType.SameAsDisplay));
        Assert.That(elements[2].ArDisplay, Is.EqualTo(PomlArDisplayType.Visible));
        Assert.That(elements[3].ArDisplay, Is.EqualTo(PomlArDisplayType.None));
        Assert.That(elements[4].ArDisplay, Is.EqualTo(PomlArDisplayType.Occlusion));
    }

    [Test]
    public void CustomAttributes()
    {
        var xml = @"
<poml>
    <scene>
        <element id=""test"" _custom-key1=""value1"" _custom-key2=""value2"">
        </element>
    </scene>
</poml>";

        var elements = ParseSceneElements(xml);
        var element = elements[0];

        Assert.That(element.CustomAttributes.Count, Is.EqualTo(2));
        Assert.That(element.CustomAttributes["_custom-key1"], Is.EqualTo("value1"));
        Assert.That(element.CustomAttributes["_custom-key2"], Is.EqualTo("value2"));
    }

    [Test]
    public void ElementHierarchy()
    {
        var xml = @"
<poml>
    <scene>
        <element id=""parent"">
            <element id=""child"">
            </element>
        </element>
    </scene>
</poml>";

        var elements = ParseSceneElements(xml);

        Assert.That(elements.Count, Is.EqualTo(1));

        var parent = elements[0];
        Assert.That(parent.Children.Count, Is.EqualTo(1));
        Assert.That(parent.Parent.ElementType, Is.EqualTo(PomlElementType.Scene));

        var child = parent.Children.First();
        Assert.That(child.Parent, Is.EqualTo(parent));
        Assert.That(child.Children.Count, Is.EqualTo(0));
    }

    [Test]
    public void ModelTag()
    {
        var xml = @"
<poml>
    <scene>
        <model src=""https://example.com/model0"">
        </model>
        <model src=""model1"">
        </model>
        <model src=""./model2"">
        </model>
    </scene>
</poml>";

        var basePath = "https://example.net/poml";
        var elements = ParseSceneElements(xml, basePath);
        Assert.That(elements.Length, Is.EqualTo(3));

        var element0 = elements[0];
        Assert.That(element0.ElementType, Is.EqualTo(PomlElementType.Model));
        Assert.That(element0.Src, Is.EqualTo("https://example.com/model0"));

        var element1 = elements[1];
        Assert.That(element1.ElementType, Is.EqualTo(PomlElementType.Model));
        Assert.That(element1.Src, Is.EqualTo("https://example.net/model1"));

        var element2 = elements[2];
        Assert.That(element2.ElementType, Is.EqualTo(PomlElementType.Model));
        Assert.That(element2.Src, Is.EqualTo("https://example.net/model2"));
    }

    [Test]
    public void ImageTag()
    {
        var xml = @"
<poml>
    <scene>
        <image src=""https://example.com/image0"">
        </image>
        <image src=""image1"" backface=""solid"" backface-color=""red"">
        </image>
        <image src=""./image2"" backface=""visible"">
        </image>
        <image src=""./image3"" backface=""flipped"">
        </image>
    </scene>
</poml>";

        var basePath = "https://example.net/poml";
        var elements = ParseSceneElements(xml, basePath);
        Assert.That(elements.Length, Is.EqualTo(4));

        var element0 = elements[0] as PomlImageElement;
        Assert.That(element0.ElementType, Is.EqualTo(PomlElementType.Image));
        Assert.That(element0.Src, Is.EqualTo("https://example.com/image0"));
        Assert.That(element0.Backface, Is.EqualTo(PomlBackfaceType.None));
        Assert.That(element0.BackfaceColor, Is.EqualTo(Color.white));

        var element1 = elements[1] as PomlImageElement;
        Assert.That(element1.ElementType, Is.EqualTo(PomlElementType.Image));
        Assert.That(element1.Src, Is.EqualTo("https://example.net/image1"));
        Assert.That(element1.Backface, Is.EqualTo(PomlBackfaceType.Solid));
        Assert.That(element1.BackfaceColor, Is.EqualTo(Color.red));

        var element2 = elements[2] as PomlImageElement;
        Assert.That(element2.ElementType, Is.EqualTo(PomlElementType.Image));
        Assert.That(element2.Src, Is.EqualTo("https://example.net/image2"));
        Assert.That(element2.Backface, Is.EqualTo(PomlBackfaceType.Visible));

        var element3 = elements[3] as PomlImageElement;
        Assert.That(element3.ElementType, Is.EqualTo(PomlElementType.Image));
        Assert.That(element3.Src, Is.EqualTo("https://example.net/image3"));
        Assert.That(element3.Backface, Is.EqualTo(PomlBackfaceType.Flipped));
    }

    [Test]
    public void VideoTag()
    {
        var xml = @"
<poml>
    <scene>
        <video src=""https://example.com/video0"">
        </video>
        <video src=""video1"">
        </video>
        <video src=""./video2"">
        </video>
    </scene>
</poml>";

        var basePath = "https://example.net/poml";
        var elements = ParseSceneElements(xml, basePath);
        Assert.That(elements.Length, Is.EqualTo(3));

        var element0 = elements[0];
        Assert.That(element0.ElementType, Is.EqualTo(PomlElementType.Video));
        Assert.That(element0.Src, Is.EqualTo("https://example.com/video0"));

        var element1 = elements[1];
        Assert.That(element1.ElementType, Is.EqualTo(PomlElementType.Video));
        Assert.That(element1.Src, Is.EqualTo("https://example.net/video1"));

        var element2 = elements[2];
        Assert.That(element2.ElementType, Is.EqualTo(PomlElementType.Video));
        Assert.That(element2.Src, Is.EqualTo("https://example.net/video2"));
    }

    [Test]
    public void TextTag()
    {
        var xml = @"
<poml>
    <scene>
        <text text=""text1"" font-size=""2m"" 
            font-color=""blue"" background-color=""red"">
        </text>
        <text text=""text1""
            font-color=""#123456"" background-color=""#234567"">
        </text>
    </scene>
</poml>";

        var elements = ParseSceneElements(xml);
        Assert.That(elements.Length, Is.EqualTo(2));

        var element0 = elements[0] as PomlTextElement;
        Assert.That(element0.ElementType, Is.EqualTo(PomlElementType.Text));
        Assert.That(element0.Text, Is.EqualTo("text1"));
        Assert.That(element0.FontSize, Is.EqualTo("2m"));
        Assert.That(element0.FontColor, Is.EqualTo(Color.blue));
        Assert.That(element0.BackgroundColor, Is.EqualTo(Color.red));

        var element1 = elements[1] as PomlTextElement;
        Assert.That(element1.ElementType, Is.EqualTo(PomlElementType.Text));
        Assert.That(element1.FontColor, Is.EqualTo((Color)new Color32(0x12, 0x34, 0x56, 0xFF)));
        Assert.That(element1.BackgroundColor, Is.EqualTo((Color)new Color32(0x23, 0x45, 0x67, 0xFF)));
    }

    [Test]
    public void GeometryTag_Line()
    {
        var xml = @"
<poml>
    <scene>
        <geometry>
            <line />
        </geometry>
        <geometry>
            <line vertices=""geodetic: 1 2 3 4 5 6"" color=""white"" width=""0.1"" />
        </geometry>
        <geometry position-type=""relative"">
            <line vertices=""1 2 3 4 5 6"" color=""white"" width=""10"" />
        </geometry>
    </scene>
</poml>";

        var elements = ParseSceneElements(xml);
        Assert.That(elements.Length, Is.EqualTo(3));

        {
            var element = elements[0] as PomlGeometryElement;
            Assert.That(element.ElementType, Is.EqualTo(PomlElementType.Geometry));

            var geometries = element.Geometries;
            Assert.That(geometries.Count, Is.EqualTo(1));

            var geometry = geometries[0];
            Assert.That(geometry.Type, Is.EqualTo(PomlGeometryType.Line));

            var line = geometry as LineGeometry;
            Assert.That(line.Width, Is.EqualTo(0f));
        }
        {
            var element = elements[1] as PomlGeometryElement;
            Assert.That(element.ElementType, Is.EqualTo(PomlElementType.Geometry));

            var geometries = element.Geometries;
            Assert.That(geometries.Count, Is.EqualTo(1));

            var geometry = geometries[0];
            Assert.That(geometry.Type, Is.EqualTo(PomlGeometryType.Line));

            var line = geometry as LineGeometry;
            Assert.That(line.Width, Is.EqualTo(0.1f));

            Assert.That(line.Vertices, Is.EqualTo("geodetic: 1 2 3 4 5 6"));
        }
        {
            var element = elements[2] as PomlGeometryElement;
            Assert.That(element.ElementType, Is.EqualTo(PomlElementType.Geometry));

            var geometries = element.Geometries;
            Assert.That(geometries.Count, Is.EqualTo(1));

            var geometry = geometries[0];
            Assert.That(geometry.Type, Is.EqualTo(PomlGeometryType.Line));

            var line = geometry as LineGeometry;
            Assert.That(line.Vertices, Is.EqualTo("1 2 3 4 5 6"));
            Assert.That(line.Width, Is.EqualTo(10f));
        }
    }


    [Test]
    public void GeometryTag_Polygon()
    {
        var xml = @"
<poml>
    <scene>
        <geometry>
            <polygon />
        </geometry>
        <geometry>
            <polygon vertices=""geodetic: 0 1 2 3 4 5 6 7 8 9 10 11"" indices=""0,1,2,1,2,3"" color=""red"" />
        </geometry>
        <geometry>
            <polygon vertices=""0 1 2 3 4 5 6 7 8"" indices=""0,1,2"" color=""white"" />
        </geometry>
    </scene>
</poml>";

        var elements = ParseSceneElements(xml);
        Assert.That(elements.Length, Is.EqualTo(3));

        {
            var element = elements[0] as PomlGeometryElement;
            Assert.That(element.ElementType, Is.EqualTo(PomlElementType.Geometry));

            var geometries = element.Geometries;
            Assert.That(geometries.Count, Is.EqualTo(1));

            var geometry = geometries[0];
            Assert.That(geometry.Type, Is.EqualTo(PomlGeometryType.Polygon));

            var polygon = geometry as PolygonGeometry;
            Assert.That(polygon.Vertices.Count, Is.EqualTo(0));
        }
        {
            var element = elements[1] as PomlGeometryElement;
            Assert.That(element.ElementType, Is.EqualTo(PomlElementType.Geometry));

            var geometries = element.Geometries;
            Assert.That(geometries.Count, Is.EqualTo(1));

            var geometry = geometries[0];
            Assert.That(geometry.Type, Is.EqualTo(PomlGeometryType.Polygon));

            var polygon = geometry as PolygonGeometry;
            Assert.That(polygon.Color, Is.EqualTo(Color.red));
            Assert.That(polygon.Vertices, Is.EqualTo("geodetic: 0 1 2 3 4 5 6 7 8 9 10 11"));

            Assert.That(polygon.Indices.Count, Is.EqualTo(6));
            Assert.That(polygon.Indices, Is.EquivalentTo(new int[] { 0, 1, 2, 1, 2, 3 }));
        }
        {
            var element = elements[2] as PomlGeometryElement;
            Assert.That(element.ElementType, Is.EqualTo(PomlElementType.Geometry));

            var geometries = element.Geometries;
            Assert.That(geometries.Count, Is.EqualTo(1));

            var geometry = geometries[0];
            Assert.That(geometry.Type, Is.EqualTo(PomlGeometryType.Polygon));

            var polygon = geometry as PolygonGeometry;
            Assert.That(polygon.Color, Is.EqualTo(Color.white));
            Assert.That(polygon.Vertices, Is.EqualTo("0 1 2 3 4 5 6 7 8"));

            Assert.That(polygon.Indices.Count, Is.EqualTo(3));
            Assert.That(polygon.Indices, Is.EquivalentTo(new int[] { 0, 1, 2 }));
        }
    }

    [Test]
    public void NestedElements()
    {
        var xml = @"
<poml>
    <scene>
        <element>
            <text text=""text1""></text>
        </element>
        <element>
            <element>
                <model src=""model1""></model>
            </element>
        </element>
    </scene>
</poml>";

        var basePath = "https://example.net/poml";
        var elements = ParseSceneElements(xml, basePath);
        Assert.That(elements.Length, Is.EqualTo(2));

        var element0 = elements[0];
        Assert.That(element0.ElementType, Is.EqualTo(PomlElementType.Element));

        var textElement00 = element0.Children.ToArray()[0] as PomlTextElement;
        Assert.That(textElement00.ElementType, Is.EqualTo(PomlElementType.Text));
        Assert.That(textElement00.Text, Is.EqualTo("text1"));

        var element1 = elements[1];
        Assert.That(element1.ElementType, Is.EqualTo(PomlElementType.Element));

        var element10 = element1.Children.ToArray()[0];
        Assert.That(element10.ElementType, Is.EqualTo(PomlElementType.Element));

        var element100 = element10.Children.ToArray()[0] as PomlModelElement;
        Assert.That(element100.ElementType, Is.EqualTo(PomlElementType.Model));
        Assert.That(element100.Src, Is.EqualTo("https://example.net/model1"));
    }

    [Test]
    public void SpaceReferenceTag()
    {
        var xml = @"
<poml>
    <scene>
        <element>
            <space-reference id=""my-reference""
                space-type=""space-type-0""
                space-id=""space-id-0"">
            </space-reference>
        </element>
    </scene>
</poml>";


        var elements = ParseSceneElements(xml);
        Assert.That(elements.Length, Is.EqualTo(1));

        var element0 = elements[0];
        var spaceReference = element0.Children.ToArray()[0] as PomlSpaceReferenceElement;
        Assert.That(spaceReference.ElementType, Is.EqualTo(PomlElementType.SpaceReference));
        Assert.That(spaceReference.Id, Is.EqualTo("my-reference"));
        Assert.That(spaceReference.SpaceType, Is.EqualTo("space-type-0"));
        Assert.That(spaceReference.SpaceId, Is.EqualTo("space-id-0"));
    }

    [Test]
    public void SpacePlacementTag()
    {
        var xml = @"
<poml>
    <scene>
        <element>
            <space-placement id=""my-placement""
                space-type=""space-type-0""
                space-id=""space-id-0"">
            </space-placement>
        </element>
    </scene>
</poml>";


        var elements = ParseSceneElements(xml);
        Assert.That(elements.Length, Is.EqualTo(1));

        var element0 = elements[0];
        var spaceReference = element0.Children.ToArray()[0] as PomlSpaceReferenceElement;
        Assert.That(spaceReference.ElementType, Is.EqualTo(PomlElementType.SpaceReference));
        Assert.That(spaceReference.Id, Is.EqualTo("my-placement"));
        Assert.That(spaceReference.SpaceType, Is.EqualTo("space-type-0"));
        Assert.That(spaceReference.SpaceId, Is.EqualTo("space-id-0"));
    }

    [Test]
    public void GeoReferenceTag()
    {
        var xml = @"
<poml>
    <scene>
        <element>
            <geo-reference id=""my-reference""
                latitude=""35.68122469808435""
                longitude=""139.76719554151146""
                ellipsoidal-height=""50""
                enu-rotation=""0,0,0,1"" >
            </geo-reference>
        </element>
    </scene>
</poml>";


        var elements = ParseSceneElements(xml);
        Assert.That(elements.Length, Is.EqualTo(1));

        var element0 = elements[0];
        var geoReference = element0.Children.ToArray()[0] as PomlGeoReferenceElement;
        Assert.That(geoReference.ElementType, Is.EqualTo(PomlElementType.GeoReference));
        Assert.That(geoReference.Id, Is.EqualTo("my-reference"));

        Assert.That(geoReference.Latitude, Is.EqualTo(35.68122469808435));
        Assert.That(geoReference.Longitude, Is.EqualTo(139.76719554151146));
        Assert.That(geoReference.EllipsoidalHeight, Is.EqualTo(50));
        Assert.That(geoReference.EnuRotation, Is.EqualTo(new Quaternion(0, 0, 0, 1)));
    }

    [Test]
    public void GeoPlacementTag()
    {
        var xml = @"
<poml>
    <scene>
        <element>
            <geo-placement id=""my-placement""
                latitude=""35.68122469808435""
                longitude=""139.76719554151146""
                ellipsoidal-height=""50""
                enu-rotation=""0,0,0,1"" >
            </geo-placement>
        </element>
    </scene>
</poml>";


        var elements = ParseSceneElements(xml);
        Assert.That(elements.Length, Is.EqualTo(1));

        var element0 = elements[0];
        var geoReference = element0.Children.ToArray()[0] as PomlGeoReferenceElement;
        Assert.That(geoReference.ElementType, Is.EqualTo(PomlElementType.GeoReference));
        Assert.That(geoReference.Id, Is.EqualTo("my-placement"));

        Assert.That(geoReference.Latitude, Is.EqualTo(35.68122469808435));
        Assert.That(geoReference.Longitude, Is.EqualTo(139.76719554151146));
        Assert.That(geoReference.EllipsoidalHeight, Is.EqualTo(50));
        Assert.That(geoReference.EnuRotation, Is.EqualTo(new Quaternion(0, 0, 0, 1)));
    }

    [Test]
    public void ScriptTag()
    {
        var xml = @"
<poml>
    <scene>
        <script src=""https://example.com/test.wasm"" args=""arg1 arg2"">
        </script>
        <script src=""test2.wasm"">
        </script>
        <script src=""./test3.wasm"">
        </script>
    </scene>
</poml>";

        var basePath = "https://example.net/poml";
        var elements = ParseSceneElements(xml, basePath);
        Assert.That(elements.Length, Is.EqualTo(3));

        var element0 = elements[0];
        Assert.That(element0.ElementType, Is.EqualTo(PomlElementType.Script));
        var scriptElement0 = element0 as PomlScriptElement;
        Assert.That(scriptElement0.Src, Is.EqualTo("https://example.com/test.wasm"));

        var args0 = scriptElement0.Args;
        Assert.That(args0.Count, Is.EqualTo(2));
        Assert.That(args0[0], Is.EqualTo("arg1"));
        Assert.That(args0[1], Is.EqualTo("arg2"));

        var element1 = elements[1];
        Assert.That(element1.ElementType, Is.EqualTo(PomlElementType.Script));
        Assert.That(element1.Src, Is.EqualTo("https://example.net/test2.wasm"));

        var element2 = elements[2];
        Assert.That(element2.ElementType, Is.EqualTo(PomlElementType.Script));
        Assert.That(element2.Src, Is.EqualTo("https://example.net/test3.wasm"));
    }

    private PomlElement[] ParseSceneElements(string xml, string basePath = "")
    {
        var result = PomlParser.TryParse(xml, basePath, out var poml);

        Assert.That(result, Is.True);
        var elements = poml.Scene.Children.ToArray();

        return elements;
    }

    private static void AssertPomlGeodticPosition(PomlGeodeticPosition position, double longitude, double latitude, double ellipsoidalHeight)
    {
        Assert.That(position.Longitude, Is.EqualTo(longitude));
        Assert.That(position.Latitude, Is.EqualTo(latitude));
        Assert.That(position.EllipsoidalHeight, Is.EqualTo(ellipsoidalHeight));
    }
}
