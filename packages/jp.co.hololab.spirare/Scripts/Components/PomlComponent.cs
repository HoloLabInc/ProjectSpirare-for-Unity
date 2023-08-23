using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private SynchronizationContext mainThreadContext;
        private int mainThreadId = -1;

        public int ElementCount => _elementStore.ElementCount;

        public string Url => _url;

        internal PomlComponent Initialize(ElementStore contents, Poml poml, PomlLoader pomlLoader, string url)
        {
            _elementStore = contents ?? throw new ArgumentNullException(nameof(contents));
            _poml = poml ?? throw new ArgumentNullException(nameof(poml));
            _pomlLoader = pomlLoader ?? throw new ArgumentNullException(nameof(pomlLoader));
            _url = url;

            _patchApplier = new PomlPatchApplier(this, defaultTarget: this, url);

            mainThreadContext = SynchronizationContext.Current;
            mainThreadId = Thread.CurrentThread.ManagedThreadId;

            return this;
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

        internal void AppendElementToScene(PomlElement pomlElement)
        {
            _poml.Scene.Elements.Add(pomlElement);

            RunInMainThread(async () =>
            {
                await _pomlLoader.LoadElement(pomlElement, transform, null, this);
            });
        }

        private void RunInMainThread(Action action)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            if (threadId == mainThreadId)
            {
                action.Invoke();
            }
            else
            {
                mainThreadContext.Send(_ =>
                {
                    action.Invoke();
                }, null);
            }
        }

        internal async Task AppendElementAsync(PomlElement pomlElement, PomlElement parentElement)
        {
            pomlElement.Parent = parentElement;
            throw new NotImplementedException();
        }

        internal bool TryGetElementByTag(string tag, out UnityEngine.Object pomlComponentOrPomlElementComponent)
        {
            // TODO: the type of pomlComponentOrPomlElementComponent should be PomlElementComponent

            if (tag == "scene")
            {
                pomlComponentOrPomlElementComponent = this;
                return true;
            }

            foreach (var element in _poml.Scene.Elements)
            {
                if (TryGetElementByTagRecursively(tag, element, out var pomlElement))
                {
                    var allElements = _elementStore.GetAllElements();
                    pomlComponentOrPomlElementComponent = allElements.FirstOrDefault(x => x.PomlElement == pomlElement);
                    return pomlComponentOrPomlElementComponent != null;
                }
            }

            // not found
            pomlComponentOrPomlElementComponent = null;
            return false;
        }

        private static bool TryGetElementByTagRecursively(string tag, PomlElement targetElement, out PomlElement pomlElement)
        {
            if (EnumLabel.TryGetLabel(targetElement.ElementType, out var targetElementTag) && targetElementTag == tag)
            {
                pomlElement = targetElement;
                return true;
            }

            foreach (var child in targetElement.Children)
            {
                if (TryGetElementByTagRecursively(tag, child, out pomlElement))
                {
                    return true;
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
    }
}
