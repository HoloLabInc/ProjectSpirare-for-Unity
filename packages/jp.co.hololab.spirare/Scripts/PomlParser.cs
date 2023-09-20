using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Debug = UnityEngine.Debug;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Color = UnityEngine.Color;
using UnityEngine.Assertions;

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
            if (scene == null)
            {
                return new PomlScene();
            }

            var pomlScene = ParseElement(scene, basePath) as PomlScene;
            Assert.IsNotNull(pomlScene);

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

            // Read custom attributes
            foreach (XmlAttribute attr in node.Attributes)
            {
                if (attr.Name.StartsWith("_"))
                {
                    pomlElement.CustomAttributes[attr.Name] = attr.Value;
                }
            }

            // Read common attributes
            var id = node.GetAttribute("id", null);
            var wsRecvUrl = node.GetAttribute("ws-recv-url", null);

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

        private static Quaternion ReadRotationAttribute(XmlNode node, string attributeName)
        {
            if (node.TryGetAttribute(attributeName, out var attribute))
            {
                var values = PomlParserUtility.ParseAsFloatArray(attribute);
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
                case PomlElementType.Scene:
                    return InitSceneElement(node);
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

        private static PomlElement InitSceneElement(XmlNode node)
        {
            return new PomlScene();
        }

        private static PomlElement InitTextElement(XmlNode node)
        {
            var text = node.GetAttribute("text");
            var textElement = new PomlTextElement(text);
            if (node.TryGetAttribute("font-size", out var fontSizeStr))
            {
                textElement.FontSize = fontSizeStr;
            }

            textElement.FontColor = node.GetColorAttribute("font-color", textElement.FontColor);
            textElement.BackgroundColor = node.GetColorAttribute("background-color", textElement.BackgroundColor);

            textElement.TextAlign = node.GetAttribute("text-align", "");

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

            image.Backface = node.GetEnumAttribute("backface", PomlBackfaceType.None);
            image.BackfaceColor = node.GetColorAttribute("backface-color", image.BackfaceColor);

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
            // <geometry>
            //   <line vertices="1,2,3,4,5,6" color="red"/>
            // </geometry>

            var element = new PomlGeometryElement();
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
                    PomlGeometryType.Line => CreateLine(gNode),
                    PomlGeometryType.Polygon => CreatePolygon(gNode),
                    _ => null,
                };
                if (geometry != null)
                {
                    element.Geometries.Add(geometry);
                }
            }
            return element;

            static LineGeometry CreateLine(XmlNode lineNode)
            {
                // <line vertices="1,2,3,4,5,6" color="red"/>
                // <line vertices="geodetic: 1,2,3,4,5,6" color="red"/>

                var line = new LineGeometry
                {
                    Vertices = lineNode.GetAttribute("vertices"),
                    Color = lineNode.GetColorAttribute("color", Color.white),
                    Width = lineNode.GetFloatAttribute("width", 0f),
                };
                return line;
            }

            static PolygonGeometry CreatePolygon(XmlNode polygonNode)
            {
                // <polygon vertices="0,1,2,3,4,5,6,7,8" indices="0,1,2" color="red"/>

                var polygon = new PolygonGeometry
                {
                    Vertices = polygonNode.GetAttribute("vertices"),
                    Color = polygonNode.GetColorAttribute("color", Color.white),
                    Indices = ReadIntArrayAttribute(polygonNode, "indices"),
                };
                return polygon;
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

        private static int[] ReadIntArrayAttribute(XmlNode node, string key)
        {
            if (!node.TryGetAttribute(key, out var attribute))
            {
                return Array.Empty<int>();
            }

            var values = PomlParserUtility.ParseAsIntArray(attribute);
            return values.ToArray();
        }

        private static Vector3 ReadVector3Attribute(XmlNode node, string key, float defaultValue)
        {
            if (!node.TryGetAttribute(key, out var attribute))
            {
                return new Vector3(defaultValue, defaultValue, defaultValue);
            }

            var values = PomlParserUtility.ParseAsFloatArray(attribute);
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

            var values = PomlParserUtility.ParseAsFloatArray(attribute);
            return values.Count switch
            {
                1 => new Vector3(values[0], values[0], values[0]),
                3 => new Vector3(values[0], values[1], values[2]),
                _ => new Vector3(defaultValue, defaultValue, defaultValue),
            };
        }

        private static Vector3? ReadMinMaxScaleAttribute(XmlNode node, string key)
        {
            if (node.TryGetAttribute(key, out var attribute) == false)
            {
                return null;
            }

            var values = PomlParserUtility.ParseAsFloatArray(attribute);
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

        private static T GetValueByIndex<T>(List<T> list, int index, T defaultValue)
        {
            if (list.Count > index)
            {
                return list[index];
            }
            return defaultValue;
        }
    }
}
