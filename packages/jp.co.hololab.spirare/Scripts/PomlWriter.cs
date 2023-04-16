using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Debug = UnityEngine.Debug;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace HoloLab.Spirare
{
    internal static class PomlWriter
    {
        public static async Task<string> WriteAsync(Poml poml)
        {
            if (poml == null) { throw new ArgumentNullException(nameof(poml)); }
            return await Task.Run(() => WriteXml(poml)).ConfigureAwait(false);
        }

        public static string Write(Poml poml)
        {
            if (poml == null) { throw new ArgumentNullException(nameof(poml)); }
            var pomlString = WriteXml(poml);
            return pomlString;
        }

        public static bool TryWrite(Poml poml, out string xml)
        {
            // To run this method in the background, do not use GameObject or Transform.
            if (poml == null)
            {
                xml = null;
                return false;
            }
            try
            {
                xml = WriteXml(poml);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                xml = null;
                return false;
            }
        }

        private static string WriteXml(Poml poml)
        {
            var doc = new XDocument(
                new XElement("poml", new[]
                {
                    BuildScene(poml.Scene),
                    BuildResource(poml.Resource),
                }));
            return doc.ToString();
        }

        private static XElement BuildScene(PomlScene scene)
        {
            var sceneNode = new XElement("scene",
                scene.Elements.Select(BuildElement));
            return sceneNode;
        }

        private static XElement BuildElement(PomlElement element)
        {
            if (element.ElementType == PomlElementType.None) { return null; }
            var name = element.ElementType.GetLabel();
            var nodeChildren = new List<XObject>
            {
                (element.Id != null) ? new XAttribute("id", element.Id) : null,
                BuildAttribute(element.Attribute),
                (element.Src != null) ? new XAttribute("src", element.Src) : null, // TODO: convert to releative path.
                (element.Filename != null) ? new XAttribute("filename", element.Filename) : null,
            };
            var elementChildren = element.Children ?? Array.Empty<PomlElement>();
            var needPos = false;
            var needRot = false;
            var needScale = false;
            // TODO:
            switch (element.ElementType)
            {
                case PomlElementType.None:
                    needPos = true;
                    needRot = true;
                    needScale = true;
                    break;
                case PomlElementType.Element:
                    needPos = true;
                    needRot = true;
                    needScale = true;
                    break;
                case PomlElementType.Model:
                    needPos = true;
                    needRot = true;
                    needScale = true;
                    break;
                case PomlElementType.Text:
                    needPos = true;
                    needRot = true;
                    needScale = true;
                    if (element is PomlTextElement textElement)
                    {
                        AddTextElementInfo(textElement, nodeChildren);
                    }
                    break;
                case PomlElementType.Script:
                    break;
                case PomlElementType.SpaceReference:
                    if (element is PomlSpaceReferenceElement spaceReferenceElement)
                    {
                        if (string.IsNullOrEmpty(spaceReferenceElement.SpaceType) == false)
                        {
                            var spaceType = PascalToKebabCase(spaceReferenceElement.SpaceType);
                            nodeChildren.Add(new XAttribute("space-type", spaceType));
                        }

                        nodeChildren.Add(new XAttribute("space-id", spaceReferenceElement.SpaceId));
                        break;
                    }
                    break;
                case PomlElementType.GeoReference:
                    if (element is PomlGeoReferenceElement geoReferenceElement)
                    {
                        nodeChildren.Add(new XAttribute("latitude", geoReferenceElement.Latitude));
                        nodeChildren.Add(new XAttribute("longitude", geoReferenceElement.Longitude));
                        nodeChildren.Add(new XAttribute("ellipsoidal-height", geoReferenceElement.EllipsoidalHeight));
                        nodeChildren.Add(BuildRotation(geoReferenceElement.EnuRotation, "enu-rotation"));
                        break;
                    }
                    break;
                default:
                    break;
            }
            if (elementChildren.Any(x => x is PomlSpaceReferenceElement))
            {
                needPos = false;
                needRot = false;
            }
            if (needPos)
            {
                nodeChildren.Add(BuildPosition(element.Position, "position"));
            }
            if (needRot)
            {
                nodeChildren.Add(BuildRotation(element.Rotation, "rotation"));
            }
            if (needScale)
            {
                nodeChildren.Add(BuildScale(element.Scale, "scale"));
            }
            nodeChildren.AddRange(elementChildren.Select(BuildElement));
            var node = new XElement(name, nodeChildren);
            return node;
        }

        private static XElement BuildResource(PomlResource resource)
        {
            return new XElement("resource");
        }

        private static void AddTextElementInfo(PomlTextElement textElement, List<XObject> list)
        {
            list.Add(new XAttribute("text", textElement.Text));
            list.Add(new XAttribute("font-size", textElement.FontSize));
        }

        private static XAttribute BuildAttribute(ElementAttributeType elemAttr)
        {
            var attrStrArray = new string[]
            {
                GetFlagLabel(elemAttr, ElementAttributeType.Static),
                GetFlagLabel(elemAttr, ElementAttributeType.Equipable),
            }.Where(x => x != null);
            var attrStr = string.Join(" ", attrStrArray);
            return string.IsNullOrEmpty(attrStr) ? null : new XAttribute("attribute", attrStr);

            string GetFlagLabel(ElementAttributeType ea, ElementAttributeType flag)
            {
                return ((ea & flag) == flag) ? flag.GetLabel() : null;
            }
        }

        private static XAttribute BuildPosition(Vector3 position, string attributeName)
        {
            return new XAttribute(attributeName, $"{position.x},{position.y},{position.z}");
        }

        private static XAttribute BuildRotation(Quaternion rotation, string attributeName)
        {
            return new XAttribute(attributeName, $"{rotation.x},{rotation.y},{rotation.z},{rotation.w}");
        }

        private static XAttribute BuildScale(Vector3 scale, string attributeName)
        {
            return new XAttribute(attributeName, $"{scale.x},{scale.y},{scale.z}");
        }

        private static string PascalToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return Regex.Replace(
                value,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                "-$1",
                RegexOptions.Compiled)
                .Trim()
                .ToLower();
        }
    }
}
