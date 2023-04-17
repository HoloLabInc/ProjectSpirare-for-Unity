#nullable enable
using IwasmUnity;
using System;

namespace HoloLab.Spirare.Wasm.Iwasm
{
    internal static class BindHelper
    {
        internal static Func<ImportedContext, int> Bind<T1>(ApiFunc api) => new Func<ImportedContext, int>(
                (c) => api.Invoke(c.MemoryPtr, c.MemorySize)
                );
        internal static Func<ImportedContext, T1, int> Bind<T1>(ApiFunc<T1> api) => new Func<ImportedContext, T1, int>(
                (c, a1) => api.Invoke(c.MemoryPtr, c.MemorySize, a1)
                );
        internal static Func<ImportedContext, T1, T2, int> Bind<T1, T2>(ApiFunc<T1, T2> api) => new Func<ImportedContext, T1, T2, int>(
                (c, a1, a2) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2)
                );
        internal static Func<ImportedContext, T1, T2, T3, int> Bind<T1, T2, T3>(ApiFunc<T1, T2, T3> api) => new Func<ImportedContext, T1, T2, T3, int>(
                (c, a1, a2, a3) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, int> Bind<T1, T2, T3, T4>(ApiFunc<T1, T2, T3, T4> api) => new Func<ImportedContext, T1, T2, T3, T4, int>(
                (c, a1, a2, a3, a4) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, int> Bind<T1, T2, T3, T4, T5>(ApiFunc<T1, T2, T3, T4, T5> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, int>(
                (c, a1, a2, a3, a4, a5) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, int> Bind<T1, T2, T3, T4, T5, T6>(ApiFunc<T1, T2, T3, T4, T5, T6> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, int>(
                (c, a1, a2, a3, a4, a5, a6) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, int> Bind<T1, T2, T3, T4, T5, T6, T7>(ApiFunc<T1, T2, T3, T4, T5, T6, T7> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, int>(
                (c, a1, a2, a3, a4, a5, a6, a7) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6, a7)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, int> Bind<T1, T2, T3, T4, T5, T6, T7, T8>(ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, int>(
                (c, a1, a2, a3, a4, a5, a6, a7, a8) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6, a7, a8)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, int> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, int>(
                (c, a1, a2, a3, a4, a5, a6, a7, a8, a9) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6, a7, a8, a9)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, int> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, int>(
                (c, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, int> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, int>(
                (c, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, int> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, int>(
                (c, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, int> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, int>(
                (c, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, int> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, int>(
                (c, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14)
                );
        internal static Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, int> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> api) => new Func<ImportedContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, int>(
                (c, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15) => api.Invoke(c.MemoryPtr, c.MemorySize, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15)
                );
    }

    internal delegate int ApiFunc(IntPtr mem, uint memlen);
    internal delegate int ApiFunc<T1>(IntPtr mem, uint memlen, T1 a1);
    internal delegate int ApiFunc<T1, T2>(IntPtr mem, uint memlen, T1 a1, T2 a2);
    internal delegate int ApiFunc<T1, T2, T3>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3);
    internal delegate int ApiFunc<T1, T2, T3, T4>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6, T7>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14);
    internal delegate int ApiFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(IntPtr mem, uint memlen, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15);
}
