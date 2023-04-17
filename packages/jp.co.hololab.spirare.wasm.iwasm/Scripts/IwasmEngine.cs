using HoloLab.Spirare.Wasm.Core.Spirare;
using IwasmUnity;
using System;

namespace HoloLab.Spirare.Wasm.Iwasm
{
    internal sealed class IwasmEngine : IDisposable
    {
        private static readonly object _lock = new object();
        private static bool _init;
        private static Engine _engine;
        private static Store _store;

        private Module _module;
        private Instance _instance;
        private readonly ElementDescriptorHelper _elementDescriptorHelper;
        private SpirareBinder _spirareBinder;
        private WasiBinder _wasiBinder;
        private const int StackSize = 4 * 1024 * 1024;

        public event Action OnTerminated;


        public IwasmEngine(ElementDescriptorHelper elementDescriptorHelper)
        {
            lock (_lock)
            {
                if (_init == false)
                {
                    _engine = new Engine();
                    _store = new Store(_engine);
                    _init = true;
                }
            }
            _elementDescriptorHelper = elementDescriptorHelper;
        }

        public void Load(byte[] wasm, string[] args, string[] envs)
        {
            Module module = null;
            Instance instance = null;
            try
            {
                module = Module.CreateFromWasm(_store, wasm);
                var imports = module.CreateImports();
                var wasiBinder = new WasiBinder(args, envs);
                var spirareBinder = new SpirareBinder(_elementDescriptorHelper);
                spirareBinder.ImportFunctions(imports);
                wasiBinder.ImportFunctions(imports);
                wasiBinder.OnProcExitCalled += exitCode =>
                {
                    Terminate();
                };


                instance = module.CreateInstance(imports, StackSize);
                spirareBinder.SetInstance(instance);
                spirareBinder.GetExportedFunctions();

                _module = module;
                _instance = instance;
                _spirareBinder = spirareBinder;
                _wasiBinder = wasiBinder;
            }
            catch (Exception)
            {
                module?.Dispose();
                instance?.Dispose();
                throw;
            }
        }

        public void InvokeEntryPoint()
        {
            _spirareBinder?.InvokeEntryPoint();

            if (WasmScriptRunMode.IsLoopRunning == false)
            {
                _spirareBinder?.InvokeOnStart();
            }
        }

        public void RequestUpdate()
        {
            _spirareBinder?.RequestUpdate();
        }

        public void RequestExit()
        {
            _spirareBinder?.RequestExit();
        }

        public void RequestSelect()
        {
            _spirareBinder?.RequestSelect();
        }

        public void Dispose()
        {
            _module?.Dispose();
            _instance?.Dispose();
            _module = null;
            _instance = null;
        }

        private void Terminate()
        {
            RequestExit();
            Dispose();
            OnTerminated?.Invoke();
        }
    }
}
