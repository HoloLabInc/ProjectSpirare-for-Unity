using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace HoloLab.Spirare
{
    public abstract class SpecificObjectElementComponentBase<T> : MonoBehaviour where T : PomlElement
    {
        protected T element;

        protected PomlLoadOptions loadOptions;

        protected PomlDisplayType? currentDisplayType = null;

        private readonly AsyncLock asyncLock = new AsyncLock();

        protected PomlDisplayType DisplayType
        {
            get
            {
                switch (loadOptions.DisplayMode)
                {
                    case PomlLoadOptions.DisplayModeType.AR:
                        return element.ArDisplayInHierarchy;
                    case PomlLoadOptions.DisplayModeType.Normal:
                    default:
                        return element.DisplayInHierarchy;
                }
            }
        }

        protected int Layer
        {
            get
            {
                if (element.IsRenderedInScreenSpace())
                {
                    return loadOptions.ScreenSpaceLayer;
                }
                else
                {
                    return loadOptions.DefaultLayer;
                }
            }
        }

        public virtual void Initialize(T element, PomlLoadOptions loadOptions)
        {
            this.element = element;
            this.loadOptions = loadOptions;

            if (TryGetComponent<PomlObjectElementComponent>(out var pec))
            {
                var updateRequested = false;
                var updating = false;
                pec.OnElementUpdated += async _ =>
                {
                    // It is difficult to maintain the consistency of flags in multithreading, so the processing is delegated to the main thread.
                    var isMainThread = SynchronizationContext.Current != null;
                    if (isMainThread == false)
                    {
                        await UniTask.SwitchToMainThread();
                    }

                    // Since UpdateGameObject might not finish in one frame, an update request flag is used.
                    updateRequested = true;
                    if (updating) { return; }
                    try
                    {
                        updating = true;
                        while (updateRequested)
                        {
                            updateRequested = false;
                            try
                            {
                                await UpdateGameObject();
                            }
                            catch
                            {
                            }
                        }
                    }
                    finally
                    {
                        updating = false;
                    }
                };
            }
        }

        public virtual async Task UpdateGameObject()
        {
            using (await asyncLock.LockAsync())
            {
                await UpdateGameObjectCore();
            }
        }

        protected abstract Task UpdateGameObjectCore();
    }
}
