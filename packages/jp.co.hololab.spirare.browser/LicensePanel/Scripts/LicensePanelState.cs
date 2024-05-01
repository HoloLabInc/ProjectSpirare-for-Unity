using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class LicensePanelState : ScriptableObject
    {
        [SerializeField]
        private bool isPanelOpen = false;

        private bool runtimeIsPanelOpen;

        private void OnEnable()
        {
            runtimeIsPanelOpen = isPanelOpen;
        }

        public bool IsPanelOpen
        {
            set
            {
                if (runtimeIsPanelOpen != value)
                {
                    runtimeIsPanelOpen = value;
                    OnIsPanelOpenChanged?.Invoke();
                }
            }
            get
            {
                return runtimeIsPanelOpen;
            }
        }

        public event Action OnIsPanelOpenChanged;
    }
}
