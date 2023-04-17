using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Wasm.Core
{
    internal static class MemoryHelper
    {
        public static bool TryReadBytes(IntPtr memoryPtr, uint memoryLength, int pointer, int length, out byte[] value)
            => TryReadBytes(memoryPtr, memoryLength, (uint)pointer, length, out value);

        public static bool TryReadBytes(IntPtr memoryPtr, uint memoryLength, uint pointer, int length, out byte[] value)
        {
            if (TryGetSlice(memoryPtr, memoryLength, pointer, length, out var slice) == false)
            {
                value = default;
                return false;
            }
            try
            {
                if (length == 0)
                {
                    value = Array.Empty<byte>();
                }
                else
                {
                    value = new byte[length];
                    unsafe
                    {
                        fixed (byte* dest = value)
                        {
                            Buffer.MemoryCopy((byte*)memoryPtr + pointer, dest, length, length);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                value = default;
                return false;
            }
        }

        public static bool TryRead<T>(IntPtr memoryPtr, uint memoryLength, int pointer, out T value) where T : unmanaged
            => TryRead(memoryPtr, memoryLength, (uint)pointer, out value);

        public unsafe static bool TryRead<T>(IntPtr memoryPtr, uint memoryLength, uint pointer, out T value)
            where T : unmanaged
        {
            if (memoryPtr == IntPtr.Zero || memoryLength == 0 ||
                pointer >= memoryLength || pointer + sizeof(T) > memoryLength)
            {
                value = default;
                return false;
            }
            value = *(T*)((byte*)memoryPtr + pointer);
            return true;
        }

        public static unsafe bool TryReadUtf8(IntPtr memoryPtr, uint memoryLength, int pointer, int length, out string text)
        {
            if (memoryPtr == IntPtr.Zero || memoryLength == 0 || length < 0 ||
                pointer >= memoryLength || pointer + length > memoryLength)
            {
                text = default;
                return false;
            }
            byte* str = (byte*)memoryPtr + pointer;
            if (length == 0)
            {
                text = "";
                return true;
            }
            try
            {
                text = Encoding.UTF8.GetString(str, length);
                return true;
            }
            catch (Exception)
            {
                text = null;
                return false;
            }
        }

        public unsafe static bool TryReadVectoredBufferToBytes(IntPtr memoryPtr, uint memoryLength, int iovs, int iovsLen, out byte[] data)
        {
            var slices = new (IntPtr Ptr, uint Length)[iovsLen];
            if (TryGetVectoredBufferSlice(memoryPtr, memoryLength, iovs, ref slices) == false)
            {
                data = default;
                return false;
            }
            ulong dataLen = 0;
            for (int i = 0; i < slices.Length; i++)
            {
                dataLen += slices[i].Length;
            }
            try
            {
                data = new byte[dataLen];
                fixed (byte* dest = data)
                {
                    uint offset = 0;
                    for (int i = 0; i < slices.Length; i++)
                    {
                        Buffer.MemoryCopy((void*)slices[i].Ptr, dest + offset, (uint)data.Length - offset, slices[i].Length);
                        offset += slices[i].Length;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                data = default;
                return false;
            }
        }

        public unsafe static bool TryReadVectoredBuffer(IntPtr memoryPtr, uint memoryLength, int iovs, int iovsLen, out IList<ArraySegment<byte>> data)
        {
            var slices = new (IntPtr Ptr, uint Length)[iovsLen];
            if (TryGetVectoredBufferSlice(memoryPtr, memoryLength, iovs, ref slices) == false)
            {
                data = default;
                return false;
            }
            data = new ArraySegment<byte>[slices.Length];
            try
            {
                for (int i = 0; i < slices.Length; i++)
                {
                    var array = new byte[slices[i].Length];
                    fixed (byte* dest = array)
                    {
                        Buffer.MemoryCopy((void*)slices[i].Ptr, dest, array.Length, slices[i].Length);
                    }
                    data[i] = new ArraySegment<byte>(array);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                return false;
            }
        }

        private static bool TryGetSlice(IntPtr memoryPtr, uint memoryLength, uint pointer, int length, out IntPtr slice)
        {
            if (memoryPtr == IntPtr.Zero || memoryLength == 0 || length < 0 ||
                pointer >= memoryLength || pointer + length > memoryLength)
            {
                slice = default;
                return false;
            }
            if (length == 0)
            {
                slice = memoryPtr;
                return true;
            }
            unsafe
            {
                slice = new IntPtr((byte*)memoryPtr + pointer);
            }
            return true;
        }

        private static bool TryGetVectoredBufferSlice(IntPtr memoryPtr, uint memoryLength, int iovs, ref (IntPtr Ptr, uint Length)[] slices)
        {
            for (var i = 0; i < slices.Length; i++)
            {
                if (TryRead(memoryPtr, memoryLength, iovs + i * 8, out int startOffset) == false)
                {
                    return false;
                }
                if (TryRead(memoryPtr, memoryLength, iovs + i * 8 + 4, out int length) == false)
                {
                    return false;
                }
                if (TryGetSlice(memoryPtr, memoryLength, (uint)startOffset, length, out var buffer) == false)
                {
                    return false;
                }
                slices[i] = (Ptr: buffer, Length: (uint)length);
            }
            return true;
        }

        public unsafe static bool TryWrite<T>(IntPtr memoryPtr, uint memoryLength, int pointer, T value) where T : unmanaged
            => TryWrite(memoryPtr, memoryLength, (uint)pointer, value);

        public unsafe static bool TryWrite<T>(IntPtr memoryPtr, uint memoryLength, uint pointer, T value) where T : unmanaged
        {
            // TODO: check

            *(T*)((byte*)memoryPtr + pointer) = value;
            return true;
        }

        public unsafe static bool TryWriteUtf8(IntPtr memoryPtr, uint memoryLength, string text, ref uint offset, bool addNullTermination = true)
        {
            // TODO: check

            try
            {
                byte* mem = (byte*)memoryPtr;
                var byteLen = Encoding.UTF8.GetByteCount(text);
                fixed (char* textPtr = text)
                {
                    offset += (uint)Encoding.UTF8.GetBytes(textPtr, text.Length, mem + offset, byteLen);
                }
                if (addNullTermination)
                {
                    mem[offset] = (byte)'\0';
                    offset++;
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }

        public unsafe static bool TryWriteArray<T>(IntPtr memoryPtr, uint memoryLength, int pointer, T[] array) where T : unmanaged
            => TryWriteArray(memoryPtr, memoryLength, (uint)pointer, array);
        public unsafe static bool TryWriteArray<T>(IntPtr memoryPtr, uint memoryLength, uint pointer, T[] array) where T : unmanaged
        {
            // TODO: check

            if (array == null)
            {
                return false;
            }
            fixed (T* source = array)
            {
                TryWriteArray(memoryPtr, memoryLength, pointer, source, (uint)array.Length);
            }
            return true;
        }

        public unsafe static bool TryWriteArray<T>(IntPtr memoryPtr, uint memoryLength, uint pointer, T* array, uint arrayLength)
            where T : unmanaged
        {
            // TODO: check

            if (array == null && arrayLength == 0)
            {
                return true;
            }
            var dest = (byte*)memoryPtr + pointer;
            var destlen = memoryLength - pointer;
            var lengthToCopy = arrayLength * (ulong)sizeof(T);
            Buffer.MemoryCopy(array, dest, destlen, lengthToCopy);
            return true;
        }
    }
}
