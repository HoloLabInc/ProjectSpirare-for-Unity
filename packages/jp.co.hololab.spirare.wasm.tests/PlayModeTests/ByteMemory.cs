using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HoloLab.Spirare.Wasm.Tests
{
    [DebuggerTypeProxy(typeof(ByteMemoryProxy))]
    [DebuggerDisplay("byte[{_length}]")]
    internal sealed unsafe class ByteMemory : IDisposable
    {
        private IntPtr _ptr;
        private int _length;

        public IntPtr Ptr => _ptr;
        public int Length => _length;

        public ref byte this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_length)
                {
                    ThrowArgException(nameof(index));
                }
                return ref *((byte*)_ptr + index);
            }
        }

        public ByteMemory(int byteSize, bool zeroFill = true)
        {
            if (byteSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteSize));
            }
            if (byteSize == 0)
            {
                _ptr = IntPtr.Zero;
                _length = 0;
                return;
            }
            _ptr = Marshal.AllocCoTaskMem(byteSize);
            _length = byteSize;

            if (zeroFill)
            {
                // Ideally, we should call a method to clear the memory rather than initializing it in a loop,
                // but there is no method that takes a pointer as an argument in .Net Standard 2.0.
                // Even if you implement your own loop, there are an infinite number of things to worry about,
                // such as vectorizing, clearing 8 bytes at a time using ulong, or loop unrolling.
                // However, this type is only used as a helper within tests, so speed is not a concern.
                // We will clear one byte at a time.
                byte* p = (byte*)_ptr;
                for (int i = 0; i < byteSize; i++)
                {
                    p[i] = 0;
                }
            }
        }

        public static ByteMemory CreateFilledWith(int byteSize, byte fillValue)
        {
            var mem = new ByteMemory(byteSize, false);
            byte* p = (byte*)mem._ptr;
            for (int i = 0; i < byteSize; i++)
            {
                p[i] = fillValue;
            }
            return mem;
        }

        ~ByteMemory() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_ptr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(_ptr);
                _length = 0;
                _ptr = IntPtr.Zero;
            }
        }

        public byte[] ToArray()
        {
            var array = new byte[_length];
            fixed (byte* dest = array)
            {
                Buffer.MemoryCopy((void*)_ptr, dest, _length, _length);
            }
            return array;
        }

        private static void ThrowArgException(string message) => throw new ArgumentException(message);

        private sealed class ByteMemoryProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public byte[] Items { get; }

            public ByteMemoryProxy(ByteMemory memory)
            {
                Items = memory.ToArray();
            }
        }
    }

}
