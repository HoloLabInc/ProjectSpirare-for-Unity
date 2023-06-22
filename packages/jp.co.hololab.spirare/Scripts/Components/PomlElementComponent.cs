using System;
using System.Threading;
using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class PomlElementComponent : MonoBehaviour
    {
        public virtual PomlElement PomlElement { get; protected set; }

        protected PomlDisplayType currentDisplayInHierarchy;
        protected PomlDisplayType currentArDisplayInHierarchy;

        private PomlElementComponent parentElementComponent;

        private SynchronizationContext mainThreadContext;
        private int mainThreadId = -1;

        public event Action<PomlElement> OnElementUpdated;
        public event Action<PomlElement> OnElementDisplayTypeUpdated;

        public event Action<PomlElementComponent> OnUpdate;
        public event Action<PomlElementComponent> OnDestroyed;

        /// <summary>
        /// This method should be called from the main thread.
        /// </summary>
        public virtual void Initialize(PomlElement element)
        {
            PomlElement = element;

            currentDisplayInHierarchy = element.DisplayInHierarchy;
            currentArDisplayInHierarchy = element.ArDisplayInHierarchy;

            mainThreadContext = SynchronizationContext.Current;
            mainThreadId = Thread.CurrentThread.ManagedThreadId;

            var parent = transform.parent;
            if (parent != null && parent.TryGetComponent(out parentElementComponent))
            {
                parentElementComponent.OnElementDisplayTypeUpdated += ParentElementComponent_OnElementDisplayTypeUpdated;
            }
        }

        private void ParentElementComponent_OnElementDisplayTypeUpdated(PomlElement parentElement)
        {
            InvokeElementUpdated();
        }

        protected virtual void OnDestroy()
        {
            if (parentElementComponent != null)
            {
                parentElementComponent.OnElementDisplayTypeUpdated -= ParentElementComponent_OnElementDisplayTypeUpdated;
            }

            OnDestroyed?.Invoke(this);
        }

        public void InvokeElementUpdated()
        {
            var displayInHierarchyChanged =
                currentDisplayInHierarchy != PomlElement.DisplayInHierarchy ||
                currentArDisplayInHierarchy != PomlElement.ArDisplayInHierarchy;

            if (displayInHierarchyChanged)
            {
                currentDisplayInHierarchy = PomlElement.DisplayInHierarchy;
                currentArDisplayInHierarchy = PomlElement.ArDisplayInHierarchy;
            }

            var threadId = Thread.CurrentThread.ManagedThreadId;
            if (threadId == mainThreadId)
            {
                InvokeOnElementUpdated();
                if (displayInHierarchyChanged)
                {
                    InvokeOnElementDisplayTypeUpdated();
                }
            }
            else
            {
                mainThreadContext.Send(_ =>
                {
                    InvokeOnElementUpdated();
                    if (displayInHierarchyChanged)
                    {
                        InvokeOnElementDisplayTypeUpdated();
                    }
                }, null);
            }
        }

        protected void InvokeOnUpdate() => OnUpdate?.Invoke(this);

        private void InvokeOnElementUpdated()
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

        private void InvokeOnElementDisplayTypeUpdated()
        {
            try
            {
                OnElementDisplayTypeUpdated?.Invoke(PomlElement);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
