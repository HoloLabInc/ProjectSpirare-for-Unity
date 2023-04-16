using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HoloLab.Spirare
{
    internal static class EnumLabel
    {
        public static string GetLabel<TEnum>(this TEnum value) where TEnum : struct, Enum
        {
            return TryGetLabel(value, out var label) ? label : null;
        }

        public static bool TryGetLabel<TEnum>(this TEnum value, out string label) where TEnum : struct, Enum
        {
            return Cache<TEnum>.TryGetLabel(value, out label);
        }

        public static IEnumerable<string> GetLabels<TEnum>(this TEnum value) where TEnum : struct, Enum
        {
            return Cache<TEnum>.GetLabels(value);
        }

        public static bool TryGetValue<TEnum>(string label, out TEnum value) where TEnum : struct, Enum
        {
            return Cache<TEnum>.TryGetValue(label, out value);
        }

        public static IEnumerable<TEnum> GetValues<TEnum>(string label) where TEnum : struct, Enum
        {
            return Cache<TEnum>.GetValues(label);
        }

        private static class Cache<TEnum> where TEnum : struct, Enum
        {
            private static readonly Dictionary<string, List<TEnum>> _labelToValue;
            private static readonly Dictionary<TEnum, List<string>> _valueToLabel;

            static Cache()
            {
                foreach (var name in Enum.GetNames(typeof(TEnum)))
                {
                    Enum.TryParse<TEnum>(name, out var value);
                    var attrs = GetLabelAttributes(name);
                    if (_valueToLabel == null)
                    {
                        _valueToLabel = new Dictionary<TEnum, List<string>>();
                    }
                    if (_valueToLabel.TryGetValue(value, out var labels) == false)
                    {
                        labels = new List<string>();
                        _valueToLabel.Add(value, labels);
                    }

                    foreach (var attr in attrs)
                    {
                        var label = attr.Label;
                        labels.Add(label);
                        if (_labelToValue == null)
                        {
                            _labelToValue = new Dictionary<string, List<TEnum>>();
                        }
                        if (_labelToValue.TryGetValue(label, out var values) == false)
                        {
                            values = new List<TEnum>();
                            _labelToValue.Add(label, values);
                        }
                        values.Add(value);
                    }
                }
            }

            private static IEnumerable<EnumLabelAttribute> GetLabelAttributes(string name)
            {
                return typeof(TEnum)
                    .GetField(name)
                    .GetCustomAttributes(typeof(EnumLabelAttribute))
                    .OfType<EnumLabelAttribute>();
            }

            public static bool TryGetLabel(TEnum value, out string label)
            {
                if (_valueToLabel != null && _valueToLabel.TryGetValue(value, out var labels) && labels.Count > 0)
                {
                    label = labels[0];
                    return true;
                }
                label = default;
                return false;
            }

            public static IEnumerable<string> GetLabels(TEnum value)
            {
                if (_valueToLabel != null && _valueToLabel.TryGetValue(value, out var labels))
                {
                    return labels;
                }
                return Array.Empty<string>();
            }

            public static bool TryGetValue(string label, out TEnum value)
            {
                if (_labelToValue != null && _labelToValue.TryGetValue(label, out var values) && values.Count > 0)
                {
                    value = values[0];
                    return true;
                }
                value = default;
                return false;
            }

            public static IEnumerable<TEnum> GetValues(string label)
            {
                if (_labelToValue != null && _labelToValue.TryGetValue(label, out var values))
                {
                    return values;
                }
                return Array.Empty<TEnum>();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    internal sealed class EnumLabelAttribute : Attribute
    {
        public string Label { get; }

        public EnumLabelAttribute(string label)
        {
            Label = label;
        }
    }
}
