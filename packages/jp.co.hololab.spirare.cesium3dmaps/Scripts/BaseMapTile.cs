using System;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class BaseMapTile : MonoBehaviour
    {
        private readonly List<MeshCollider> meshColliders = new List<MeshCollider>();
        public IList<MeshCollider> MeshColliders => meshColliders;

        public event Action<BaseMapTile> OnMapTileEnabled;
        public event Action<BaseMapTile> OnMapTileDisabled;
        public event Action<BaseMapTile> OnMapTileDestroyed;

        private void OnEnable()
        {
            OnMapTileEnabled?.Invoke(this);
        }

        private void OnDisable()
        {
            OnMapTileDisabled?.Invoke(this);
        }

        private void OnDestroy()
        {
            OnMapTileDestroyed?.Invoke(this);
        }

        public IList<MeshCollider> AddMeshColliders(bool convex, bool enabled)
        {
            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>(includeInactive: true);

            foreach (var meshFilter in meshFilters)
            {
                var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.enabled = enabled;
                meshCollider.convex = convex;

                meshColliders.Add(meshCollider);
            }

            return meshColliders;
        }
    }
}

