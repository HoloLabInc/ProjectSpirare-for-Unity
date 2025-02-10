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

        public event Action<float> OnPointSizeChanged;

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

        private void OnEnable()
        {
            runtimePointSize = pointSize;
        }
    }
}

