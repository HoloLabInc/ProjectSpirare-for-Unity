using System;
using System.Runtime.InteropServices;

namespace HoloLab.Spirare.Wasm.Core.Wasi
{
    internal enum Errno
    {
        /// <summary>
        /// No error occurred. System call completed successfully. <summary>
        /// </summary>
        Success,
        /// <summary>
        /// Argument list too long.
        /// </summary>
        Toobig,
        /// <summary>
        /// Permission denied.
        /// </summary>
        Access,
        /// <summary>
        /// Address in use.
        /// </summary>
        Addrinuse,
        /// <summary>
        /// Address not available.
        /// </summary>
        Addrnotavail,
        /// <summary>
        /// Address family not supported.
        /// </summary>
        Afnosupport,
        /// <summary>
        /// Resource unavailable, or operation would block.
        /// </summary>
        Again,
        /// <summary>
        /// Connection already in progress.
        /// </summary>
        Already,
        /// <summary>
        /// Bad file descriptor.
        /// </summary>
        Badf,
        /// <summary>
        /// Bad message.
        /// </summary>
        Badmsg,
        /// <summary>
        /// Device or resource busy.
        /// </summary>
        Busy,
        /// <summary>
        /// Operation canceled.
        /// </summary>
        Canceled,
        /// <summary>
        /// No child processes.
        /// </summary>
        Child,
        /// <summary>
        /// Connection aborted.
        /// </summary>
        Connaborted,
        /// <summary>
        /// Connection refused.
        /// </summary>
        Connrefused,
        /// <summary>
        /// Connection reset.
        /// </summary>
        Connreset,
        /// <summary>
        /// Resource deadlock would occur.
        /// </summary>
        Deadlk,
        /// <summary>
        /// Destination address required.
        /// </summary>
        Destaddrreq,
        /// <summary>
        /// Mathematics argument out of domain of function.
        /// </summary>
        Dom,
        /// <summary>
        /// Reserved.
        /// </summary>
        Dquot,
        /// <summary>
        /// File exists.
        /// </summary>
        Exist,
        /// <summary>
        /// Bad address.
        /// </summary>
        Fault,
        /// <summary>
        /// File too large.
        /// </summary>
        Fbig,
        /// <summary>
        /// Host is unreachable.
        /// </summary>
        Hostunreach,
        /// <summary>
        /// Identifier removed.
        /// </summary>
        Idrm,
        /// <summary>
        /// Illegal byte sequence.
        /// </summary>
        Ilseq,
        /// <summary>
        /// Operation in progress.
        /// </summary>
        Inprogress,
        /// <summary>
        /// Interrupted function.
        /// </summary>
        Intr,
        /// <summary>
        /// Invalid argument.
        /// </summary>
        Inval,
        /// <summary>
        /// I/O error.
        /// </summary>
        Io,
        /// <summary>
        /// Socket is connected.
        /// </summary>
        Isconn,
        /// <summary>
        /// Is a directory.
        /// </summary>
        Isdir,
        /// <summary>
        /// Too many levels of symbolic links.
        /// </summary>
        Loop,
        /// <summary>
        /// File descriptor value too large.
        /// </summary>
        Mfile,
        /// <summary>
        /// Too many links.
        /// </summary>
        Mlink,
        /// <summary>
        /// Message too large.
        /// </summary>
        Msgsize,
        /// <summary>
        /// Reserved.
        /// </summary>
        Multihop,
        /// <summary>
        /// Filename too long.
        /// </summary>
        Nametoolong,
        /// <summary>
        /// Network is down.
        /// </summary>
        Netdown,
        /// <summary>
        /// Connection aborted by network.
        /// </summary>
        Netreset,
        /// <summary>
        /// Network unreachable.
        /// </summary>
        Netunreach,
        /// <summary>
        /// Too many files open in system.
        /// </summary>
        Nfile,
        /// <summary>
        /// No buffer space available.
        /// </summary>
        Nobufs,
        /// <summary>
        /// No such device.
        /// </summary>
        Nodev,
        /// <summary>
        /// No such file or directory.
        /// </summary>
        Noent,
        /// <summary>
        /// Executable file format error.
        /// </summary>
        Noexec,
        /// <summary>
        /// No locks available.
        /// </summary>
        Nolck,
        /// <summary>
        /// Reserved.
        /// </summary>
        Nolink,
        /// <summary>
        /// Not enough space.
        /// </summary>
        Nomem,
        /// <summary>
        /// No message of the desired type.
        /// </summary>
        Nomsg,
        /// <summary>
        /// Protocol not available.
        /// </summary>
        Noprotoopt,
        /// <summary>
        /// No space left on device.
        /// </summary>
        Nospc,
        /// <summary>
        /// Function not supported.
        /// </summary>
        Nosys,
        /// <summary>
        /// The socket is not connected.
        /// </summary>
        Notconn,
        /// <summary>
        /// Not a directory or a symbolic link to a directory.
        /// </summary>
        Notdir,
        /// <summary>
        /// Directory not empty.
        /// </summary>
        Notempty,
        /// <summary>
        /// State not recoverable.
        /// </summary>
        Notrecoverable,
        /// <summary>
        /// Not a socket.
        /// </summary>
        Notsock,
        /// <summary>
        /// Not supported, or operation not supported on socket.
        /// </summary>
        Notsup,
        /// <summary>
        /// Inappropriate I/O control operation.
        /// </summary>
        Notty,
        /// <summary>
        /// No such device or address.
        /// </summary>
        Nxio,
        /// <summary>
        /// Value too large to be stored in data type.
        /// </summary>
        Overflow,
        /// <summary>
        /// Previous owner died.
        /// </summary>
        Ownerdead,
        /// <summary>
        /// Operation not permitted.
        /// </summary>
        Perm,
        /// <summary>
        /// Broken pipe.
        /// </summary>
        Pipe,
        /// <summary>
        /// Protocol error.
        /// </summary>
        Proto,
        /// <summary>
        /// Protocol not supported.
        /// </summary>
        Protonosupport,
        /// <summary>
        /// Protocol wrong type for socket.
        /// </summary>
        Prototype,
        /// <summary>
        /// Result too large.
        /// </summary>
        Range,
        /// <summary>
        /// Read-only file system.
        /// </summary>
        Rofs,
        /// <summary>
        /// Invalid seek.
        /// </summary>
        Spipe,
        /// <summary>
        /// No such process.
        /// </summary>
        Srch,
        /// <summary>
        /// Reserved.
        /// </summary>
        Stale,
        /// <summary>
        /// Connection timed out.
        /// </summary>
        Timedout,
        /// <summary>
        /// Text file busy.
        /// </summary>
        Txtbsy,
        /// <summary>
        /// Cross-device link.
        /// </summary>
        Xdev,
        /// <summary>
        /// Extension: Capabilities insufficient.
        /// </summary>
        Notcapable,
    }

