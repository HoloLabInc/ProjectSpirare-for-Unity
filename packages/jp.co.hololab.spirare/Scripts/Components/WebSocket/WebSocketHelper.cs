using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace HoloLab.Spirare
{
    public sealed class WebSocketHelper : IDisposable
    {
        // private readonly Func<string, (PomlElementComponent Component, PomlElement Element)> _finder;
        // private readonly (PomlElementComponent Component, PomlElement Element) _target;
        // private readonly bool _hasTarget;
        // private readonly PomlComponent pomlComponent;
        private readonly PomlPatchApplier patchApplier;

        private NetWebSocketClient wsClient;

        /*
        public WebSocketHelper(PomlElementComponent component, PomlElement element)
        {
            _target = (component, element);
            _hasTarget = true;
            _finder = null;
        }

        public WebSocketHelper(Func<string, (PomlElementComponent Component, PomlElement Element)> finder)
        {
            _target = (null, null);
            _hasTarget = false;
            _finder = finder;
        }
        */
        public WebSocketHelper(PomlPatchApplier patchApplier)
        {
            this.patchApplier = patchApplier;
        }

        public void Dispose()
        {
            wsClient?.Dispose();
        }

        /// <summary>Connects to the specified URL using WebSocket</summary>
        /// <param name="url">The URL of the target host (e.g. "ws://localhost:8000")</param>
        /// <returns></returns>
        public async Task<bool> Connect(string url, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return false;
            }

            Dispose();

            wsClient = new NetWebSocketClient(url);
            wsClient.OnMessageReceived += MessageReceived;
            var result = await wsClient.OpenAsync(ct);

            return result;
        }

        private void MessageReceived(string message)
        {
            patchApplier.ApplyPomlPatchAsync(message);
        }

#if FALSE
        private void MessageReceived(string json)
        {
            if (PomlPatchParser.TryParse(json, out var patches) == false)
            {
                return;
            }

            foreach (var patch in patches)
            {
                ApplyPomlPatch(patch);
            }
            /*
            json = json.TrimStart();
            if (json.StartsWith("["))
            {

                var result = TryParseToJArray(json, out var array);
                if (result)
                {
                    await UniTask.Yield();
                    foreach (JObject obj in array)
                    {
                        AssignJObject(obj);
                    }
                }
            }
            else if (json.StartsWith("{"))
            {
                var result = TryParseToJObject(json, out var obj);
                if (result)
                {
                    await UniTask.Yield();
                    AssignJObject(obj);
                }
            }
            */
        }

        private void ApplyPomlPatch(PomlPatch patch)
        {
            if (TryGetTargetElementComponent(patch.Target, out var elementComponent) == false)
            {
                return;
            }

            switch (patch)
            {
                case PomlPatchAdd patchAdd:
                    ApplyPomlPatchAdd(patchAdd);
                    break;
                case PomlPatchUpdate patchUpdate:
                    ApplyPomlPatchUpdate(patchUpdate, elementComponent);
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

        private bool TryParseToJObject(string json, out JObject obj)
        {
            try
            {
                obj = JObject.Parse(json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                obj = null;
                return false;
            }
        }

        private bool TryParseToJArray(string json, out JArray array)
        {
            try
            {
                array = JArray.Parse(json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                array = null;
                return false;
            }
        }

        /*
        private void AssignJObject(JObject obj)
        {
            PomlElementComponent component = null;
            PomlElement element = null;

            if (_hasTarget)
            {
                component = _target.Component;
                element = _target.Element;
            }
            else
            {
                if (obj.TryGetValue("id", out var idValue) && idValue.Type == JTokenType.String)
                {
                    var id = idValue.ToString();
                    (component, element) = _finder.Invoke(id);
                }
            }

            if (component == null || element == null)
            {
                return;
            }

            UpdateAttributes(component, element, obj);
        }
        */

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
#endif
    }
}
