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
        private readonly Func<string, (PomlElementComponent Component, PomlElement Element)> _finder;
        private readonly (PomlElementComponent Component, PomlElement Element) _target;
        private readonly bool _hasTarget;

        private NetWebSocketClient wsClient;

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

        private async void MessageReceived(string json)
        {
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
            if (component == null || element == null) { return; }

            var elementType = element.GetType();
            var updated = false;
            foreach (var prop in obj.Properties())
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
                component.InvokeElementUpdated();
            }
        }
    }
}
