using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace HoloLab.Spirare.Wasm.Core.Wasi
{
    public sealed class SocketImplementation
    {
        private readonly Dictionary<int, Socket> sockets = new Dictionary<int, Socket>();

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
        */

#pragma warning disable IDE1006 // naming style
        public int sock_open(IntPtr memoryPtr, uint memoryLength, int addressFamily, int sockType, int fdPtr)
        {
            // TODO Implement correctly
            var netAddressFamily = System.Net.Sockets.AddressFamily.InterNetwork;
            var netSocketType = System.Net.Sockets.SocketType.Stream;
            var protocolType = System.Net.Sockets.ProtocolType.Tcp;
            var socket = new Socket(netAddressFamily, netSocketType, protocolType);
            var fd = socket.Handle.ToInt32();
            sockets.Add(fd, socket);
            MemoryHelper.TryWrite(memoryPtr, memoryLength, fdPtr, fd);
            fd += 1;
            return 0;
        }

        public int sock_connect(IntPtr memoryPtr, uint memoryLength, int fd, int addressPtr, int port)
        {
            MemoryHelper.TryRead(memoryPtr, memoryLength, addressPtr, out Address address);
            MemoryHelper.TryReadBytes(memoryPtr, memoryLength, address.BufPtr, address.BufLen, out var addressBytes);

            var ipAddress = new IPAddress(addressBytes);

            if (sockets.TryGetValue(fd, out var socket) == false)
            {
                return (int)Errno.Badf;
            }

            try
            {
                var ipe = new IPEndPoint(ipAddress, port);
                socket.Connect(ipe);
                return (int)Errno.Success;
            }
            catch (Exception e)
            {
                // TODO: Return appropriate error
                Debug.LogException(e);
                return (int)Errno.Io;
            }
        }

        public int sock_send(IntPtr memoryPtr, uint memoryLength, int fd, int siDataPtr, int siDataLen, int siFlags, int soDataLenPtr)
        {
            if (sockets.TryGetValue(fd, out var socket) == false)
            {
                return (int)Errno.Badf;
            }

            if (MemoryHelper.TryReadVectoredBuffer(memoryPtr, memoryLength, siDataPtr, siDataLen, out var siData) == false)
            {
                return (int)Errno.Inval;
            }

            try
            {
                var sentSize = socket.Send(siData);
                MemoryHelper.TryWrite(memoryPtr, memoryLength, soDataLenPtr, sentSize);
            }
            catch (Exception)
            {
                // TODO: Return appropriate error
                return (int)Errno.Io;
            }

            return (int)Errno.Success;
        }

        public int sock_recv(IntPtr memoryPtr, uint memoryLength, int fd, int riDataPtr, int riDataLen, int riFlags,
                   int roDataLenPtr, int roFlagsPtr)
        {
            if (sockets.TryGetValue(fd, out var socket) == false)
            {
                return (int)Errno.Badf;
            }

            var receivedLengthSum = 0;
            try
            {
                for (var i = 0; i < riDataLen; i++)
                {
                    if (!MemoryHelper.TryRead(memoryPtr, memoryLength, riDataPtr + i * 8, out int start))
                    {
                        return (int)Errno.Inval;
                    }

                    if (!MemoryHelper.TryRead(memoryPtr, memoryLength, riDataPtr + i * 8 + 4, out int length))
                    {
                        return (int)Errno.Inval;
                    }

                    var buffer = new byte[length];
                    var receivedLength = socket.Receive(buffer, SocketFlags.None);

                    if (!MemoryHelper.TryWriteArray(memoryPtr, memoryLength, start, buffer))
                    {
                        return (int)Errno.Inval;
                    }

                    receivedLengthSum += receivedLength;
                }
                return (int)Errno.Success;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return (int)Errno.Io;
            }
            finally
            {
                MemoryHelper.TryWrite(memoryPtr, memoryLength, roDataLenPtr, receivedLengthSum);
            }
        }

        public int sock_shutdown(IntPtr memoryPtr, uint memoryLength, int fd, int sdFlags)
        {
            if (sockets.TryGetValue(fd, out var socket) == false)
            {
                return (int)Errno.Badf;
            }

            var sdFlagsType = (SdFlagsType)Enum.ToObject(typeof(SdFlagsType), sdFlags);

            SocketShutdown socketShutdown;
            if (sdFlagsType.HasFlag(SdFlagsType.Rd | SdFlagsType.Wr))
            {
                socketShutdown = SocketShutdown.Both;
            }
            else if (sdFlagsType.HasFlag(SdFlagsType.Rd))
            {
                socketShutdown = SocketShutdown.Receive;
            }
            else if (sdFlagsType.HasFlag(SdFlagsType.Wr))
            {
                socketShutdown = SocketShutdown.Send;
            }
            else
            {
                return (int)Errno.Inval;
            }

            try
            {
                socket.Shutdown(socketShutdown);
                return (int)Errno.Success;
            }
            catch (SocketException)
            {
                return (int)Errno.Io;
            }
            catch (ObjectDisposedException)
            {
                return (int)Errno.Notconn;
            }
            catch (Exception)
            {
                return (int)Errno.Notsock;
            }
        }

        public int sock_getaddrinfo(IntPtr memoryPtr, uint memoryLength, int hostPtr, int hostLength, int servicePtr, int serviceLength,
            int hintsPtr, int resPtr, int maxResLength, int resLengthPtr)
        {
            if (MemoryHelper.TryReadUtf8(memoryPtr, memoryLength, hostPtr, hostLength, out var host) == false)
            {
                return (int)Errno.Inval;
            }

            if (MemoryHelper.TryReadUtf8(memoryPtr, memoryLength, servicePtr, serviceLength, out var service) == false)
            {
                return (int)Errno.Inval;
            }

            if (MemoryHelper.TryRead(memoryPtr, memoryLength, hintsPtr, out AddrInfo hints) == false)
            {
                return (int)Errno.Inval;
            }

            if (MemoryHelper.TryRead(memoryPtr, memoryLength, resPtr, out int res0Ptr) == false)
            {
                return (int)Errno.Inval;
            }

            if (MemoryHelper.TryRead(memoryPtr, memoryLength, res0Ptr, out AddrInfo res) == false)
            {
                return (int)Errno.Inval;
            }

            if (MemoryHelper.TryRead(memoryPtr, memoryLength, res.AiAddr, out Sockaddr sockaddr) == false)
            {
                return (int)Errno.Inval;
            }

            res.AiAddrlen = 12;

            if (MemoryHelper.TryWrite<AddrInfo>(memoryPtr, memoryLength, res0Ptr, res) == false)
            {
                return (int)Errno.Inval;
            }

            var saDataLen = sockaddr.SaDataLen;
            var saData = new byte[saDataLen];

            var entry = Dns.GetHostEntry(host);
            if (entry.AddressList.Length >= 1)
            {
                var firstEntry = entry.AddressList[0];
                var addressBytes = firstEntry.GetAddressBytes();
                Buffer.BlockCopy(addressBytes, 0, saData, 2, addressBytes.Length);
            }

            short port = GetServiceDefaultPort(service);

            var portBytes = BitConverter.GetBytes(port).Reverse().ToArray();
            Buffer.BlockCopy(portBytes, 0, saData, 0, portBytes.Length);

            if (MemoryHelper.TryWriteArray(memoryPtr, memoryLength, sockaddr.SaDataPtr, saData) == false)
            {
                return (int)Errno.Inval;
            }

            if (MemoryHelper.TryWrite(memoryPtr, memoryLength, resLengthPtr, 1) == false)
            {
                return (int)Errno.Inval;
            }

            return (int)Errno.Success;
        }

#pragma warning restore IDE1006 // naming style

        private static short GetServiceDefaultPort(string service)
        {
            switch (service.ToLower())
            {
                case "http":
                    return 80;
                case "https":
                    return 443;
                default:
                    return 0;
            }
        }
    }
}
