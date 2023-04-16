using System;
using System.Threading;
using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class PomlElementComponent : MonoBehaviour
    {
        public virtual PomlElement PomlElement { get; protected set; }

        private SynchronizationContext mainThreadContext;
        private int mainThreadId = -1;


        public event Action<PomlElement> OnElementUpdated;

        public event Action<PomlElementComponent> OnUpdate;
        public event Action<PomlElementComponent> OnDestroyed;

        private void Awake()
        {
            mainThreadContext = SynchronizationContext.Current;
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        protected virtual void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }

        internal void InvokeElementUpdated()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;

            if (threadId == mainThreadId)
            {
                try
                {
                    OnElementUpdated?.Invoke(PomlElement);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            else
            {
                mainThreadContext.Send(_ =>
                {
                    try
                    {
                        OnElementUpdated?.Invoke(PomlElement);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }, null);
            }
        }

        protected void InvokeOnUpdate() => OnUpdate?.Invoke(this);
    }
}
