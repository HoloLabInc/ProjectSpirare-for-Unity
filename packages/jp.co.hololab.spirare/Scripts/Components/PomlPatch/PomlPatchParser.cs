using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    internal static class PomlPatchParser
    {
        public static bool TryParse(string json, out PomlPatch[] patches)
        {
            json = json.TrimStart();
            if (json.StartsWith("["))
            {
                try
                {
                    var patchList = new List<PomlPatch>();

                    var jArray = JArray.Parse(json);
                    foreach (JObject jObj in jArray)
                    {
                        if (TryParse(jObj, out var patch))
                        {
                            patchList.Add(patch);
                        }
                    }

                    patches = patchList.ToArray();
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
            else
            {
                try
                {
                    var jObj = JObject.Parse(json);
                    if (TryParse(jObj, out var patch))
                    {
                        patches = new PomlPatch[] { patch };
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            patches = Array.Empty<PomlPatch>();
            return false;
        }

        private static bool TryParse(JObject jObj, out PomlPatch patch)
        {
            patch = null;

            if (TryGetOperation(jObj, out var operation) == false)
            {
                return false;
            }

            TryGetTarget(jObj, out var target);

            switch (operation)
            {
                case PomlPatch.PomlPatchOperation.Add:
                    if (TryGetPomlPatchAddElement(jObj, out var addElement) == false)
                    {
                        return false;
                    }
                    patch = new PomlPatchAdd()
                    {
                        Target = target,
                        Element = addElement
                    };
                    return true;
                case PomlPatch.PomlPatchOperation.Update:
                    patch = new PomlPatchUpdate()
                    {
                        Target = target,
                        Attributes = GetAttributes(jObj)
                    };
                    return true;
                case PomlPatch.PomlPatchOperation.Remove:
                    patch = new PomlPatchRemove()
                    {
                        Target = target
                    };
                    return true;
                default:
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

        private static JObject GetAttributes(JObject jObj)
        {
            if (jObj.TryGetJObject("attributes", out var attributes))
            {
                return attributes;
            }
            return null;
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

            var attributes = GetAttributes(jObj);
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
