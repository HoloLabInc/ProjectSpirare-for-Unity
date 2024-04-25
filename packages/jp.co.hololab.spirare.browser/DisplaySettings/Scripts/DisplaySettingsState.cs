using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class DisplaySettingsState : ScriptableObject
    {
        [SerializeField]
        private bool isMenuOpen = false;

        private bool runtimeIsMenuOpen;

        private void OnEnable()
        {
            runtimeIsMenuOpen = isMenuOpen;
        }

        public bool IsMenuOpen
        {
            set
            {
                if (runtimeIsMenuOpen != value)
                {
                    runtimeIsMenuOpen = value;
                    OnIsMenuOpenChanged?.Invoke();
                }
            }
            get
            {
                return runtimeIsMenuOpen;
            }
        }

        public event Action OnIsMenuOpenChanged;
    }
}
