using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace HoloLab.Spirare
{
    public sealed class PomlPatchApplier
    {
        private readonly PomlComponent pomlComponent;
        private readonly UnityEngine.Object defaultTarget;

        public PomlPatchApplier(PomlComponent pomlComponent)
        {
            this.pomlComponent = pomlComponent;
        }

        public PomlPatchApplier(PomlComponent pomlComponent, UnityEngine.Object defaultTarget) : this(pomlComponent)
        {
            this.defaultTarget = defaultTarget;
        }

        internal void ApplyPomlPatch(string json)
        {
            if (PomlPatchParser.TryParse(json, out var patches) == false)
            {
                return;
            }

            foreach (var patch in patches)
            {
                ApplyPomlPatch(patch);
            }
        }

        internal void ApplyPomlPatch(PomlPatch patch)
        {
            UnityEngine.Object targetComponent;

            if (patch.Target == null)
            {
                targetComponent = defaultTarget;
            }
            else
            {
                if (TryGetTargetElementComponent(patch.Target, out targetComponent) == false)
                {
                    return;
                }
            }

            switch (patch)
            {
                case PomlPatchAdd patchAdd:
                    ApplyPomlPatchAdd(patchAdd);
                    break;
                case PomlPatchUpdate patchUpdate:
                    ApplyPomlPatchUpdate(patchUpdate, targetComponent);
                    break;
                case PomlPatchRemove patchRemove:
                    ApplyPomlPatchRemove(patchRemove);
                    break;
            }
        }

        private void ApplyPomlPatchAdd(PomlPatchAdd patch)
        {
            throw new NotImplementedException();
        }

        private void ApplyPomlPatchUpdate(PomlPatchUpdate patchUpdate, UnityEngine.Object component)
        {
            UpdateAttributes(component, patchUpdate.Attributes);
        }

        private void ApplyPomlPatchRemove(PomlPatchRemove patchRemove)
        {
            throw new NotImplementedException();
        }

        private bool TryGetTargetElementComponent(PomlPatch.PomlPatchTarget target, out UnityEngine.Object elementComponent)
        {
            if (target == null)
            {
                elementComponent = null;
                return false;
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
                return pomlComponent.TryGetElementByTag(target.Tag, out elementComponent);
            }

            elementComponent = null;
            return false;
        }

        private void UpdateAttributes(UnityEngine.Object component, JObject attributes)
        {
            if (attributes == null)
            {
                return;
            }

            var elementComponent = component as PomlElementComponent;
            if (component == null)
            {
                return;
            }

            var element = elementComponent.PomlElement;
            var elementType = element.GetType();
            var updated = false;
            foreach (var prop in attributes.Properties())
            {
                try
                {
                    var propName = char.ToUpper(prop.Name[0]) + prop.Name.Substring(1);

                    var propInfo = elementType.GetProperty(propName);
                    if (propInfo != null)
                    {
                        var type = propInfo.PropertyType;
                        if (type.IsAbstract == false)
                        {
                            var value = prop.Value.ToObject(type);
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
                            var value = prop.Value.ToObject(type);
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
            if (updated)
            {
                elementComponent.InvokeElementUpdated();
            }
        }
    }
}
