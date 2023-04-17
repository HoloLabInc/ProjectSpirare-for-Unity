using System;
using System.Text;
using UnityEngine;

namespace HoloLab.Spirare.Wasm.Core.Wasi
{
    public sealed class FileDescriptorImplementation
    {
        // private readonly Dictionary<int, Socket> sockets = new Dictionary<int, Socket>();

#pragma warning disable IDE1006 // naming style
        public int fd_prestat_get(IntPtr memoryPtr, uint memoryLength, int fd, int resultPtr)
        {
            // TODO: not implemented yet
            var isValidFd = false;

            if (isValidFd == false)
            {
                return (int)Errno.Badf;
            }
            uint prNameLen = 0;
            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, resultPtr, PreStat.Dir(prNameLen)) == false)
            {
                return (int)Errno.Inval;
            }
            return (int)Errno.Success;
        }

        //public int fd_fdstat_get(IntPtr memoryPtr, uint memoryLength, int fd, int resultPtr)
        //{
        //    FdStat fdStat;
        //    switch (fd)
        //    {
        //        case 0: // stdin
        //            {
        //                var type = FileType.CharacterDevice;
        //                var (rightBase, rightInheriting) = GetRights(type, true, false, true);
        //                fdStat = new FdStat
        //                {
        //                    fs_filetype = type,
        //                    fs_flags = 0,   // TODO: I don't know what it should be.
        //                    fs_rights_base = rightBase,
        //                    fs_rights_inheriting = rightInheriting,
        //                };
        //                break;
        //            }
        //        case 1: // stdout
        //        case 2: // stderr
        //            {
        //                var type = FileType.CharacterDevice;
        //                var (rightBase, rightInheriting) = GetRights(type, false, true, true);
        //                fdStat = new FdStat
        //                {
        //                    fs_filetype = type,
        //                    fs_flags = 0,   // TODO: I don't know what it should be.
        //                    fs_rights_base = rightBase,
        //                    fs_rights_inheriting = rightInheriting,
        //                };
        //                break;
        //            }
        //        default:
        //            Debug.LogError("'fd_fdstat_get' is not implemented");
        //            return (int)Errno.Inval;
        //    }

        //    if (MemoryHelper.TryWrite(memoryPtr, memoryLength, resultPtr, fdStat) == false)
        //    {
        //        return (int)Errno.Inval;
        //    }
        //    return (int)Errno.Success;
        //}

        private static (Rights RightBase, Rights RightInheriting) GetRights(FileType type, bool canRead, bool canWrite, bool isTty)
        {
            Rights rightbase;
            Rights rightInheriting;

            switch (type)
            {
                case FileType.Unknown:
                    rightbase = 0;
                    rightInheriting = 0;
                    break;
                case FileType.BlockDevice:
                    rightbase = Rights.BlockDeviceBase;
                    rightInheriting = Rights.BlockDeviceInheriting;
                    break;
                case FileType.CharacterDevice:
                    if (isTty)
                    {
                        rightbase = Rights.TtyBase;
                        rightInheriting = Rights.TtyInheriting;
                    }
                    else
                    {
                        rightbase = Rights.CharacterDeviceBase;
                        rightInheriting = Rights.CharacterDeviceInheriting;
                    }
                    break;
                case FileType.Directory:
                    rightbase = Rights.DirectoryBase;
                    rightInheriting = Rights.DirectoryInheriting;
                    break;
                case FileType.RegularFile:
                    rightbase = Rights.RegularFileBase;
                    rightInheriting = Rights.RegularFileInheriting;
                    break;
                case FileType.SocketDgram:
                case FileType.SocketStream:
                    rightbase = Rights.SocketBase;
                    rightInheriting = Rights.SocketInheriting;
                    break;
                case FileType.SymbolicLink:
                    rightbase = 0;
                    rightInheriting = 0;
                    break;
                default:
                    rightbase = 0;
                    rightInheriting = 0;
                    break;
            }
            if (canRead == false)
            {
                rightbase &= ~Rights.fd_read;
            }
            if (canWrite == false)
            {
                rightbase &= ~Rights.fd_write;
            }
            return (rightbase, rightInheriting);
        }

        public int fd_write(IntPtr memoryPtr, uint memoryLength, int fd, int ciovecPtr, int ciovecLen, int nwrittenPtr)
        {
            if (MemoryHelper.TryReadVectoredBufferToBytes(memoryPtr, memoryLength, ciovecPtr, ciovecLen, out var buffer) == false)
            {
                return (int)Errno.Inval;
            }

            string text;
            try
            {
                text = Encoding.UTF8.GetString(buffer);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return (int)Errno.Inval;
            }

            if (fd == 1)
            {
                Debug.Log(text);
            }
            else if (fd == 2)
            {
                Debug.LogError(text);
            }

            if (!MemoryHelper.TryWrite(memoryPtr, memoryLength, nwrittenPtr, buffer.Length))
            {
                return (int)Errno.Inval;
            }
            return (int)Errno.Success;
        }

        public int fd_close(IntPtr memoryPtr, uint memoryLength, int fd)
        {
            // TODO: not implemented yet
            return (int)Errno.Success;
        }
#pragma warning restore IDE1006 // naming style

        /*
        public int Close(ArgumentParser parser, MemoryReader memoryReader)
        {
            if (!parser.TryReadInt(out var fd))
            {
                return Invalid;
            }

            if (!sockets.TryGetValue(fd, out var socket))
            {
                return Invalid;
            }
            try
            {
                socket.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            sockets.Remove(fd);
            return Success;
        }
           }
    */
    }
}
