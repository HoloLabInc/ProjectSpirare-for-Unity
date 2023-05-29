using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using HoloLab.Spirare.Wasm.Core.Spirare;
using System.Linq;

namespace HoloLab.Spirare.Wasm.Iwasm
{
    public sealed class IwasmScriptElementComponent : PomlElementComponent
    {
        private PomlObjectElementComponent parentElementComponent;
        private ElementDescriptorHelper elementDescriptorHelper;

        // when IsLoopRunning == true
        private Action _onUpdate;
        private Action _onDestroy;
        private Action _onSelect;

        // when IsLoopRunning == false
        private IwasmEngine _engineNonLoop;
        private bool _onStartCalledNonLoop;

        internal void Initialize(PomlScriptElement scriptElement, PomlObjectElementComponent parentElementComponent, PomlComponent pomlComponent)
        {
            PomlElement = scriptElement;
            this.parentElementComponent = parentElementComponent;
            elementDescriptorHelper = new ElementDescriptorHelper(
                selfObjectElementComponent: parentElementComponent,
                selfScriptElementComponent: this,
                pomlComponent: pomlComponent);
        }

        private async void Start()
        {
            if (parentElementComponent != null)
            {
                parentElementComponent.OnSelect += ParentOnSelect;
            }
            await LoadScriptAsync();
        }

        private void Update()
        {
            if (WasmScriptRunMode.IsLoopRunning)
            {
                _onUpdate?.Invoke();
            }
            else
            {
                if (_onStartCalledNonLoop)
                {
                    _engineNonLoop.RequestUpdate();
                }
            }
        }

        protected override void OnDestroy()
        {
            if (parentElementComponent != null)
            {
                parentElementComponent.OnSelect -= ParentOnSelect;
            }
            if (WasmScriptRunMode.IsLoopRunning)
            {
                _onDestroy?.Invoke();
            }
            else
            {
                _engineNonLoop?.Dispose();
            }
            base.OnDestroy();
        }

        private void ParentOnSelect()
        {
            if (WasmScriptRunMode.IsLoopRunning)
            {
                _onSelect?.Invoke();
            }
            else
            {
                if (_onStartCalledNonLoop)
                {
                    _engineNonLoop?.RequestSelect();
                }
            }
        }

        private async Task LoadScriptAsync()
        {
            var element = PomlElement as PomlScriptElement;
            var url = element.Src;
            var envs = new string[0];
            var args = element.Args.ToArray();

            var data = await GetScriptDataAsync(url);
            await Task.Delay(1000);     // TODO: Wait until all elements of the poml are loaded (especially glb model).
            if (WasmScriptRunMode.IsLoopRunning)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        using var engine = new IwasmEngine(elementDescriptorHelper);
                        _onUpdate = () => engine.RequestUpdate();
                        _onSelect = () => engine.RequestSelect();
                        _onDestroy = () => engine.RequestExit();
                        engine.Load(data, args, envs);
                        engine.InvokeEntryPoint();
                        _onUpdate = null;
                        _onSelect = null;
                        _onDestroy = null;
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            else
            {
                try
                {
                    _engineNonLoop = new IwasmEngine(elementDescriptorHelper);
                    _engineNonLoop.OnTerminated += () =>
                    {
                        _engineNonLoop = null;
                    };
                    _engineNonLoop.Load(data, args, envs);
                    _engineNonLoop.InvokeEntryPoint();
                    _onStartCalledNonLoop = true;
                }
                catch (Exception ex)
                {
                    _engineNonLoop = null;
                    Debug.LogException(ex);
                }
            }
        }

        private static async Task<byte[]> GetScriptDataAsync(string url)
        {
            var request = UnityWebRequest.Get(url);
            var webRequest = await request.SendWebRequest();
            var data = webRequest.downloadHandler.data;
            return data;
        }
    }

    internal static class WasmScriptRunMode
    {
        public static bool IsLoopRunning => false;
    }
}
