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

        public float PointSize
        {
            get
            {
                return pointSize;
            }
            set
            {
                pointSize = value;
                InvokeOnPointSizeChanged();
            }
        }

        public event Action<float> OnPointSizeChanged;

        private void InvokeOnPointSizeChanged()
        {
            try
            {
                OnPointSizeChanged?.Invoke(pointSize);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
