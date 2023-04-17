using HoloLab.Spirare.Wasm.Core.Wasi;
using System;
using System.Collections.Generic;
using IwasmUnity;
using static HoloLab.Spirare.Wasm.Iwasm.BindHelper;

namespace HoloLab.Spirare.Wasm.Iwasm
{
    internal unsafe sealed class WasiBinder
    {
        private const string WasiModuleName = "wasi_snapshot_preview1";

        private readonly ArgsEnvironImplementation _envApi;
        private readonly FileDescriptorImplementation _fdApi = new FileDescriptorImplementation();
        private readonly SocketImplementation _socketApi = new SocketImplementation();
        private readonly WasiMiscImplementation _miscApi = new WasiMiscImplementation();

        public event Action<int> OnProcExitCalled;

        public WasiBinder(IReadOnlyList<string> args, IReadOnlyList<string> envs)
        {
            _envApi = new ArgsEnvironImplementation(args, envs);
        }

        public void ImportFunctions(Imports imports)
        {
            const string W = WasiModuleName;

            imports.ImportAction<int>(W, nameof(proc_exit), proc_exit);
            imports.ImportFunc(W, nameof(_envApi.args_get), Bind<int, int>(_envApi.args_get));
            imports.ImportFunc(W, nameof(_envApi.args_sizes_get), Bind<int, int>(_envApi.args_sizes_get));
            imports.ImportFunc(W, nameof(_envApi.environ_get), Bind<int, int>(_envApi.environ_get));
            imports.ImportFunc(W, nameof(_envApi.environ_sizes_get), Bind<int, int>(_envApi.environ_sizes_get));
            imports.ImportFunc(W, nameof(_fdApi.fd_write), Bind<int, int, int, int>(_fdApi.fd_write));
            imports.ImportFunc(W, nameof(_fdApi.fd_close), Bind<int>(_fdApi.fd_close));
            imports.ImportFunc(W, nameof(_fdApi.fd_prestat_get), Bind<int, int>(_fdApi.fd_prestat_get));
            imports.ImportFunc(W, nameof(_miscApi.clock_time_get), Bind<int, long, int>(_miscApi.clock_time_get));
            imports.ImportFunc(W, nameof(_miscApi.random_get), Bind<int, int>(_miscApi.random_get));
            imports.ImportFunc(W, nameof(_miscApi.clock_res_get), Bind<int, int>(_miscApi.clock_res_get));
            imports.ImportFunc(W, nameof(_socketApi.sock_getaddrinfo), Bind<int, int, int, int, int, int, int, int>(_socketApi.sock_getaddrinfo));
            imports.ImportFunc(W, nameof(_socketApi.sock_open), Bind<int, int, int>(_socketApi.sock_open));
            imports.ImportFunc(W, nameof(_socketApi.sock_send), Bind<int, int, int, int, int>(_socketApi.sock_send));
            imports.ImportFunc(W, nameof(_socketApi.sock_recv), Bind<int, int, int, int, int, int>(_socketApi.sock_recv));
            imports.ImportFunc(W, nameof(_socketApi.sock_connect), Bind<int, int, int>(_socketApi.sock_connect));
            imports.ImportFunc(W, nameof(_socketApi.sock_shutdown), Bind<int, int>(_socketApi.sock_shutdown));

            // not implemented yet...
            const int Inval = 28;
            imports.ImportFunc<int, long, long, int, int>(W, "fd_advise", (a0, a1, a2, a3, a4) =>
            {
                UnityEngine.Debug.LogError("'fd_advise' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int>(W, "fd_fdstat_get", (context, fd, resultPtr) =>
            {
                UnityEngine.Debug.LogError("'fd_fdstat_get' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int>(W, "fd_fdstat_set_flags", (a0, a1, a2) =>
            {
                UnityEngine.Debug.LogError("'fd_fdstat_set_flags' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int>(W, "fd_filestat_get", (a0, a1, a2) =>
            {
                UnityEngine.Debug.LogError("'fd_filestat_get' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, long, int>(W, "fd_filestat_set_size", (a0, a1, a2) =>
            {
                UnityEngine.Debug.LogError("'fd_filestat_set_size' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, long, long, int, int>(W, "fd_filestat_set_times", (a0, a1, a2, a3, a4) =>
            {
                UnityEngine.Debug.LogError("'fd_filestat_set_times' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, long, int, int>(W, "fd_pread", (a0, a1, a2, a3, a4, a5) =>
            {
                UnityEngine.Debug.LogError("'fd_pread' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int>(W, "fd_prestat_dir_name", (a0, a1, a2, a3) =>
            {
                UnityEngine.Debug.LogError("'fd_prestat_dir_name' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, long, int, int>(W, "fd_pwrite", (a0, a1, a2, a3, a4, a5) =>
            {
                UnityEngine.Debug.LogError("'fd_pwrite' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int, int>(W, "fd_read", (a0, a1, a2, a3, a4) =>
            {
                UnityEngine.Debug.LogError("'fd_read' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, long, int, int>(W, "fd_readdir", (a0, a1, a2, a3, a4, a5) =>
            {
                UnityEngine.Debug.LogError("'fd_readdir' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, long, int, int, int>(W, "fd_seek", (a0, a1, a2, a3, a4) =>
            {
                UnityEngine.Debug.LogError("'fd_seek' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int>(W, "fd_sync", (a0, a1) =>
            {
                UnityEngine.Debug.LogError("'fd_sync' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int>(W, "fd_tell", (a0, a1, a2) =>
            {
                UnityEngine.Debug.LogError("'fd_tell' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int>(W, "path_create_directory", (a0, a1, a2, a3) =>
            {
                UnityEngine.Debug.LogError("'path_create_directory' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int, int, int>(W, "path_filestat_get", (a0, a1, a2, a3, a4, a5) =>
            {
                UnityEngine.Debug.LogError("'path_filestat_get' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int, long, long, int, int>(W, "path_filestat_set_times", (a0, a1, a2, a3, a4, a5, a6, a7) =>
            {
                UnityEngine.Debug.LogError("'path_filestat_set_times' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int, int, int, int, int>(W, "path_link", (a0, a1, a2, a3, a4, a5, a6, a7) =>
            {
                UnityEngine.Debug.LogError("'path_link' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int, int, long, long, int, int, int>(W, "path_open", (a0, a1, a2, a3, a4, a5, a6, a7, a8, a9) =>
            {
                UnityEngine.Debug.LogError("'path_open' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int, int, int, int>(W, "path_readlink", (a0, a1, a2, a3, a4, a5, a6) =>
            {
                UnityEngine.Debug.LogError("'path_readlink' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int>(W, "path_remove_directory", (a0, a1, a2, a3) =>
            {
                UnityEngine.Debug.LogError("'path_remove_directory' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int, int, int, int>(W, "path_rename", (a0, a1, a2, a3, a4, a5, a6) =>
            {
                UnityEngine.Debug.LogError("'path_rename' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int, int, int>(W, "path_symlink", (a0, a1, a2, a3, a4, a5) =>
            {
                UnityEngine.Debug.LogError("'path_symlink' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int>(W, "path_unlink_file", (a0, a1, a2, a3) =>
            {
                UnityEngine.Debug.LogError("'path_unlink_file' is not implemented.");
                return Inval;
            });
            imports.ImportFunc<int, int, int, int, int>(W, "poll_oneoff", (a0, a1, a2, a3, a4) =>
            {
                UnityEngine.Debug.LogError("'poll_oneoff' is not implemented.");
                return Inval;
            });
        }

#pragma warning disable IDE1006 // naming style
        private void proc_exit(ImportedContext context, int exitCode)
        {
            OnProcExitCalled?.Invoke(exitCode);
        }
#pragma warning restore IDE1006 // naming style
    }
}
