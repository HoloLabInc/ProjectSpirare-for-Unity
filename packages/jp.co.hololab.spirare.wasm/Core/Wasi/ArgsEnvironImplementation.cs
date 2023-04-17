using System;
using System.Collections.Generic;

namespace HoloLab.Spirare.Wasm.Core.Wasi
{
    public sealed class ArgsEnvironImplementation
    {
        private readonly IReadOnlyList<string> args;
        private readonly IReadOnlyList<string> envs;

        public ArgsEnvironImplementation(IReadOnlyList<string> args, IReadOnlyList<string> envs)
        {
            this.args = args;
            this.envs = envs;
        }

#pragma warning disable IDE1006 // naming style
        public int args_get(IntPtr memoryPtr, uint memoryLength, int argv, int argvBuf)
        {
            return WriteStringList(memoryPtr, memoryLength, argv, argvBuf, args);
        }

        public int args_sizes_get(IntPtr memoryPtr, uint memoryLength, int sizePtr, int bufferSizePtr)
        {
            return WriteLength(memoryPtr, memoryLength, sizePtr, bufferSizePtr, args);
        }

        public int environ_get(IntPtr memoryPtr, uint memoryLength, int environ, int environBuf)
        {
            return WriteStringList(memoryPtr, memoryLength, environ, environBuf, envs);
        }

        public int environ_sizes_get(IntPtr memoryPtr, uint memoryLength, int sizePtr, int bufferSizePtr)
        {
            return WriteLength(memoryPtr, memoryLength, sizePtr, bufferSizePtr, envs);
        }
#pragma warning restore IDE1006 // naming style

        private int WriteStringList(IntPtr memoryPtr, uint memoryLength, int offset, int bufferOffset, IReadOnlyList<string> textList)
        {
            var bufOffset = (uint)bufferOffset;
            foreach (var text in textList)
            {
                if (!MemoryHelper.TryWrite(memoryPtr, memoryLength, offset, bufferOffset))
                {
                    return (int)Errno.Inval;
                }
                offset += 4;

                if (!MemoryHelper.TryWriteUtf8(memoryPtr, memoryLength, text, ref bufOffset))
                {
                    return (int)Errno.Inval;
                }
            }

            return (int)Errno.Success;
        }

        private int WriteLength(IntPtr memoryPtr, uint memoryLength, int sizePtr, int bufferSizePtr, IReadOnlyList<string> textList)
        {
            var dataSize = 0;
            foreach (var text in textList)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                dataSize += bytes.Length + 1;
            }

            if (!MemoryHelper.TryWrite(memoryPtr, memoryLength, sizePtr, textList.Count) ||
                 !MemoryHelper.TryWrite(memoryPtr, memoryLength, bufferSizePtr, dataSize))
            {
                return (int)Errno.Inval;
            }
            return (int)Errno.Success;
        }
    }
}