    internal enum ClockId
    {
        Realtime = 0,
        Monotonic = 1,
        ProcessCputimeId = 2,
        ThreadCputimeId = 3
    }

    internal enum SocketType
    {
        SockAny = 0,
        SockDgram = 1,
        SockStream = 2,
    };

    internal enum AddressFamily : byte
    {
        Unspec = 0,
        Inet4 = 1,
        Inet6 = 2,
    };

    [Flags]
    internal enum SdFlagsType
    {
        Rd = 1 << 0,
        Wr = 1 << 1,
    };

    [StructLayout(LayoutKind.Explicit)]
    internal struct AddrInfo
    {
        [FieldOffset(0)]
        public ushort AiFlags;

        [FieldOffset(2)]
        public byte AiFamily;

        [FieldOffset(3)]
        public byte AiSocktype;

        [FieldOffset(4)]
        public byte AiProtocol;

        [FieldOffset(8)]
        public uint AiAddrlen;

        [FieldOffset(12)]
        public int AiAddr;

        [FieldOffset(16)]
        public int AdCanonname;

        [FieldOffset(20)]
        public uint AdCanonnameLen;

        [FieldOffset(24)]
        public int AiNext;
    };

    [StructLayout(LayoutKind.Explicit)]
    internal struct Sockaddr
    {
        [FieldOffset(0)]
        public AddressFamily SaFamily;

