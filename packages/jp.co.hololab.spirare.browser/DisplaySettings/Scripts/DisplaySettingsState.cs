using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class DisplaySettingsState : ScriptableObject
    {
        #region IsMenuOpen

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

        #endregion

        #region Opacity

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

        #endregion

        #region FarClip

        [SerializeField]
        private float farClip = 100f;
        private float runtimeFarClip;

        public float FarClip
        {
            set
            {
                if (runtimeFarClip != value)
                {
                    runtimeFarClip = value;
                    OnFarClipChanged?.Invoke(value);
                }
            }
            get
            {
                return runtimeFarClip;
            }
        }

        public event Action<float> OnFarClipChanged;

        #endregion

        #region Occlusion

        public enum OcclusionType
        {
            None = 0,
            EnvironmentFastest,
            EnvironmentMedium,
            EnvironmentBest,
            HumanFastest,
            HumanBest
        }

        [SerializeField]
        private OcclusionType occlusion = OcclusionType.None;
        private OcclusionType runtimeOcclusion;

        public OcclusionType Occlusion
        {
            set
            {
                if (runtimeOcclusion != value)
                {
                    runtimeOcclusion = value;
                    OnOcclusionChanged?.Invoke(value);
                }
            }
            get
            {
                return runtimeOcclusion;
            }
        }

        public event Action<OcclusionType> OnOcclusionChanged;

        #endregion

        #region StreetscapeGeometryOcclusion

        [Flags]
        public enum StreetscapeGeometryOcclusionType
        {
            None = 0,
            Building = 1 << 0,
            Terrain = 1 << 1,
            All = Building | Terrain
        }

        [SerializeField]
        private StreetscapeGeometryOcclusionType streetscapeGeometryOcclusion = StreetscapeGeometryOcclusionType.None;
        private StreetscapeGeometryOcclusionType runtimeStreetscapeGeometryOcclusion;

        public StreetscapeGeometryOcclusionType StreetscapeGeometryOcclusion
        {
            set
            {
                if (runtimeStreetscapeGeometryOcclusion != value)
                {
                    runtimeStreetscapeGeometryOcclusion = value;
                    OnStreetscapeGeometryOcclusionChanged?.Invoke(value);
                }
            }
            get
            {
                return runtimeStreetscapeGeometryOcclusion;
            }
        }

        public event Action<StreetscapeGeometryOcclusionType> OnStreetscapeGeometryOcclusionChanged;

        #endregion

        private void OnEnable()
        {
            runtimeIsMenuOpen = isMenuOpen;
            runtimeOpacity = opacity;
            runtimeFarClip = farClip;
            runtimeOcclusion = occlusion;
            runtimeStreetscapeGeometryOcclusion = streetscapeGeometryOcclusion;
        }
    }
}

