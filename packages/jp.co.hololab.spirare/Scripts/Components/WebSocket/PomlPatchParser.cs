using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    internal abstract class PomlPatch
    {
        public enum PomlPatchOperation
        {
            None,
            [EnumLabel("add")]
            Add,
            [EnumLabel("update")]
            Update,
            [EnumLabel("remove")]
            Remove
        }

        public class PomlPatchTarget
        {
            public string Id { set; get; }
            public string Tag { set; get; }
        }

        public PomlPatchOperation Operation { get; }

        public PomlPatchTarget Target { get; set; }

        public PomlPatch(PomlPatchOperation operation)
        {
            Operation = operation;
        }
    }

    internal class PomlPatchAddElement
    {
        public string Tag { set; get; }
        public JObject Attributes { set; get; }

        public PomlPatchAddElement[] Children { set; get; }
    }

    internal class PomlPatchAdd : PomlPatch
    {
        public PomlPatchAddElement Element { set; get; }

        public PomlPatchAdd() : base(PomlPatchOperation.Add) { }
    }

    internal static class PomlPatchParser
    {
        public static bool TryParse(string json, out PomlPatch patch)
        {
            patch = null;

            try
            {
                var jObj = JObject.Parse(json);

                if (TryGetOperation(jObj, out var operation) == false)
                {
                    return false;
                }

                TryGetTarget(jObj, out var target);

                switch (operation)
                {
                    case PomlPatch.PomlPatchOperation.Add:
                        patch = new PomlPatchAdd()
                        {
                            Target = target
                        };
                        break;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                patch = null;
                return false;
            }
        }

        private static bool TryGetOperation(JObject jObj, out PomlPatch.PomlPatchOperation operation)
        {
            if (jObj.TryGetString("operation", out var operationString) == false)
            {
                operation = PomlPatch.PomlPatchOperation.None;
                return false;
            }

            return EnumLabel.TryGetValue(operationString, out operation);
        }

        private static bool TryGetTarget(JObject jObj, out PomlPatch.PomlPatchTarget target)
        {
            if (jObj.TryGetJObject("target", out var targetJObject) == false)
            {
                target = null;
                return false;
            }

            targetJObject.TryGetString("id", out var id);
            targetJObject.TryGetString("tag", out var tag);

            target = new PomlPatch.PomlPatchTarget()
            {
                Id = id,
                Tag = tag
            };
            return true;
        }

        private static bool TryGetAttributes(JObject jObj, out JObject attributes)
        {
            return jObj.TryGetJObject("attributes", out attributes);
        }

        private static PomlPatchAddElement[] GetChildren(JObject jObj)
        {
            if (jObj.TryGetValue("children", out var childrenToken) == false || childrenToken.Type != JTokenType.Array)
            {
                return Array.Empty<PomlPatchAddElement>();
            }

            var children = new List<PomlPatchAddElement>();

            foreach (var child in childrenToken)
            {
                if (child is JObject childObj)
                {
                    if (TryGetPomlPatchAddElement(childObj, out var element))
                    {
                        children.Add(element);
                    }
                }
            }

            return children.ToArray();
        }

        public static bool TryGetPomlPatchAddElement(JObject jObj, out PomlPatchAddElement element)
        {
            if (jObj.TryGetString("tag", out var tag) == false)
            {
                element = null;
                return false;
            }

            TryGetAttributes(jObj, out var attributes);

            var children = GetChildren(jObj);

            element = new PomlPatchAddElement()
            {
                Tag = tag,
                Attributes = attributes,
                Children = children
            };
            return true;
        }
    }

    internal static class NewtonsoftJsonExtensions
    {
        public static bool TryGetString(this JObject jObj, string propertyName, out string value)
        {
            if (jObj.TryGetValue(propertyName, out var token) && token.Type == JTokenType.String)
            {
                value = token.ToString();
                return true;
            }

            value = null;
            return false;
        }

        public static bool TryGetJObject(this JObject jObj, string propertyName, out JObject value)
        {
            if (jObj.TryGetValue(propertyName, out var token) && token.Type == JTokenType.Object)
            {
                value = token as JObject;
                return true;
            }

            value = null;
            return false;
        }
    }
}
