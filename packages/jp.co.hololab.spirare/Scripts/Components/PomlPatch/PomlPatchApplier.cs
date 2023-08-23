using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace HoloLab.Spirare
{
    public sealed class PomlPatchApplier
    {
        private readonly PomlComponent pomlComponent;
        private readonly object defaultTargetPomlElement;
        private readonly Component defaultTargetPomlElementComponent;
        private readonly string basePath;

        public PomlPatchApplier(PomlComponent pomlComponent, object defaultTargetPomlElement, Component defaultTargetPomlElementComponent, string basePath)
        {
            this.pomlComponent = pomlComponent;
            this.defaultTargetPomlElement = defaultTargetPomlElement;
            this.defaultTargetPomlElementComponent = defaultTargetPomlElementComponent;
            this.basePath = basePath;
        }

        internal async UniTask ApplyPomlPatchAsync(string json)
        {
            if (PomlPatchParser.TryParse(json, out var patches) == false)
            {
                return;
            }

            await UniTask.WhenAll(patches.Select(patch => ApplyPomlPatchAsync(patch)));
        }

        internal async UniTask ApplyPomlPatchAsync(PomlPatch patch)
        {
            switch (patch)
            {
                case PomlPatchAdd patchAdd:
                    await ApplyPomlPatchAdd(patchAdd);
                    break;
                case PomlPatchUpdate patchUpdate:
                    ApplyPomlPatchUpdate(patchUpdate);
                    break;
                case PomlPatchRemove patchRemove:
                    await ApplyPomlPatchRemove(patchRemove);
                    break;
            }
        }

        private bool TryGetTargetPomlElement(PomlPatch.PomlPatchTarget target, out object pomlElement)
        {
            if (target == null)
            {
                pomlElement = defaultTargetPomlElement;
                return true;
            }

            if (string.IsNullOrEmpty(target.Id) == false)
            {
                if (pomlComponent.TryGetElementById(target.Id, out var element))
                {
                    pomlElement = element.Element;
                    return true;
                }
            }

            if (string.IsNullOrEmpty(target.Tag) == false)
            {
                if (pomlComponent.TryGetPomlElementByTag(target.Tag, out var element))
                {
                    pomlElement = element;
                    return true;
                }
            }

            pomlElement = null;
            return false;
        }

        private bool TryGetTargetPomlElementComponent(PomlPatch.PomlPatchTarget target, out Component elementComponent)
        {
            if (target == null)
            {
                elementComponent = defaultTargetPomlElementComponent;
                return true;
            }

            if (string.IsNullOrEmpty(target.Id) == false)
            {
                if (pomlComponent.TryGetElementById(target.Id, out var element))
                {
                    elementComponent = element.Component;
                    return true;
                }
            }

            if (string.IsNullOrEmpty(target.Tag) == false)
            {
                return pomlComponent.TryGetElementComponentByTag(target.Tag, out elementComponent);
            }

            elementComponent = null;
            return false;
        }

        private async UniTask ApplyPomlPatchAdd(PomlPatchAdd patch)
        {
            if (TryGetTargetPomlElement(patch.Target, out var targetPomlElementObject) == false)
            {
                return;
            }

            var pomlElement = ConvertPomlPatchAddElementToPomlElement(patch.Element, parentElement: null, basePath);

            if (targetPomlElementObject is PomlScene)
            {
                await pomlComponent.AppendElementToSceneAsync(pomlElement);
            }
            else if (targetPomlElementObject is PomlElement targetPomlElement)
            {
                await pomlComponent.AppendElementAsync(pomlElement, parentElement: targetPomlElement);
            }
        }

        private void ApplyPomlPatchUpdate(PomlPatchUpdate patch)
        {
            if (TryGetTargetPomlElementComponent(patch.Target, out var targetElementComponent) == false)
            {
                return;
            }

            UpdateAttributes(targetElementComponent, patch.Attributes, basePath);
        }

        private async UniTask ApplyPomlPatchRemove(PomlPatchRemove patch)
        {
            if (TryGetTargetPomlElement(patch.Target, out var targetPomlElementObject) == false)
            {
                return;
            }

            if (targetPomlElementObject is PomlElement targetPomlElement)
            {
                await pomlComponent.RemoveElementAsync(targetPomlElement);
            }
        }

        private static PomlElement ConvertPomlPatchAddElementToPomlElement(PomlPatchAddElement addElement, PomlElement parentElement, string basePath)
        {
            var pomlElement = CreatePomlElement(addElement.ElementType);
            if (parentElement != null)
            {
                pomlElement.Parent = parentElement;
            }

            // Set attributes to pomlElement
            UpdatePomlElementAttributes(pomlElement, addElement.Attributes, basePath);

            var children = addElement.Children
                .Select(x => ConvertPomlPatchAddElementToPomlElement(x, pomlElement, basePath))
                .ToList();
            pomlElement.Children = children;

            return pomlElement;
        }

        private static PomlElement CreatePomlElement(PomlElementType pomlElementType)
        {
            switch (pomlElementType)
            {
                case PomlElementType.Element:
                    return new PomlEmptyElement();
                case PomlElementType.Model:
                    return new PomlModelElement();
                case PomlElementType.Text:
                    return new PomlTextElement("");
                case PomlElementType.Image:
                    return new PomlImageElement();
                case PomlElementType.Video:
                    return new PomlVideoElement();
                case PomlElementType.Audio:
                    return new PomlAudioElement();
                case PomlElementType.Geometry:
                    return new PomlGeometryElement();
                case PomlElementType.Cesium3dTiles:
                    return new PomlCesium3dTilesElement();
                case PomlElementType.Script:
                    return new PomlScriptElement();
                case PomlElementType.SpaceReference:
                    return new PomlSpaceReferenceElement();
                case PomlElementType.GeoReference:
                    return new PomlGeoReferenceElement();
                case PomlElementType.ScreenSpace:
                    return new PomlScreenSpaceElement();
                default:
                    return null;
            }
        }

        private static void UpdateAttributes(UnityEngine.Object component, JObject attributes, string basePath)
        {
            if (attributes == null)
            {
                return;
            }

            var elementComponent = component as PomlElementComponent;
            if (elementComponent == null)
            {
                return;
            }

            var element = elementComponent.PomlElement;
            var updated = UpdatePomlElementAttributes(element, attributes, basePath);

            if (updated)
            {
                elementComponent.InvokeElementUpdated();
            }
        }

        private static bool UpdatePomlElementAttributes(PomlElement element, JObject attributes, string basePath)
        {
            if (attributes == null)
            {
                return false;
            }

            var elementType = element.GetType();
            var updated = false;
            foreach (var prop in attributes.Properties())
            {
                var propValue = prop.Value;
                if (prop.Name.ToLower() == "src")
                {
                    var absolutePath = FilePathUtility.GetAbsolutePath(propValue.ToString(), basePath);
                    propValue = JToken.FromObject(absolutePath);
                }

                try
                {
                    var propName = char.ToUpper(prop.Name[0]) + prop.Name.Substring(1);

                    var propInfo = elementType.GetProperty(propName);
                    if (propInfo != null)
                    {
                        var type = propInfo.PropertyType;
                        if (type.IsAbstract == false)
                        {
                            var value = propValue.ToObject(type);
                            propInfo.SetValue(element, value);
                            updated = true;
                        }
                        continue;
                    }

                    var fieldInfo = elementType.GetField(propName);
                    if (fieldInfo != null)
                    {
                        var type = fieldInfo.FieldType;
                        if (type.IsAbstract == false)
                        {
                            var value = propValue.ToObject(type);
                            fieldInfo.SetValue(element, value);
                            updated = true;
                        }
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            return updated;
        }
    }
}
