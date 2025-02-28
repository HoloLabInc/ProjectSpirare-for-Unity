using CesiumForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class BaseMapTileset : MonoBehaviour
    {
        [SerializeField]
        private bool useConvexColliders = true;

        public bool UseConvexColliders => useConvexColliders;

        private Cesium3DTileset tileset;

        private bool collidersEnabled = true;

        private readonly List<BaseMapTile> baseMapTiles = new List<BaseMapTile>();
        private readonly Queue<MeshCollider> tileMeshCollidersToBeActive = new Queue<MeshCollider>();

        public bool AllCollidersEnabled
        {
            get
            {
                return tileMeshCollidersToBeActive.Count == 0;
            }
        }

        private void Awake()
        {
            tileset = GetComponent<Cesium3DTileset>();
            tileset.OnTileGameObjectCreated += Tileset_OnTileGameObjectCreated;
        }

        private void Update()
        {
            if (collidersEnabled && tileMeshCollidersToBeActive.Count > 0)
            {
                while (tileMeshCollidersToBeActive.Count > 0)
                {
                    var meshCollider = tileMeshCollidersToBeActive.Dequeue();
                    if (meshCollider == null || meshCollider.enabled || meshCollider.gameObject == null || meshCollider.gameObject.activeInHierarchy == false)
                    {
                        continue;
                    }

                    meshCollider.enabled = true;
                    break;
                }
            }
        }

        public void EnableColliders()
        {
            collidersEnabled = true;

            foreach (var baseMapTile in baseMapTiles)
            {
                if (baseMapTile != null && baseMapTile.gameObject != null && baseMapTile.gameObject.activeInHierarchy)
                {
                    foreach (var meshCollider in baseMapTile.MeshColliders)
                    {
                        tileMeshCollidersToBeActive.Enqueue(meshCollider);
                    }
                }
            }
        }

        public void DisableColliders()
        {
            collidersEnabled = false;

            foreach (var baseMapTile in baseMapTiles)
            {
                if (baseMapTile != null && baseMapTile.gameObject != null)
                {
                    DisableCollider(baseMapTile);
                }
            }
        }

        private void Tileset_OnTileGameObjectCreated(GameObject tileObject)
        {
            var baseMapTile = tileObject.AddComponent<BaseMapTile>();

            var colliders = baseMapTile.AddMeshColliders(convex: useConvexColliders, enabled: false);
            foreach (var collider in colliders)
            {
                tileMeshCollidersToBeActive.Enqueue(collider);
            }

            baseMapTiles.Add(baseMapTile);

            baseMapTile.OnMapTileEnabled += BaseMapTile_OnMapTileEnabled;
            baseMapTile.OnMapTileDisabled += BaseMapTile_OnMapTileDisabled;
            baseMapTile.OnMapTileDestroyed += BaseMapTile_OnMapTileDestroyed;
        }

        private void BaseMapTile_OnMapTileEnabled(BaseMapTile baseMapTile)
        {
            foreach (var meshCollider in baseMapTile.MeshColliders)
            {
                tileMeshCollidersToBeActive.Enqueue(meshCollider);
            }
        }

        private void BaseMapTile_OnMapTileDisabled(BaseMapTile baseMapTile)
        {
            // To distribute the performance impact of enabling the collider over frames, disable the collider once.
            if (useConvexColliders)
            {
                DisableCollider(baseMapTile);
            }
        }

        private void BaseMapTile_OnMapTileDestroyed(BaseMapTile baseMapTile)
        {
            baseMapTiles.Remove(baseMapTile);
        }

        private void DisableCollider(BaseMapTile baseMapTile)
        {
            foreach (var meshCollider in baseMapTile.MeshColliders)
            {
                if (meshCollider != null)
                {
                    meshCollider.enabled = false;
                }
            }
        }
    }
}