        [FieldOffset(4)]
        public int SaDataLen;

        [FieldOffset(8)]
        public int SaDataPtr;
    };

    [StructLayout(LayoutKind.Explicit)]
    internal struct Address
    {
        [FieldOffset(0)]
        public int BufPtr;

        [FieldOffset(4)]
        public int BufLen;
    };

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal struct PreStat
    {
        [FieldOffset(0)]
        byte tag;
        [FieldOffset(4)]
        uint u;

        private const byte TAG_DIR = 0;

        public static PreStat Dir(uint pr_name_len)
        {
            return new PreStat
            {
                tag = TAG_DIR,
                u = pr_name_len,
            };
        }
    }

    internal enum FileAdvice : uint
    {
        Normal = 0,
        Sequential = 1,
        Random = 2,
        WillNeed = 3,
        DontNeed = 4,
        NoReuse = 5,
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct FileStat
    {
        [FieldOffset(0)]
        public ulong device_id;
        [FieldOffset(8)]
        public ulong inode;
        [FieldOffset(16)]
        public FileType filetype;
        [FieldOffset(24)]
        public ulong nlink;
        [FieldOffset(32)]
        public ulong size; // this is a read field, the rest are file fields
        [FieldOffset(40)]
        public Timestamp atim;
        [FieldOffset(48)]
        public Timestamp mtim;
        [FieldOffset(56)]
        public Timestamp ctim;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal struct Timestamp
    {
        [FieldOffset(0)]
        public ulong tick;

        public static explicit operator Timestamp(ulong value) => new Timestamp { tick = value };
        public static explicit operator Timestamp(long value) => new Timestamp { tick = (ulong)value };
        public static explicit operator ulong(Timestamp value) => value.tick;
        public static explicit operator long(Timestamp value) => (long)value.tick;
    }

    internal enum FileType : byte
    {
        Unknown = 0,
        BlockDevice = 1,
        CharacterDevice = 2,
        Directory = 3,
        RegularFile = 4,
        SocketDgram = 5,
        SocketStream = 6,
        SymbolicLink = 7,
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    internal struct FdStat
    {
        [FieldOffset(0)]
        public FileType fs_filetype;
        [FieldOffset(2)]
        public FdFlags fs_flags;
        [FieldOffset(8)]
        public Rights fs_rights_base;
        [FieldOffset(16)]
        public Rights fs_rights_inheriting;
    }

    [Flags]
    internal enum Rights : ulong
    {
        fd_datasync = 1 << 0,
        fd_read = 1 << 1,
        fd_seek = 1 << 2,
        fd_fdstat_set_flags = 1 << 3,
        fd_sync = 1 << 4,
        fd_tell = 1 << 5,
        fd_write = 1 << 6,
        fd_advise = 1 << 7,
        fd_allocate = 1 << 8,
        path_create_directory = 1 << 9,
        path_create_file = 1 << 10,
        path_link_source = 1 << 11,
        path_link_target = 1 << 12,
        path_open = 1 << 13,
        fd_readdir = 1 << 14,
        path_readlink = 1 << 15,
        path_rename_source = 1 << 16,
        path_rename_target = 1 << 17,
        path_filestat_get = 1 << 18,
        path_filestat_set_size = 1 << 19,
        path_filestat_set_times = 1 << 20,
        fd_filestat_get = 1 << 21,
        fd_filestat_set_size = 1 << 22,
        fd_filestat_set_times = 1 << 23,
        path_symlink = 1 << 24,
        path_remove_directory = 1 << 25,
        path_unlink_file = 1 << 26,
        poll_fd_readwrite = 1 << 27,
        sock_shutdown = 1 << 28,
        sock_accept = 1 << 29,

        // -------
        All =
            fd_datasync |
            fd_read |
            fd_seek |
            fd_fdstat_set_flags |
            fd_sync |
            fd_tell |
            fd_write |
            fd_advise |
            fd_allocate |
            path_create_directory |
            path_create_file |
            path_link_source |
            path_link_target |
            path_open |
            fd_readdir |
            path_readlink |
            path_rename_source |
            path_rename_target |
            path_filestat_get |
            path_filestat_set_size |
            path_filestat_set_times |
            fd_filestat_get |
            fd_filestat_set_size |
            fd_filestat_set_times |
            path_symlink |
            path_remove_directory |
            path_unlink_file |
            poll_fd_readwrite |
            sock_shutdown |
            sock_accept,

        BlockDeviceBase = All,
        BlockDeviceInheriting = All,
        CharacterDeviceBase = All,
        CharacterDeviceInheriting = All,

        RegularFileBase =
            fd_datasync |
            fd_read |
            fd_seek |
            fd_fdstat_set_flags |
            fd_sync |
            fd_tell |
            fd_write |
            fd_advise |
            fd_allocate |
            fd_filestat_get |
            fd_filestat_set_size |
            fd_filestat_set_times |
            poll_fd_readwrite,

        RegularFileInheriting = 0,

        DirectoryBase = fd_fdstat_set_flags |
                        fd_sync |
                        fd_advise |
                        path_create_directory |
                        path_create_file |
                        path_link_source |
                        path_link_target |
                        path_open |
                        fd_readdir |
                        path_readlink |
                        path_rename_source |
                        path_rename_target |
                        path_filestat_get |
                        path_filestat_set_size |
                        path_filestat_set_times |
                        fd_filestat_get |
                        fd_filestat_set_times |
                        path_symlink |
                        path_unlink_file |
                        path_remove_directory |
                        poll_fd_readwrite,
        DirectoryInheriting = DirectoryBase | RegularFileBase,

        SocketBase =
            fd_read |
            fd_fdstat_set_flags |
            fd_write |
            fd_filestat_get |
            poll_fd_readwrite |
            sock_shutdown,

        SocketInheriting = All,

        TtyBase = fd_read |
                  fd_fdstat_set_flags |
                  fd_write |
                  fd_filestat_get |
                  poll_fd_readwrite,
        TtyInheriting = 0,
    }

    [Flags]
    internal enum FstFlags : ushort
    {
        atim = 1 << 0,
        atim_now = 1 << 1,
        mtim = 1 << 2,
        mtim_now = 1 << 3,
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal struct Dircookie
    {
        [FieldOffset(0)]
        public ulong value;

        public static explicit operator ulong(Dircookie a) => a.value;
        public static explicit operator Dircookie(ulong value) => new Dircookie { value = value };
        public static explicit operator long(Dircookie a) => (long)a.value;
        public static explicit operator Dircookie(long value) => new Dircookie { value = (ulong)value };
    }

    internal enum Whence : byte
    {
        set = 0,
        cur = 1,
        end = 2,
    }

    [Flags]
    internal enum LookupFlags : uint
    {
        symlink_follow = 1 << 0,
    }

    [Flags]
    internal enum OFlags : ushort
    {
        creat = 1 << 0,
        directory = 1 << 1,
        excl = 1 << 2,
        trunc = 1 << 3,
    }

    [Flags]
    internal enum FdFlags : ushort
    {
        append = 1 << 0,
        dsync = 1 << 1,
        nonblock = 1 << 2,
        rsync = 1 << 3,
        sync = 1 << 4,
    }

    [StructLayout(LayoutKind.Explicit, Size = 48)]
    internal struct Subscription
    {
        [FieldOffset(0)]
        public ulong userdata;
        [FieldOffset(8)]
        public Subscription_u u;
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    internal struct Subscription_u
    {
        // not implemented yet
    }

    [StructLayout(LayoutKind.Explicit, Size = 32)]
    internal struct Event
    {
        // not implemented yet
    }
}
