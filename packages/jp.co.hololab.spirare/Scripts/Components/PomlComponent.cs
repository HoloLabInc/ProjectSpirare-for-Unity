using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    public sealed class PomlComponent : MonoBehaviour
    {
        private ElementStore _elementStore;
        private Poml _poml;
        private WebSocketHelper _webSocket;

        public int ElementCount => _elementStore.ElementCount;

        private void Start()
        {
            StartWebSocket();
        }

        private async void StartWebSocket()
        {
            var wsRecvUrl = _poml.Scene.WsRecvUrl;
            if (string.IsNullOrEmpty(wsRecvUrl))
            {
                return;
            }
            var ct = this.GetCancellationTokenOnDestroy();
            // _webSocket = new WebSocketHelper(id => GetElementById(id));
            _webSocket = new WebSocketHelper(this);
            await _webSocket.Connect(wsRecvUrl, ct);
        }

        internal PomlComponent Initialize(ElementStore contents, Poml poml)
        {
            _elementStore = contents ?? throw new ArgumentNullException(nameof(contents));
            _poml = poml ?? throw new ArgumentNullException(nameof(poml));
            return this;
        }

        public IEnumerable<PomlElementComponent> GetAllElements()
        {
            return _elementStore.GetAllElements();
        }

        public bool TryGetElementById(string id, out (PomlElementComponent Component, PomlElement Element) element)
        {
            if (_elementStore.TryGetElementById(id, out var elementComponent) == false)
            {
                element = default;
                return false;
            }

            element = (elementComponent, elementComponent.PomlElement);
            return true;
        }

        public (PomlElementComponent Component, PomlElement Element) GetElementById(string id)
        {
            if (TryGetElementById(id, out var element))
            {
                return element;
            }
            return default;
        }

        internal (int ElementDescriptor, PomlElementComponent Component)[] GetAllElementsWithDescriptor()
        {
            return _elementStore.GetAllElementsWithDescriptor();
        }

        internal bool TryGetElementById(string id, out PomlElementComponent component, out int elemDescr)
        {
            return _elementStore.TryGetElementById(id, out component, out elemDescr);
        }

        internal bool TryGetElementByTag(string tag, out (PomlElementComponent Component, PomlElement Element) element)
        {
            throw new NotImplementedException();
        }

        internal bool TryGetElementByDescriptor(int elemDescr, out PomlElementComponent elemComp)
        {
            return _elementStore.TryGetElementByDescriptor(elemDescr, out elemComp);
        }

        internal void RegisterComponent(PomlElementComponent elementComponent)
        {
            if (elementComponent == null)
            {
                return;
            }

            elementComponent.OnDestroyed += PomlElementComponentBase_OnDestroyed;
            _elementStore.RegisterElement(elementComponent);
        }

        private void PomlElementComponentBase_OnDestroyed(PomlElementComponent elementComponent)
        {
            _elementStore.RemoveElement(elementComponent);
        }
    }
}
