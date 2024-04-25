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

        public bool IsMenuOpen
        {
            set
            {
                if (isMenuOpen != value)
                {
                    isMenuOpen = value;
                    OnIsMenuOpenChanged?.Invoke();
                }
            }
            get
            {
                return isMenuOpen;
            }
        }

        public event Action OnIsMenuOpenChanged;
    }
}
