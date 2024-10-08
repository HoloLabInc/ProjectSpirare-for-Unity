﻿using Cysharp.Threading.Tasks;
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

        public int ElementCount => _elementStore.ElementCount;

        public string Url => _url;

        internal void Initialize(ElementStore contents, Poml poml, PomlLoader pomlLoader, string url)
        {
            _elementStore = contents ?? throw new ArgumentNullException(nameof(contents));
            _poml = poml ?? throw new ArgumentNullException(nameof(poml));
            _pomlLoader = pomlLoader ?? throw new ArgumentNullException(nameof(pomlLoader));
            _url = url;
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
                Debug.LogWarning("scene root cannot be removed");
                return;
            }
            else
            {
                parent.Children.Remove(pomlElement);
            }

            _elementStore.RemoveElement(pomlElementComponent);

            await UniTask.SwitchToMainThread();
            Destroy(pomlElementComponent.gameObject);
        }

        internal bool TryGetPomlElementById(string id, out PomlElement pomlElement)
        {
            if (_elementStore.TryGetElementById(id, out var pomlElementComponent))
            {
                pomlElement = pomlElementComponent.PomlElement;
                return true;
            }

            pomlElement = null;
            return false;
        }

        internal bool TryGetPomlElementComponentByTag(string tag, out PomlElementComponent pomlElementComponent)
        {
            if (TryGetPomlElementByTag(tag, out var pomlElement) == false)
            {
                pomlElementComponent = null;
                return false;
            }

            return _elementStore.TryGetPomlElementComponentByPomlElement(pomlElement, out pomlElementComponent);
        }

        internal bool TryGetPomlElementByTag(string tag, out PomlElement pomlElement)
        {
            if (EnumLabel.TryGetValue(tag, out PomlElementType elementType) == false)
            {
                pomlElement = null;
                return false;
            }

            return TryGetElementByElementTypeRecursively(elementType, _poml.Scene, out pomlElement);
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
