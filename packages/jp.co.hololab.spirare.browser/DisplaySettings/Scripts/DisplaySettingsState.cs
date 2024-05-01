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

        public bool IsMenuOpen
        {
            set
            {
                if (runtimeIsMenuOpen != value)
                {
                    runtimeIsMenuOpen = value;
                    OnIsMenuOpenChanged?.Invoke(value);
                }
            }
            get
            {
                return runtimeIsMenuOpen;
            }
        }

        public event Action<bool> OnIsMenuOpenChanged;

        [SerializeField]
        private float opacity = 1f;
        private float runtimeOpacity;

        public float Opacity
        {
            set
            {
                if (runtimeOpacity != value)
                {
                    runtimeOpacity = value;
                    OnOpacityChanged?.Invoke(value);
                }
            }
            get
            {
                return runtimeOpacity;
            }
        }

        public event Action<float> OnOpacityChanged;

        private void OnEnable()
        {
            runtimeIsMenuOpen = isMenuOpen;
            runtimeOpacity = opacity;
        }
    }
}

