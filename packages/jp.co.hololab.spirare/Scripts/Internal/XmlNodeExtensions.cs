using System;
using System.Xml;

namespace HoloLab.Spirare
{
    internal static class XmlNodeExtensions
    {
        public static bool TryGetAttribute(this XmlNode node, string key, out string value)
        {
            var type = node.Attributes[key];
            if (type == null)
            {
                value = null;
                return false;
            }

            value = type.Value;
            return true;
        }

        public static string GetAttribute(this XmlNode node, string key, string defaultValue = "")
        {
            if (node.TryGetAttribute(key, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        public static bool GetBooleanAttribute(this XmlNode node, string key, bool defaultValue = false)
        {
            if (bool.TryParse(node.GetAttribute(key), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public static float GetFloatAttribute(this XmlNode node, string key, float defaultValue = 0)
        {
            if (float.TryParse(node.GetAttribute(key), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public static UnityEngine.Color GetColorAttribute(this XmlNode node, string key, UnityEngine.Color defaultValue)
        {
            if (node.TryGetAttribute(key, out var colorString) && ColorConverter.TryParseHtmlString(colorString, out var color))
            {
                return color;
            }

            return defaultValue;
        }

        public static TEnum GetEnumAttribute<TEnum>(this XmlNode node, string key, TEnum defaultValue, bool ignoreCase = true) where TEnum : struct, Enum
        {
            var attributeString = node.GetAttribute(key, "");
            if (ignoreCase)
            {
                attributeString = attributeString.ToLower();
            }

            if (EnumLabel.TryGetValue<TEnum>(attributeString, out var value) == false)
            {
                return defaultValue;
            }
            return value;
        }
    }
}
