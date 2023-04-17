using HoloLab.Immersal;
using HoloLab.Toolkit.Modules.Yaml;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.HoloLensImmersal
{
    public class ImmersalLocalizationSettingsLoader : MonoBehaviour
    {
        [SerializeField]
        private LocalizationSettings localizationSettings;

        private void Awake()
        {
            LoadImmersalSettings();
        }

        private void LoadImmersalSettings()
        {
            var deserializer = new YamlDeserializer();

            var filepath = "immersal-settings.yaml";
            if (deserializer.TryDeserializeFromPersistentDataPath<ImmersalLocalizationSettings>(filepath, out var settings))
            {
                localizationSettings.Token = settings.Token;
                localizationSettings.MapIds = settings.MapIds;
            }
            else
            {
                Debug.LogWarning($"Failed to load {filepath}");
            }
        }

        public class ImmersalLocalizationSettings
        {
            public string Token { get; set; }

            public List<int> MapIds { get; set; }
        }
    }
}
