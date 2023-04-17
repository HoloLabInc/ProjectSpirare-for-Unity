using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HoloLab.Spirare.Wasm.Core.Spirare
{
    internal enum Errno
    {
        Success = 0,
        UnknownError = 1,
        InvalidArgument = 2,
        NotImplemented = 3,
        ElementNotFound = 4,
        InsufficientBufferSize = 5,
        UnsupportedOperation = 6,
    }

    internal enum SpecialElementDescriptor
    {
        SelfObject = 0,
        SelfScript = 1,
    }

    internal struct ElementInfo
    {
        public readonly int ElementDescriptor;
        public readonly int ElementType;

        public ElementInfo(int elementDescriptor, PomlElementType elementType)
        {
            ElementDescriptor = elementDescriptor;
            ElementType = (int)elementType;
        }

        public static ElementInfo InvalidElement => new ElementInfo(-1, PomlElementType.None);
    }

    internal enum EventName
    {
        Start = 0,
        Update = 1,
        Select = 2,
    }
}

