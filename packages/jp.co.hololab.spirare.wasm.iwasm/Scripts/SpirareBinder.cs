using HoloLab.Spirare.Wasm.Core.Spirare;
using IwasmUnity;
using System;
using static HoloLab.Spirare.Wasm.Iwasm.BindHelper;

namespace HoloLab.Spirare.Wasm.Iwasm
{
    internal unsafe sealed class SpirareBinder
    {
        private Instance _instance;
        private readonly ElementDescriptorHelper _elementDescriptorHelper;
        private readonly SpirareApiImpl _api;

        private Action _wasiEntryPoint;
        private Action _onStartAction;
        private Action _onUpdateAction;
        private Action _onSelectAction;
        private Action<int, int, int, int> _onEventCallbackAction;

        // when IsLoopRunning == true
        private bool _updateRequested;
        private bool _exitRequested;
        private bool _selectRequested;

        private const string SpirareModuleName = "spirare_preview1";

        public SpirareBinder(ElementDescriptorHelper elementDescriptorHelper)
        {
            _elementDescriptorHelper = elementDescriptorHelper;
            _api = new SpirareApiImpl(elementDescriptorHelper);
        }

        public void InvokeEntryPoint()
        {
            _wasiEntryPoint?.Invoke();
        }

        public void InvokeOnStart()
        {
            _onStartAction?.Invoke();
        }
        public void RequestUpdate()
        {
            if (WasmScriptRunMode.IsLoopRunning)
            {
                _updateRequested = true;
            }
            else
            {
                _onUpdateAction?.Invoke();
            }
        }

        public void RequestExit()
        {
            if (WasmScriptRunMode.IsLoopRunning)
            {
                _exitRequested = true;
            }
        }

        public void RequestSelect()
        {
            if (WasmScriptRunMode.IsLoopRunning)
            {
                _selectRequested = true;
            }
            else
            {
                _onSelectAction?.Invoke();
            }
        }

        public void SetInstance(Instance instance)
        {
            _instance = instance;
        }

        public void GetExportedFunctions()
        {
            var exports = _instance.Exports;
            if (exports.TryGetFunction("_start", out var wasiStartFunction))
            {
                _wasiEntryPoint = IgnoreException(wasiStartFunction, x => x.ToAction());
            }
            if (exports.TryGetFunction("on_start", out var onStartFunction))
            {
                _onStartAction = IgnoreException(onStartFunction, x => x.ToAction());
            }
            if (exports.TryGetFunction("on_update", out var onUpdateFunction))
            {
                _onUpdateAction = IgnoreException(onUpdateFunction, x => x.ToAction());
            }
            if (exports.TryGetFunction("on_select", out var onSelectFunction))
            {
                _onSelectAction = IgnoreException(onSelectFunction, x => x.ToAction());
            }
            if (exports.TryGetFunction("on_event_callback", out var onEventCallbackFunction))
            {
                _onEventCallbackAction = IgnoreException(onEventCallbackFunction, x => x.ToAction<int, int, int, int>());
            }
        }

        public void ImportFunctions(Imports imports)
        {
            const string S = SpirareModuleName;

            if (WasmScriptRunMode.IsLoopRunning)
            {
                imports.ImportAction(S, nameof(run_loop), run_loop);
            }
            imports.ImportFunc(S, nameof(_api.get_element_by_id), Bind<int, int, int>(_api.get_element_by_id));
            imports.ImportFunc(S, nameof(_api.get_all_elements), Bind<int, int, int>(_api.get_all_elements));
            imports.ImportFunc(S, nameof(_api.get_all_elements_len), Bind<int>(_api.get_all_elements_len));
            imports.ImportFunc(S, nameof(_api.register_event), BindRegisterEvent());
            imports.ImportFunc(S, nameof(_api.get_position), Bind<int, int>(_api.get_position));
            imports.ImportFunc(S, nameof(_api.set_position), Bind<int, int>(_api.set_position));
            imports.ImportFunc(S, nameof(_api.get_rotation), Bind<int, int>(_api.get_rotation));
            imports.ImportFunc(S, nameof(_api.set_rotation), Bind<int, int>(_api.set_rotation));
            imports.ImportFunc(S, nameof(_api.get_scale), Bind<int, int>(_api.get_scale));
            imports.ImportFunc(S, nameof(_api.set_scale), Bind<int, int>(_api.set_scale));
            imports.ImportFunc(S, nameof(_api.get_display), Bind<int, int>(_api.get_display));
            imports.ImportFunc(S, nameof(_api.set_display), Bind<int, int>(_api.set_display));
            imports.ImportFunc(S, nameof(_api.get_id_len), Bind<int, int>(_api.get_id_len));
            imports.ImportFunc(S, nameof(_api.get_id), Bind<int, int, int>(_api.get_id));
            imports.ImportFunc(S, nameof(_api.get_text), Bind<int, int, int>(_api.get_text));
            imports.ImportFunc(S, nameof(_api.get_text_len), Bind<int, int>(_api.get_text_len));
            imports.ImportFunc(S, nameof(_api.set_text), Bind<int, int, int>(_api.set_text));
            imports.ImportFunc(S, nameof(_api.get_background_color), Bind<int, int>(_api.get_background_color));
            imports.ImportFunc(S, nameof(_api.set_background_color), Bind<int, int>(_api.set_background_color));
            imports.ImportFunc(S, nameof(_api.change_anim), Bind<int, int, int, int>(_api.change_anim));
            imports.ImportFunc(S, nameof(_api.change_anim_by_name), Bind<int, int, int, int, int>(_api.change_anim_by_name));
            imports.ImportFunc(S, nameof(_api.get_anim_state), Bind<int, int>(_api.get_anim_state));
            imports.ImportFunc(S, nameof(_api.set_anim_state), Bind<int, int>(_api.set_anim_state));
            imports.ImportFunc(S, nameof(_api.get_current_anim), Bind<int, int>(_api.get_current_anim));
        }

        private void run_loop(ImportedContext context)
        {
            System.Diagnostics.Debug.Assert(WasmScriptRunMode.IsLoopRunning);
            InvokeOnStart();

            while (true)
            {
                if (_exitRequested)
                {
                    _exitRequested = false;
                    break;
                }
                if (_updateRequested)
                {
                    _updateRequested = false;
                    _onUpdateAction?.Invoke();
                }
                if (_selectRequested)
                {
                    _selectRequested = false;
                    _onSelectAction?.Invoke();
                }
                System.Threading.Thread.Sleep(1);
            }
        }

        private Func<ImportedContext, int, int, int, int> BindRegisterEvent()
        {
            return new Func<ImportedContext, int, int, int, int>(
                (c, a1, a2, a3) =>
                {
                    var callbackState = new EventCallbackState(
                        getMemory: () => throw new NotSupportedException(),
                        callback: _onEventCallbackAction);
                    return _api.register_event(c.MemoryPtr, c.MemorySize, a1, a2, a3, callbackState);
                });
        }

        private static TOut IgnoreException<TIn, TOut>(TIn arg, Func<TIn, TOut> f)
        {
            try
            {
                return f(arg);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
