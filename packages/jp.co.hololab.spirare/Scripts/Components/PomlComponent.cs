using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public sealed class PomlComponent : MonoBehaviour
    {
        private ElementStore _elementStore;
        private Poml _poml;
        private PomlLoader _pomlLoader;
        private string _url;

        private PomlPatchApplier _patchApplier;
        private WebSocketHelper _webSocket;

        public int ElementCount => _elementStore.ElementCount;

        public string Url => _url;

        internal void Initialize(ElementStore contents, Poml poml, PomlLoader pomlLoader, string url)
        {
            _elementStore = contents ?? throw new ArgumentNullException(nameof(contents));
            _poml = poml ?? throw new ArgumentNullException(nameof(poml));
            _pomlLoader = pomlLoader ?? throw new ArgumentNullException(nameof(pomlLoader));
            _url = url;

            _patchApplier = new PomlPatchApplier(this, _poml.Scene, this, url);
        }

        private void Start()
        {
            StartWebSocket().Forget();
        }

        private async UniTask StartWebSocket()
        {
            var wsRecvUrl = _poml.Scene.WsRecvUrl;
            if (string.IsNullOrEmpty(wsRecvUrl))
            {
                return;
            }
            var ct = this.GetCancellationTokenOnDestroy();
            _webSocket = new WebSocketHelper(_patchApplier);
            await _webSocket.Connect(wsRecvUrl, ct);
        }

        #region Public methods

        public IEnumerable<PomlElementComponent> GetAllPomlElementComponents()
        {
            return _elementStore.GetAllElements();
        }

        public bool TryGetPomlElementComponentById(string id, out PomlElementComponent pomlElementComponent)
        {
            return _elementStore.TryGetElementById(id, out pomlElementComponent);
        }

        [Obsolete("Please use GetAllPomlElementComponents instead of this method.")]
        public IEnumerable<PomlElementComponent> GetAllElements()
        {
            return _elementStore.GetAllElements();
        }

        [Obsolete("Please use TryGetPomlElementComponentById instead of this method.")]
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

        [Obsolete("Please use TryGetPomlElementComponentById instead of this method.")]
        public (PomlElementComponent Component, PomlElement Element) GetElementById(string id)
        {
            if (TryGetElementById(id, out var element))
            {
                return element;
            }
            return default;
        }

        #endregion

        internal async Task AppendElementToSceneAsync(PomlElement pomlElement)
        {
            await UniTask.SwitchToMainThread();

            _poml.Scene.Children.Add(pomlElement);
            await _pomlLoader.LoadElement(pomlElement, transform, null, this);
        }

        internal async Task AppendElementAsync(PomlElement pomlElement, PomlElement parentElement)
        {
            await UniTask.SwitchToMainThread();

            if (_elementStore.TryGetPomlElementComponentByPomlElement(parentElement, out var parentElementComponent) == false)
            {
                Debug.LogWarning("parentElement not found");
                return;
            }

            pomlElement.Parent = parentElement;
            parentElement.Children.Add(pomlElement);

            var parentObjectElementComponent = parentElementComponent as PomlObjectElementComponent;
            await _pomlLoader.LoadElement(pomlElement, parentElementComponent.transform, parentObjectElementComponent, this);
        }

        internal async Task RemoveElementAsync(PomlElement pomlElement)
        {
            if (_elementStore.TryGetPomlElementComponentByPomlElement(pomlElement, out var pomlElementComponent) == false)
            {
                Debug.LogWarning("parentElement not found");
                return;
            }

            var parent = pomlElement.Parent;
            if (parent == null)
            {
                _poml.Scene.Children.Remove(pomlElement);
            }
            else
            {
                parent.Children.Remove(pomlElement);
            }

            _elementStore.RemoveElement(pomlElementComponent);

            await UniTask.SwitchToMainThread();
            Destroy(pomlElementComponent.gameObject);
        }

        internal bool TryGetPomlElementComponentByTag(string tag, out Component pomlComponentOrPomlElementComponent)
        {
            // TODO: the type of pomlComponentOrPomlElementComponent should be PomlElementComponent

            if (tag == "scene")
            {
                pomlComponentOrPomlElementComponent = this;
                return true;
            }

            if (EnumLabel.TryGetValue(tag, out PomlElementType elementType))
            {
                foreach (var element in _poml.Scene.Children)
                {
                    if (TryGetElementByElementTypeRecursively(elementType, element, out var pomlElement))
                    {
                        var result = _elementStore.TryGetPomlElementComponentByPomlElement(pomlElement, out var pomlElementComponent);
                        pomlComponentOrPomlElementComponent = pomlElementComponent;
                        return result;
                    }
                }
            }

            // not found
            pomlComponentOrPomlElementComponent = null;
            return false;
        }

        internal bool TryGetPomlElementByTag(string tag, out object pomlElement)
        {
            if (tag == "scene")
            {
                pomlElement = _poml.Scene;
                return true;
            }

            if (EnumLabel.TryGetValue(tag, out PomlElementType elementType))
            {
                foreach (var element in _poml.Scene.Children)
                {
                    if (TryGetElementByElementTypeRecursively(elementType, element, out var foundPomlElement))
                    {
                        pomlElement = foundPomlElement;
                        return true;
                    }
                }
            }

            pomlElement = null;
            return false;
        }

        internal (int ElementDescriptor, PomlElementComponent Component)[] GetAllElementsWithDescriptor()
        {
            return _elementStore.GetAllElementsWithDescriptor();
        }

        internal bool TryGetElementById(string id, out PomlElementComponent component, out int elemDescr)
        {
            return _elementStore.TryGetElementById(id, out component, out elemDescr);
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

        private static bool TryGetElementByElementTypeRecursively(PomlElementType elementType, PomlElement targetElement, out PomlElement pomlElement)
        {
            if (targetElement.ElementType == elementType)
            {
                pomlElement = targetElement;
                return true;
            }

            foreach (var child in targetElement.Children)
            {
                if (TryGetElementByElementTypeRecursively(elementType, child, out pomlElement))
                {
                    return true;
                }
            }

            pomlElement = null;
            return false;
        }
    }
}
