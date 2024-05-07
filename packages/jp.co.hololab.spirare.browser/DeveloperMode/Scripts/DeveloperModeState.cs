using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class DeveloperModeState : ScriptableObject
    {
        #region Enabled

        [SerializeField]
        private bool enabled = false;
        private bool runtimeEnabled;

        public bool Enabled
        {
            set
            {
                if (runtimeEnabled != value)
                {
                    runtimeEnabled = value;
                    OnEnabledChanged?.Invoke(value);
                }
            }
            get
            {
                return runtimeEnabled;
            }
        }

        public event Action<bool> OnEnabledChanged;

        #endregion

        private void OnEnable()
        {
            runtimeEnabled = enabled;
        }
    }
}

