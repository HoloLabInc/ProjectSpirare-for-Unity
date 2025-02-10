using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class PointCloudRenderSettings : ScriptableObject
    {
        [SerializeField]
        private float pointSize = 0f;
        private float runtimePointSize;

        public float PointSize
        {
            get
            {
                return runtimePointSize;
            }
            set
            {
                if (runtimePointSize != value)
                {
                    runtimePointSize = value;
                    InvokeOnPointSizeChanged(value);
                }
            }
        }

        private readonly List<Component> referrers = new List<Component>();
        public List<Component> Referrers => referrers;

        public event Action<float> OnPointSizeChanged;
        public event Action<List<Component>> OnReferrersChanged;

        private void OnEnable()
        {
            runtimePointSize = pointSize;
        }

        public void AddReferrer(Component referrer)
        {
            if (referrers.Contains(referrer) == false)
            {
                referrers.Add(referrer);
                InvokeOnReferrersChanged();
            }
        }

        public void RemoveReferrer(Component referrer)
        {
            if (referrers.Remove(referrer))
            {
                InvokeOnReferrersChanged();
            }
        }

        private void InvokeOnPointSizeChanged(float size)
        {
            try
            {
                OnPointSizeChanged?.Invoke(size);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void InvokeOnReferrersChanged()
        {
            try
            {
                OnReferrersChanged?.Invoke(referrers);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}

