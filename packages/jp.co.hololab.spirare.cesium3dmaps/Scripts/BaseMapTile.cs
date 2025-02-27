using System;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class BaseMapTile : MonoBehaviour
    {
        public event Action<GameObject> OnMapTileEnabled;
        public event Action<GameObject> OnMapTileDisabled;

        private void OnEnable()
        {
            OnMapTileEnabled?.Invoke(gameObject);
        }

        private void OnDisable()
        {
            OnMapTileDisabled?.Invoke(gameObject);
        }
    }
}

