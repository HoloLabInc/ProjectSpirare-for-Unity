using System;

namespace HoloLab.Spirare.Wasm.Core.Spirare
{
    public sealed class ElementDescriptorHelper
    {
        private readonly PomlObjectElementComponent selfObjectElementComponent;
        private readonly PomlElementComponent selfScriptElementComponent;
        private readonly PomlComponent pomlComponent;

        public int ElementCount => pomlComponent.ElementCount;

        public ElementDescriptorHelper(PomlObjectElementComponent selfObjectElementComponent,
            PomlElementComponent selfScriptElementComponent, PomlComponent pomlComponent)
        {
            this.selfObjectElementComponent = selfObjectElementComponent;
            this.selfScriptElementComponent = selfScriptElementComponent;
            this.pomlComponent = pomlComponent;
        }

        internal (int ElementDescriptor, PomlElementComponent Component)[] GetAllElementsWithDescriptor()
        {
            return pomlComponent.GetAllElementsWithDescriptor();
        }

        internal bool TryGetElementComponent(int elementDescriptor, out PomlElementComponent elementComponent)
        {
            switch (elementDescriptor)
            {
                case (int)SpecialElementDescriptor.SelfObject:
                    elementComponent = selfObjectElementComponent;
                    return true;
                case (int)SpecialElementDescriptor.SelfScript:
                    elementComponent = selfScriptElementComponent;
                    return true;
                default:
                    return pomlComponent.TryGetElementByDescriptor(elementDescriptor, out elementComponent);
            }
        }

        internal bool TryGetElementById(string id, out PomlElementComponent component, out int elemDescr)
        {
            return pomlComponent.TryGetElementById(id, out component, out elemDescr);
        }


        internal bool TryGetElement<T>(int elemDescr, out PomlElementComponent elementComponent, out T pomlElement, out int errorCode)
            where T : PomlElement
        {
            if (TryGetElementComponent(elemDescr, out elementComponent) == false)
            {
                pomlElement = null;
                errorCode = (int)Errno.ElementNotFound;
                return false;
            }

            if (elementComponent.PomlElement is T element)
            {
                pomlElement = element;
                errorCode = (int)Errno.Success;
                return true;
            }
            else
            {
                pomlElement = null;
                errorCode = (int)Errno.UnsupportedOperation;
                return false;
            }
        }

        internal int SetAttribute(int elemDescr, Func<PomlElement, Errno> setAttributeFunc)
        {
            return SetAttribute<PomlElement>(elemDescr, setAttributeFunc);
        }

        internal int SetAttribute<T>(int elemDescr, Func<T, Errno> setAttributeFunc) where T : PomlElement
        {
            if (TryGetElement<T>(elemDescr, out var elementComponent, out var element, out var errorCode) == false)
            {
                return errorCode;
            }

            var err = setAttributeFunc(element);
            if (err == Errno.Success)
            {
                elementComponent.InvokeElementUpdated();
            }

            return (int)err;
        }

        internal int SetAttribute<T>(int elemDescr, Func<PomlElementComponent, T, Errno> setAttributeFunc) where T : PomlElement
        {
            if (TryGetElement<T>(elemDescr, out var elementComponent, out var element, out var errorCode) == false)
            {
                return errorCode;
            }

            var err = setAttributeFunc(elementComponent, element);
            if (err == Errno.Success)
            {
                elementComponent.InvokeElementUpdated();
            }

            return (int)err;
        }

        internal int GetAttribute(int elemDescr, Func<PomlElement, Errno> setAttributeFunc)
        {
            return GetAttribute<PomlElement>(elemDescr, setAttributeFunc);
        }

        internal int GetAttribute<T>(int elemDescr, Func<T, Errno> setAttributeFunc) where T : PomlElement
        {
            if (TryGetElement<T>(elemDescr, out var elementComponent, out var element, out var errorCode) == false)
            {
                return errorCode;
            }

            var err = setAttributeFunc(element);
            if (err == Errno.Success)
            {
                elementComponent.InvokeElementUpdated();
            }

            return (int)err;
        }
    }
}
