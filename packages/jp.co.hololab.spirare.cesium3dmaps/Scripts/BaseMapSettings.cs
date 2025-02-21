using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    [Serializable]
    public class BaseMapSetting
    {
        [SerializeField]
        private string mapName;

        [SerializeField]
        private GameObject mapPrefab;

        [SerializeField]
        private GameObject creditPrefab;

        public string MapName => mapName;
        public GameObject MapPrefab => mapPrefab;
        public GameObject CreditPrefab => creditPrefab;
    }

    public class BaseMapSettings : ScriptableObject
    {
        [SerializeField]
        private List<BaseMapSetting> baseMaps = new List<BaseMapSetting>();

        public List<BaseMapSetting> BaseMaps => baseMaps;
    }
}

