using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Debug = UnityEngine.Debug;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Color = UnityEngine.Color;

namespace HoloLab.Spirare
{
    internal static class PomlParser
    {
        public static bool TryParse(string xml, string basePath, out Poml poml)
        {
            // To run this method in the background, do not use GameObject or Transform.
            try
            {
                poml = ParseXml(xml, basePath);
                return true;
            }
            catch (XmlException)
            {
                try
                {
                    var modifiedXml = $"<root>{xml}</root>";
                    poml = ParseXml(modifiedXml, basePath);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            poml = null;
            return false;
        }

        private static Poml ParseXml(string xml, string basePath)
        {
            var parseXml = xml;
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(parseXml);

            var scene = xmlDocument.SelectSingleNode("//scene");
            var pomlScene = ParseScene(scene, basePath);

            var resource = xmlDocument.SelectSingleNode("//resource");
            var pomlResource = ParseResource(resource, basePath);

            var poml = new Poml(pomlScene, pomlResource);
            return poml;
        }

        private static PomlScene ParseScene(XmlNode scene, string basePath)
        {
            var elements = ParseElements(scene, basePath);
            var pomlScene = new PomlScene(elements);
            if (scene != null)
            {
                pomlScene.WsRecvUrl = scene.GetAttribute("ws-recv-url", null);
            }
            return pomlScene;
        }

        private static PomlResource ParseResource(XmlNode resource, string basePath)
        {
            var elements = ParseElements(resource, basePath);
            var pomlResource = new PomlResource(elements);
            return pomlResource;
        }

        private static List<PomlElement> ParseElements(XmlNode rootNode, string basePath)
        {
            var elements = new List<PomlElement>();
            if (rootNode == null)
            {
                return elements;
            }

            foreach (XmlNode node in rootNode.ChildNodes)
            {
                var element = ParseElement(node, basePath);
                if (element != null)
                {
                    elements.Add(element);
                }
            }
            return elements;
        }

        private static PomlElementType GetElementType(XmlNode node)
        {
            var tag = node.Name.ToLower();
            if (EnumLabel.TryGetValue<PomlElementType>(tag, out var type) == false)
            {
                Debug.LogWarning($"Tag:{tag} is invalid");
                return PomlElementType.None;
            }
            return type;
        }

        private static PomlElement ParseElement(XmlNode node, string basePath, PomlElement parentElement = null)
        {
            var pomlElement = InitElement(node, basePath);
            if (pomlElement == null)
            {
                return null;
            }
            Func<XmlNode, bool> childFilter = null;
            if (pomlElement.ElementType == PomlElementType.Geometry)
            {
                // Ignore shape tags (<line>, etc.) within <geometry>.
                childFilter = (n) => EnumLabel.TryGetValue<PomlGeometryType>(n.Name.ToLower(), out _) == false;
            }

            // child elements
            var childElements = new List<PomlElement>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (childFilter?.Invoke(child) == false)
                {
                    continue;
                }

                var childElement = ParseElement(child, basePath, pomlElement);
                if (childElement != null)
                {
                    childElements.Add(childElement);
                }
            }

            // Read common attributes
            var id = node.GetAttribute("id", null);
            var wsRecvUrl = node.GetAttribute("ws-recv-url", null);

            var attribute = ReadElementAttributeAttribute(node);

            // transform
            var position = ReadVector3Attribute(node, "position", 0);
            var scale = ReadScaleAttribute(node, "scale", 1);
            var rotation = ReadRotationAttribute(node, "rotation");

            var rotationMode = ReadRotationMode(node, "rotation-mode");

            // scale-by-distance
            float? scaleByDistance = null;
            var scaleByDistanceText = node.GetAttribute("scale-by-distance", null);
            if (scaleByDistanceText != null)
            {
                if (float.TryParse(scaleByDistanceText, out var scaleByDistanceFloat))
                {
                    scaleByDistance = scaleByDistanceFloat;
                }
                else if (bool.TryParse(scaleByDistanceText, out var scaleByDistanceBool))
                {
                    if (scaleByDistanceBool)
                    {
                        scaleByDistance = 1;
                    }
                }
            }

            var minScale = ReadMinMaxScaleAttribute(node, "min-scale");
            var maxScale = ReadMinMaxScaleAttribute(node, "max-scale");

            var src = node.GetAttribute("src", null);
            if (src != null)
            {
                src = FilePathUtility.GetAbsolutePath(src, basePath);
            }
            var filename = node.GetAttribute("filename", null);

            pomlElement.Parent = parentElement;
            pomlElement.Children = childElements;

            pomlElement.Attribute = attribute;
            pomlElement.Id = id;
            pomlElement.WsRecvUrl = wsRecvUrl;
            pomlElement.Position = position;
            pomlElement.Scale = scale;
            pomlElement.Rotation = rotation;
            pomlElement.ScaleByDistance = scaleByDistance;
            pomlElement.MinScale = minScale;
            pomlElement.MaxScale = maxScale;
            pomlElement.Src = src;
            pomlElement.Filename = filename;
            pomlElement.RotationMode = rotationMode;
            pomlElement.WebLink = node.GetAttribute("web-link");

            pomlElement.Display = node.GetEnumAttribute("display", PomlDisplayType.Visible);
            pomlElement.ArDisplay = node.GetEnumAttribute("ar-display", PomlArDisplayType.SameAsDisplay);

            return pomlElement;
        }

        private static PomlRotationMode ReadRotationMode(XmlNode node, string attributeName)
        {
            var attribute = node.GetAttribute(attributeName);
            if (EnumLabel.TryGetValue<PomlRotationMode>(attribute, out var value))
            {
                return value;
            }
            return PomlRotationMode.None;
        }

        private static ElementAttributeType ReadElementAttributeAttribute(XmlNode node)
        {
            var attributeString = node.GetAttribute("attribute");

            var separator = new char[] { ' ' };
            var attributeArray = attributeString.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            var attribute = ElementAttributeType.None;
            foreach (var attributeToken in attributeArray)
            {
                if (EnumLabel.TryGetValue<ElementAttributeType>(attributeToken.ToLower(), out var attr))
                {
                    attribute |= attr;
                }
            }
            return attribute;
        }

        private static Quaternion ReadRotationAttribute(XmlNode node, string attributeName)
        {
            if (node.TryGetAttribute(attributeName, out var attribute))
            {
                var values = ReadFloatArray(attribute);
                if (values.Count < 4)
                {
                    return Quaternion.identity;
                }

                var x = values[0];
                var y = values[1];
                var z = values[2];
                var w = values[3];
                var rotation = new Quaternion(x, y, z, w);
                rotation.Normalize();
                return rotation;
            }

            return Quaternion.identity;
        }

        private static PomlElement InitElement(XmlNode node, string basePath)
        {
            var elementType = GetElementType(node);
            switch (elementType)
            {
                case PomlElementType.Script:
                    return InitScriptElement(node, basePath);
                case PomlElementType.Text:
                    return InitTextElement(node);
                case PomlElementType.SpaceReference:
                    return InitSpaceReferenceElement(node);
                case PomlElementType.GeoReference:
                    return InitGeoReferenceElement(node);
                case PomlElementType.Element:
                    return new PomlEmptyElement();
                case PomlElementType.Model:
                    return InitModelElement(node);
                case PomlElementType.Audio:
                    return InitAudioElement(node);
                case PomlElementType.Image:
                    return InitImageElement(node);
                case PomlElementType.Video:
                    return InitVideoElement(node);
                case PomlElementType.Geometry:
                    return InitGeometryElement(node);
                case PomlElementType.Cesium3dTiles:
                    return InitCesium3dTilesElement(node);
                case PomlElementType.ScreenSpace:
                    return InitScreenSpaceElement(node);
                default:
                    return null;
            }
        }

        private static PomlElement InitTextElement(XmlNode node)
        {
            var text = node.GetAttribute("text");
            var textElement = new PomlTextElement(text);
            if (node.TryGetAttribute("font-size", out var fontSizeStr))
            {
                textElement.FontSize = fontSizeStr;
            }
            if (node.TryGetAttribute("font-color", out var fontColorStr) && ColorConverter.TryParseHtmlString(fontColorStr, out var color))
            {
                textElement.FontColor = color;
            }
            if (node.TryGetAttribute("background-color", out var backgroundColorStr)
                && ColorConverter.TryParseHtmlString(backgroundColorStr, out var backgroundColor))
            {
                textElement.BackgroundColor = backgroundColor;
            }

            var textAlign = node.GetAttribute("text-align", "");
            textElement.TextAlign = textAlign;

            textElement.Width = node.GetFloatAttribute("width");
            textElement.Height = node.GetFloatAttribute("height");

            textElement.BorderWidth = node.GetFloatAttribute("border-width");

            return textElement;
        }

        private static PomlElement InitImageElement(XmlNode node)
        {
            var image = new PomlImageElement();
            if (float.TryParse(node.GetAttribute("width"), out var width))
            {
                image.Width = width;
            }
            if (float.TryParse(node.GetAttribute("height"), out var height))
            {
                image.Height = height;
            }
            return image;
        }

        private static PomlElement InitVideoElement(XmlNode node)
        {
            var video = new PomlVideoElement();
            if (float.TryParse(node.GetAttribute("width"), out var width))
            {
                video.Width = width;
            }
            if (float.TryParse(node.GetAttribute("height"), out var height))
            {
                video.Height = height;
            }
            return video;
        }

        private static PomlGeometryElement InitGeometryElement(XmlNode node)
        {
            // <geometry position-type="relative">
            //   <line start="1,2,3" end="4,5,6"/>
            // </geometry>

            var element = new PomlGeometryElement();
            var positionTypeStr = node.GetAttribute("position-type");
            if (EnumLabel.TryGetValue<PositionType>(positionTypeStr, out var positionType) == false)
            {
                positionType = PositionType.Relative;
            }
            foreach (XmlNode gNode in node.ChildNodes)
            {
                var typeName = gNode.Name.ToLower();
                if (EnumLabel.TryGetValue<PomlGeometryType>(typeName, out var type) == false)
                {
                    Debug.LogWarning($"Tag:{typeName} is invalid");
                    type = PomlGeometryType.Unknown;
                }
                PomlGeometry geometry = type switch
                {
                    PomlGeometryType.Line => CreateLine(gNode, positionType),
                    _ => null,
                };
                if (geometry != null)
                {
                    element.Geometries.Add(geometry);
                }
            }
            return element;

            static LineGeometry CreateLine(XmlNode lineNode, PositionType positionType)
            {
                // <line start="1,2,3" end="4,5,6" color="red"/>

                var line = new LineGeometry
                {
                    PositionType = positionType,
                    Color = lineNode.GetColorAttribute("color", Color.white),
                    Width = lineNode.GetFloatAttribute("width", 0f),
                };
                switch (positionType)
                {
                    case PositionType.Relative:
                        line.Start = ReadVector3Attribute(lineNode, "start", 0);
                        line.End = ReadVector3Attribute(lineNode, "end", 0);
                        break;
                    case PositionType.GeoLocation:
                        line.StartGeoLocation = ReadDouble3Attribute(lineNode, "start", 0);
                        line.EndGeoLocation = ReadDouble3Attribute(lineNode, "end", 0);
                        break;
                    default:
                        break;
                }
                return line;
            }
        }

        private static PomlElement InitAudioElement(XmlNode node)
        {
            var audio = new PomlAudioElement();
            audio.Loop = node.GetBooleanAttribute("loop", false);

            if (float.TryParse(node.GetAttribute("play-distance"), out var playDistance))
            {
                audio.PlayDistance = playDistance;
            }
            if (float.TryParse(node.GetAttribute("stop-distance"), out var stopDistance))
            {
                audio.StopDistance = stopDistance;
            }
            return audio;
        }

        private static PomlElement InitModelElement(XmlNode node)
        {
            var model = new PomlModelElement();
            return model;
        }

        private static PomlElement InitSpaceReferenceElement(XmlNode node)
        {
            var placement = new PomlSpaceReferenceElement();

            placement.SpaceType = node.GetAttribute("space-type");
            placement.SpaceId = node.GetAttribute("space-id");

            return placement;
        }

        private static PomlElement InitGeoReferenceElement(XmlNode node)
        {
            var placement = new PomlGeoReferenceElement();

            if (double.TryParse(node.GetAttribute("latitude"), out var latitude))
            {
                placement.Latitude = latitude;
            }
            if (double.TryParse(node.GetAttribute("longitude"), out var longitude))
            {
                placement.Longitude = longitude;
            }
            if (double.TryParse(node.GetAttribute("ellipsoidal-height"), out var ellipsoidalHeight))
            {
                placement.EllipsoidalHeight = ellipsoidalHeight;
            }
            placement.EnuRotation = ReadRotationAttribute(node, "enu-rotation");

            return placement;
        }

        private static PomlElement InitScriptElement(XmlNode node, string basePath)
        {
            var args = node.GetAttribute("args", "");

            var separator = new char[] { ' ' };
            var argsList = args.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            return new PomlScriptElement()
            {
                Args = argsList
            };
        }

        private static PomlElement InitCesium3dTilesElement(XmlNode node)
        {
            var model = new PomlCesium3dTilesElement();
            return model;
        }

        private static PomlElement InitScreenSpaceElement(XmlNode node)
        {
            var screenSpace = new PomlScreenSpaceElement();
            return screenSpace;
        }

        private static float ReadFloatAttribute(XmlNode node, string key, float defaultValue = 0)
        {
            var stringValue = node.GetAttribute(key);
            if (float.TryParse(stringValue, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        private static Vector3 ReadVector3Attribute(XmlNode node, string key, float defaultValue)
        {
            if (!node.TryGetAttribute(key, out var attribute))
            {
                return new Vector3(defaultValue, defaultValue, defaultValue);
            }

            var values = ReadFloatArray(attribute);
            var x = GetValueByIndex(values, 0, defaultValue);
            var y = GetValueByIndex(values, 1, defaultValue);
            var z = GetValueByIndex(values, 2, defaultValue);
            return new Vector3(x, y, z);
        }

        private static Vector3 ReadScaleAttribute(XmlNode node, string key, float defaultValue)
        {
            if (!node.TryGetAttribute(key, out var attribute))
            {
                return new Vector3(defaultValue, defaultValue, defaultValue);
            }

            var values = ReadFloatArray(attribute);
            return values.Count switch
            {
                1 => new Vector3(values[0], values[0], values[0]),
                3 => new Vector3(values[0], values[1], values[2]),
                _ => new Vector3(defaultValue, defaultValue, defaultValue),
            };
        }

        private static (double X, double Y, double Z) ReadDouble3Attribute(XmlNode node, string key, double defaultValue)
        {
            if (!node.TryGetAttribute(key, out var attribute))
            {
                return (defaultValue, defaultValue, defaultValue);
            }
            var split = attribute.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return (
                X: ExtractDouble(split, 0, defaultValue),
                Y: ExtractDouble(split, 1, defaultValue),
                Z: ExtractDouble(split, 2, defaultValue)
            );

            static double ExtractDouble(string[] values, int index, double defaultValue)
            {
                return (values.Length > index) && double.TryParse(values[index], out var a) ? a : defaultValue;
            }
        }

        private static Vector3? ReadMinMaxScaleAttribute(XmlNode node, string key)
        {
            if (node.TryGetAttribute(key, out var attribute) == false)
            {
                return null;
            }

            var values = ReadFloatArray(attribute);
            if (values.Count == 0)
            {
                return null;
            }
            else if (values.Count == 1)
            {
                var scale = values[0];
                var vector = new Vector3(scale, scale, scale);
                return vector;
            }
            else
            {
                var defaultValue = 0;
                var x = GetValueByIndex(values, 0, defaultValue);
                var y = GetValueByIndex(values, 1, defaultValue);
                var z = GetValueByIndex(values, 2, defaultValue);
                var vector = CoordinateUtility.ToUnityCoordinate(x, y, z, directional: false);
                return vector;
            }
        }

        private static List<float> ReadFloatArray(string text)
        {
            var values = new List<float>();
            var separator = new char[] { ',', ' ' };
            var tokens = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (!float.TryParse(token, out var value))
                {
                    break;
                }
                values.Add(value);
            }
            return values;
        }

        private static T GetValueByIndex<T>(List<T> list, int index, T defaultValue)
        {
            if (list.Count > index)
            {
                return list[index];
            }
            return defaultValue;
        }
    }

    // TODO: Move this class.
    internal static class ColorConverter
    {
        private static readonly Dictionary<string, Color> _htmlColors = new Dictionary<string, Color>()
        {
            // black (r, g, b, a) = (0, 0, 0, 255)
            ["black"] = new Color(0f, 0f, 0f, 1f),
            // aliceblue (r, g, b, a) = (240, 248, 255, 255)
            ["aliceblue"] = new Color(0.9411765f, 0.972549f, 1f, 1f),
            // darkcyan (r, g, b, a) = (0, 139, 139, 255)
            ["darkcyan"] = new Color(0f, 0.54509807f, 0.54509807f, 1f),
            // lightyellow (r, g, b, a) = (255, 255, 224, 255)
            ["lightyellow"] = new Color(1f, 1f, 0.8784314f, 1f),
            // coral (r, g, b, a) = (255, 127, 80, 255)
            ["coral"] = new Color(1f, 0.49803922f, 0.3137255f, 1f),
            // dimgray (r, g, b, a) = (105, 105, 105, 255)
            ["dimgray"] = new Color(0.4117647f, 0.4117647f, 0.4117647f, 1f),
            // lavender (r, g, b, a) = (230, 230, 250, 255)
            ["lavender"] = new Color(0.9019608f, 0.9019608f, 0.98039216f, 1f),
            // teal (r, g, b, a) = (0, 128, 128, 255)
            ["teal"] = new Color(0f, 0.5019608f, 0.5019608f, 1f),
            // lightgoldenrodyellow (r, g, b, a) = (250, 250, 210, 255)
            ["lightgoldenrodyellow"] = new Color(0.98039216f, 0.98039216f, 0.8235294f, 1f),
            // tomato (r, g, b, a) = (255, 99, 71, 255)
            ["tomato"] = new Color(1f, 0.3882353f, 0.2784314f, 1f),
            // gray (r, g, b, a) = (128, 128, 128, 255)
            ["gray"] = new Color(0.5019608f, 0.5019608f, 0.5019608f, 1f),
            // lightsteelblue (r, g, b, a) = (176, 196, 222, 255)
            ["lightsteelblue"] = new Color(0.6901961f, 0.76862746f, 0.87058824f, 1f),
            // darkslategray (r, g, b, a) = (47, 79, 79, 255)
            ["darkslategray"] = new Color(0.18431373f, 0.30980393f, 0.30980393f, 1f),
            // lemonchiffon (r, g, b, a) = (255, 250, 205, 255)
            ["lemonchiffon"] = new Color(1f, 0.98039216f, 0.8039216f, 1f),
            // orangered (r, g, b, a) = (255, 69, 0, 255)
            ["orangered"] = new Color(1f, 0.27058825f, 0f, 1f),
            // darkgray (r, g, b, a) = (169, 169, 169, 255)
            ["darkgray"] = new Color(0.6627451f, 0.6627451f, 0.6627451f, 1f),
            // lightslategray (r, g, b, a) = (119, 136, 153, 255)
            ["lightslategray"] = new Color(0.46666667f, 0.53333336f, 0.6f, 1f),
            // darkgreen (r, g, b, a) = (0, 100, 0, 255)
            ["darkgreen"] = new Color(0f, 0.39215687f, 0f, 1f),
            // wheat (r, g, b, a) = (245, 222, 179, 255)
            ["wheat"] = new Color(0.9607843f, 0.87058824f, 0.7019608f, 1f),
            // red (r, g, b, a) = (255, 0, 0, 255)
            ["red"] = new Color(1f, 0f, 0f, 1f),
            // silver (r, g, b, a) = (192, 192, 192, 255)
            ["silver"] = new Color(0.7529412f, 0.7529412f, 0.7529412f, 1f),
            // slategray (r, g, b, a) = (112, 128, 144, 255)
            ["slategray"] = new Color(0.4392157f, 0.5019608f, 0.5647059f, 1f),
            // green (r, g, b, a) = (0, 128, 0, 255)
            ["green"] = new Color(0f, 0.5019608f, 0f, 1f),
            // burlywood (r, g, b, a) = (222, 184, 135, 255)
            ["burlywood"] = new Color(0.87058824f, 0.72156864f, 0.5294118f, 1f),
            // crimson (r, g, b, a) = (220, 20, 60, 255)
            ["crimson"] = new Color(0.8627451f, 0.078431375f, 0.23529412f, 1f),
            // lightgray (r, g, b, a) = (211, 211, 211, 255)
            ["lightgray"] = new Color(0.827451f, 0.827451f, 0.827451f, 1f),
            // steelblue (r, g, b, a) = (70, 130, 180, 255)
            ["steelblue"] = new Color(0.27450982f, 0.50980395f, 0.7058824f, 1f),
            // forestgreen (r, g, b, a) = (34, 139, 34, 255)
            ["forestgreen"] = new Color(0.13333334f, 0.54509807f, 0.13333334f, 1f),
            // tan (r, g, b, a) = (210, 180, 140, 255)
            ["tan"] = new Color(0.8235294f, 0.7058824f, 0.54901963f, 1f),
            // mediumvioletred (r, g, b, a) = (199, 21, 133, 255)
            ["mediumvioletred"] = new Color(0.78039217f, 0.08235294f, 0.52156866f, 1f),
            // gainsboro (r, g, b, a) = (220, 220, 220, 255)
            ["gainsboro"] = new Color(0.8627451f, 0.8627451f, 0.8627451f, 1f),
            // royalblue (r, g, b, a) = (65, 105, 225, 255)
            ["royalblue"] = new Color(0.25490198f, 0.4117647f, 0.88235295f, 1f),
            // seagreen (r, g, b, a) = (46, 139, 87, 255)
            ["seagreen"] = new Color(0.18039216f, 0.54509807f, 0.34117648f, 1f),
            // khaki (r, g, b, a) = (240, 230, 140, 255)
            ["khaki"] = new Color(0.9411765f, 0.9019608f, 0.54901963f, 1f),
            // deeppink (r, g, b, a) = (255, 20, 147, 255)
            ["deeppink"] = new Color(1f, 0.078431375f, 0.5764706f, 1f),
            // whitesmoke (r, g, b, a) = (245, 245, 245, 255)
            ["whitesmoke"] = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f),
            // midnightblue (r, g, b, a) = (25, 25, 112, 255)
            ["midnightblue"] = new Color(0.09803922f, 0.09803922f, 0.4392157f, 1f),
            // mediumseagreen (r, g, b, a) = (60, 179, 113, 255)
            ["mediumseagreen"] = new Color(0.23529412f, 0.7019608f, 0.44313726f, 1f),
            // yellow (r, g, b, a) = (255, 255, 0, 255)
            ["yellow"] = new Color(1f, 1f, 0f, 1f),
            // hotpink (r, g, b, a) = (255, 105, 180, 255)
            ["hotpink"] = new Color(1f, 0.4117647f, 0.7058824f, 1f),
            // white (r, g, b, a) = (255, 255, 255, 255)
            ["white"] = new Color(1f, 1f, 1f, 1f),
            // navy (r, g, b, a) = (0, 0, 128, 255)
            ["navy"] = new Color(0f, 0f, 0.5019608f, 1f),
            // mediumaquamarine (r, g, b, a) = (102, 205, 170, 255)
            ["mediumaquamarine"] = new Color(0.4f, 0.8039216f, 0.6666667f, 1f),
            // gold (r, g, b, a) = (255, 215, 0, 255)
            ["gold"] = new Color(1f, 0.84313726f, 0f, 1f),
            // palevioletred (r, g, b, a) = (219, 112, 147, 255)
            ["palevioletred"] = new Color(0.85882354f, 0.4392157f, 0.5764706f, 1f),
            // snow (r, g, b, a) = (255, 250, 250, 255)
            ["snow"] = new Color(1f, 0.98039216f, 0.98039216f, 1f),
            // darkblue (r, g, b, a) = (0, 0, 139, 255)
            ["darkblue"] = new Color(0f, 0f, 0.54509807f, 1f),
            // darkseagreen (r, g, b, a) = (143, 188, 143, 255)
            ["darkseagreen"] = new Color(0.56078434f, 0.7372549f, 0.56078434f, 1f),
            // orange (r, g, b, a) = (255, 165, 0, 255)
            ["orange"] = new Color(1f, 0.64705884f, 0f, 1f),
            // pink (r, g, b, a) = (255, 192, 203, 255)
            ["pink"] = new Color(1f, 0.7529412f, 0.79607844f, 1f),
            // ghostwhite (r, g, b, a) = (248, 248, 255, 255)
            ["ghostwhite"] = new Color(0.972549f, 0.972549f, 1f, 1f),
            // mediumblue (r, g, b, a) = (0, 0, 205, 255)
            ["mediumblue"] = new Color(0f, 0f, 0.8039216f, 1f),
            // aquamarine (r, g, b, a) = (127, 255, 212, 255)
            ["aquamarine"] = new Color(0.49803922f, 1f, 0.83137256f, 1f),
            // sandybrown (r, g, b, a) = (244, 164, 96, 255)
            ["sandybrown"] = new Color(0.95686275f, 0.6431373f, 0.3764706f, 1f),
            // lightpink (r, g, b, a) = (255, 182, 193, 255)
            ["lightpink"] = new Color(1f, 0.7137255f, 0.75686276f, 1f),
            // floralwhite (r, g, b, a) = (255, 250, 240, 255)
            ["floralwhite"] = new Color(1f, 0.98039216f, 0.9411765f, 1f),
            // blue (r, g, b, a) = (0, 0, 255, 255)
            ["blue"] = new Color(0f, 0f, 1f, 1f),
            // palegreen (r, g, b, a) = (152, 251, 152, 255)
            ["palegreen"] = new Color(0.59607846f, 0.9843137f, 0.59607846f, 1f),
            // darkorange (r, g, b, a) = (255, 140, 0, 255)
            ["darkorange"] = new Color(1f, 0.54901963f, 0f, 1f),
            // thistle (r, g, b, a) = (216, 191, 216, 255)
            ["thistle"] = new Color(0.84705883f, 0.7490196f, 0.84705883f, 1f),
            // linen (r, g, b, a) = (250, 240, 230, 255)
            ["linen"] = new Color(0.98039216f, 0.9411765f, 0.9019608f, 1f),
            // dodgerblue (r, g, b, a) = (30, 144, 255, 255)
            ["dodgerblue"] = new Color(0.11764706f, 0.5647059f, 1f, 1f),
            // lightgreen (r, g, b, a) = (144, 238, 144, 255)
            ["lightgreen"] = new Color(0.5647059f, 0.93333334f, 0.5647059f, 1f),
            // goldenrod (r, g, b, a) = (218, 165, 32, 255)
            ["goldenrod"] = new Color(0.85490197f, 0.64705884f, 0.1254902f, 1f),
            // magenta (r, g, b, a) = (255, 0, 255, 255)
            ["magenta"] = new Color(1f, 0f, 1f, 1f),
            // antiquewhite (r, g, b, a) = (250, 235, 215, 255)
            ["antiquewhite"] = new Color(0.98039216f, 0.92156863f, 0.84313726f, 1f),
            // cornflowerblue (r, g, b, a) = (100, 149, 237, 255)
            ["cornflowerblue"] = new Color(0.39215687f, 0.58431375f, 0.92941177f, 1f),
            // springgreen (r, g, b, a) = (0, 255, 127, 255)
            ["springgreen"] = new Color(0f, 1f, 0.49803922f, 1f),
            // peru (r, g, b, a) = (205, 133, 63, 255)
            ["peru"] = new Color(0.8039216f, 0.52156866f, 0.24705882f, 1f),
            // fuchsia (r, g, b, a) = (255, 0, 255, 255)
            ["fuchsia"] = new Color(1f, 0f, 1f, 1f),
            // papayawhip (r, g, b, a) = (255, 239, 213, 255)
            ["papayawhip"] = new Color(1f, 0.9372549f, 0.8352941f, 1f),
            // deepskyblue (r, g, b, a) = (0, 191, 255, 255)
            ["deepskyblue"] = new Color(0f, 0.7490196f, 1f, 1f),
            // mediumspringgreen (r, g, b, a) = (0, 250, 154, 255)
            ["mediumspringgreen"] = new Color(0f, 0.98039216f, 0.6039216f, 1f),
            // darkgoldenrod (r, g, b, a) = (184, 134, 11, 255)
            ["darkgoldenrod"] = new Color(0.72156864f, 0.5254902f, 0.043137256f, 1f),
            // violet (r, g, b, a) = (238, 130, 238, 255)
            ["violet"] = new Color(0.93333334f, 0.50980395f, 0.93333334f, 1f),
            // blanchedalmond (r, g, b, a) = (255, 235, 205, 255)
            ["blanchedalmond"] = new Color(1f, 0.92156863f, 0.8039216f, 1f),
            // lightskyblue (r, g, b, a) = (135, 206, 250, 255)
            ["lightskyblue"] = new Color(0.5294118f, 0.80784315f, 0.98039216f, 1f),
            // lawngreen (r, g, b, a) = (124, 252, 0, 255)
            ["lawngreen"] = new Color(0.4862745f, 0.9882353f, 0f, 1f),
            // chocolate (r, g, b, a) = (210, 105, 30, 255)
            ["chocolate"] = new Color(0.8235294f, 0.4117647f, 0.11764706f, 1f),
            // plum (r, g, b, a) = (221, 160, 221, 255)
            ["plum"] = new Color(0.8666667f, 0.627451f, 0.8666667f, 1f),
            // bisque (r, g, b, a) = (255, 228, 196, 255)
            ["bisque"] = new Color(1f, 0.89411765f, 0.76862746f, 1f),
            // skyblue (r, g, b, a) = (135, 206, 235, 255)
            ["skyblue"] = new Color(0.5294118f, 0.80784315f, 0.92156863f, 1f),
            // chartreuse (r, g, b, a) = (127, 255, 0, 255)
            ["chartreuse"] = new Color(0.49803922f, 1f, 0f, 1f),
            // sienna (r, g, b, a) = (160, 82, 45, 255)
            ["sienna"] = new Color(0.627451f, 0.32156864f, 0.1764706f, 1f),
            // orchid (r, g, b, a) = (218, 112, 214, 255)
            ["orchid"] = new Color(0.85490197f, 0.4392157f, 0.8392157f, 1f),
            // moccasin (r, g, b, a) = (255, 228, 181, 255)
            ["moccasin"] = new Color(1f, 0.89411765f, 0.70980394f, 1f),
            // lightblue (r, g, b, a) = (173, 216, 230, 255)
            ["lightblue"] = new Color(0.6784314f, 0.84705883f, 0.9019608f, 1f),
            // greenyellow (r, g, b, a) = (173, 255, 47, 255)
            ["greenyellow"] = new Color(0.6784314f, 1f, 0.18431373f, 1f),
            // saddlebrown (r, g, b, a) = (139, 69, 19, 255)
            ["saddlebrown"] = new Color(0.54509807f, 0.27058825f, 0.07450981f, 1f),
            // mediumorchid (r, g, b, a) = (186, 85, 211, 255)
            ["mediumorchid"] = new Color(0.7294118f, 0.33333334f, 0.827451f, 1f),
            // navajowhite (r, g, b, a) = (255, 222, 173, 255)
            ["navajowhite"] = new Color(1f, 0.87058824f, 0.6784314f, 1f),
            // powderblue (r, g, b, a) = (176, 224, 230, 255)
            ["powderblue"] = new Color(0.6901961f, 0.8784314f, 0.9019608f, 1f),
            // lime (r, g, b, a) = (0, 255, 0, 255)
            ["lime"] = new Color(0f, 1f, 0f, 1f),
            // maroon (r, g, b, a) = (128, 0, 0, 255)
            ["maroon"] = new Color(0.5019608f, 0f, 0f, 1f),
            // darkorchid (r, g, b, a) = (153, 50, 204, 255)
            ["darkorchid"] = new Color(0.6f, 0.19607843f, 0.8f, 1f),
            // peachpuff (r, g, b, a) = (255, 218, 185, 255)
            ["peachpuff"] = new Color(1f, 0.85490197f, 0.7254902f, 1f),
            // paleturquoise (r, g, b, a) = (175, 238, 238, 255)
            ["paleturquoise"] = new Color(0.6862745f, 0.93333334f, 0.93333334f, 1f),
            // limegreen (r, g, b, a) = (50, 205, 50, 255)
            ["limegreen"] = new Color(0.19607843f, 0.8039216f, 0.19607843f, 1f),
            // darkred (r, g, b, a) = (139, 0, 0, 255)
            ["darkred"] = new Color(0.54509807f, 0f, 0f, 1f),
            // darkviolet (r, g, b, a) = (148, 0, 211, 255)
            ["darkviolet"] = new Color(0.5803922f, 0f, 0.827451f, 1f),
            // mistyrose (r, g, b, a) = (255, 228, 225, 255)
            ["mistyrose"] = new Color(1f, 0.89411765f, 0.88235295f, 1f),
            // lightcyan (r, g, b, a) = (224, 255, 255, 255)
            ["lightcyan"] = new Color(0.8784314f, 1f, 1f, 1f),
            // yellowgreen (r, g, b, a) = (154, 205, 50, 255)
            ["yellowgreen"] = new Color(0.6039216f, 0.8039216f, 0.19607843f, 1f),
            // brown (r, g, b, a) = (165, 42, 42, 255)
            ["brown"] = new Color(0.64705884f, 0.16470589f, 0.16470589f, 1f),
            // darkmagenta (r, g, b, a) = (139, 0, 139, 255)
            ["darkmagenta"] = new Color(0.54509807f, 0f, 0.54509807f, 1f),
            // lavenderblush (r, g, b, a) = (255, 240, 245, 255)
            ["lavenderblush"] = new Color(1f, 0.9411765f, 0.9607843f, 1f),
            // cyan (r, g, b, a) = (0, 255, 255, 255)
            ["cyan"] = new Color(0f, 1f, 1f, 1f),
            // darkolivegreen (r, g, b, a) = (85, 107, 47, 255)
            ["darkolivegreen"] = new Color(0.33333334f, 0.41960785f, 0.18431373f, 1f),
            // firebrick (r, g, b, a) = (178, 34, 34, 255)
            ["firebrick"] = new Color(0.69803923f, 0.13333334f, 0.13333334f, 1f),
            // purple (r, g, b, a) = (128, 0, 128, 255)
            ["purple"] = new Color(0.5019608f, 0f, 0.5019608f, 1f),
            // seashell (r, g, b, a) = (255, 245, 238, 255)
            ["seashell"] = new Color(1f, 0.9607843f, 0.93333334f, 1f),
            // aqua (r, g, b, a) = (0, 255, 255, 255)
            ["aqua"] = new Color(0f, 1f, 1f, 1f),
            // olivedrab (r, g, b, a) = (107, 142, 35, 255)
            ["olivedrab"] = new Color(0.41960785f, 0.5568628f, 0.13725491f, 1f),
            // indianred (r, g, b, a) = (205, 92, 92, 255)
            ["indianred"] = new Color(0.8039216f, 0.36078432f, 0.36078432f, 1f),
            // indigo (r, g, b, a) = (75, 0, 130, 255)
            ["indigo"] = new Color(0.29411766f, 0f, 0.50980395f, 1f),
            // oldlace (r, g, b, a) = (253, 245, 230, 255)
            ["oldlace"] = new Color(0.99215686f, 0.9607843f, 0.9019608f, 1f),
            // turquoise (r, g, b, a) = (64, 224, 208, 255)
            ["turquoise"] = new Color(0.2509804f, 0.8784314f, 0.8156863f, 1f),
            // olive (r, g, b, a) = (128, 128, 0, 255)
            ["olive"] = new Color(0.5019608f, 0.5019608f, 0f, 1f),
            // rosybrown (r, g, b, a) = (188, 143, 143, 255)
            ["rosybrown"] = new Color(0.7372549f, 0.56078434f, 0.56078434f, 1f),
            // darkslateblue (r, g, b, a) = (72, 61, 139, 255)
            ["darkslateblue"] = new Color(0.28235295f, 0.23921569f, 0.54509807f, 1f),
            // ivory (r, g, b, a) = (255, 255, 240, 255)
            ["ivory"] = new Color(1f, 1f, 0.9411765f, 1f),
            // mediumturquoise (r, g, b, a) = (72, 209, 204, 255)
            ["mediumturquoise"] = new Color(0.28235295f, 0.81960785f, 0.8f, 1f),
            // darkkhaki (r, g, b, a) = (189, 183, 107, 255)
            ["darkkhaki"] = new Color(0.7411765f, 0.7176471f, 0.41960785f, 1f),
            // darksalmon (r, g, b, a) = (233, 150, 122, 255)
            ["darksalmon"] = new Color(0.9137255f, 0.5882353f, 0.47843137f, 1f),
            // blueviolet (r, g, b, a) = (138, 43, 226, 255)
            ["blueviolet"] = new Color(0.5411765f, 0.16862746f, 0.8862745f, 1f),
            // honeydew (r, g, b, a) = (240, 255, 240, 255)
            ["honeydew"] = new Color(0.9411765f, 1f, 0.9411765f, 1f),
            // darkturquoise (r, g, b, a) = (0, 206, 209, 255)
            ["darkturquoise"] = new Color(0f, 0.80784315f, 0.81960785f, 1f),
            // palegoldenrod (r, g, b, a) = (238, 232, 170, 255)
            ["palegoldenrod"] = new Color(0.93333334f, 0.9098039f, 0.6666667f, 1f),
            // lightcoral (r, g, b, a) = (240, 128, 128, 255)
            ["lightcoral"] = new Color(0.9411765f, 0.5019608f, 0.5019608f, 1f),
            // mediumpurple (r, g, b, a) = (147, 112, 219, 255)
            ["mediumpurple"] = new Color(0.5764706f, 0.4392157f, 0.85882354f, 1f),
            // mintcream (r, g, b, a) = (245, 255, 250, 255)
            ["mintcream"] = new Color(0.9607843f, 1f, 0.98039216f, 1f),
            // lightseagreen (r, g, b, a) = (32, 178, 170, 255)
            ["lightseagreen"] = new Color(0.1254902f, 0.69803923f, 0.6666667f, 1f),
            // cornsilk (r, g, b, a) = (255, 248, 220, 255)
            ["cornsilk"] = new Color(1f, 0.972549f, 0.8627451f, 1f),
            // salmon (r, g, b, a) = (250, 128, 114, 255)
            ["salmon"] = new Color(0.98039216f, 0.5019608f, 0.44705883f, 1f),
            // slateblue (r, g, b, a) = (106, 90, 205, 255)
            ["slateblue"] = new Color(0.41568628f, 0.3529412f, 0.8039216f, 1f),
            // azure (r, g, b, a) = (240, 255, 255, 255)
            ["azure"] = new Color(0.9411765f, 1f, 1f, 1f),
            // cadetblue (r, g, b, a) = (95, 158, 160, 255)
            ["cadetblue"] = new Color(0.37254903f, 0.61960787f, 0.627451f, 1f),
            // beige (r, g, b, a) = (245, 245, 220, 255)
            ["beige"] = new Color(0.9607843f, 0.9607843f, 0.8627451f, 1f),
            // lightsalmon (r, g, b, a) = (255, 160, 122, 255)
            ["lightsalmon"] = new Color(1f, 0.627451f, 0.47843137f, 1f),
            // mediumslateblue (r, g, b, a) = (123, 104, 238, 255)
            ["mediumslateblue"] = new Color(0.48235294f, 0.40784314f, 0.93333334f, 1f),
        };

        public static bool TryParseHtmlString(string htmlColor, out Color color)
        {
            color = default;
            if (string.IsNullOrEmpty(htmlColor)) { return false; }

            // #RRGGBB or #RGB
            if (htmlColor[0] == '#' && (htmlColor.Length == 7 || htmlColor.Length == 4))
            {
                if (htmlColor.Length == 7)
                {
                    var r = Convert.ToInt32(htmlColor.Substring(1, 2), 16);
                    var g = Convert.ToInt32(htmlColor.Substring(3, 2), 16);
                    var b = Convert.ToInt32(htmlColor.Substring(5, 2), 16);
                    color = new Color(r / 255f, g / 255f, b / 255f);
                    return true;
                }
                else
                {
                    var rStr = char.ToString(htmlColor[1]);
                    var gStr = char.ToString(htmlColor[2]);
                    var bStr = char.ToString(htmlColor[3]);
                    var r = Convert.ToInt32(rStr + rStr, 16);
                    var g = Convert.ToInt32(gStr + gStr, 16);
                    var b = Convert.ToInt32(bStr + bStr, 16);
                    color = new Color(r / 255f, g / 255f, b / 255f);
                    return true;
                }
            }
            else
            {
                return _htmlColors.TryGetValue(htmlColor.ToLower(), out color);
            }
        }
    }
}
