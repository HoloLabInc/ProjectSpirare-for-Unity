using CesiumForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class BaseMapTileset : MonoBehaviour
    {
        private Cesium3DTileset tileset;

        private List<GameObject> tileObjects = new List<GameObject>();

        private Queue<MeshFilter> tileMeshFilters = new Queue<MeshFilter>();

        private Queue<MeshCollider> tileMeshCollidersToBeActive = new Queue<MeshCollider>();

        public bool AllCollidersEnabled
        {
            get
            {
                return tileMeshFilters.Count == 0 && tileMeshCollidersToBeActive.Count == 0;
            }
        }

        private void Awake()
        {
            tileset = GetComponent<Cesium3DTileset>();
            tileset.OnTileGameObjectCreated += Tileset_OnTileGameObjectCreated;
        }

        private void Update()
        {
            if (tileMeshCollidersToBeActive.Count > 0)
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

        private void Tileset_OnTileGameObjectCreated(GameObject tileObject)
        {
            tileObjects.Add(tileObject);
            var baseMapTile = tileObject.AddComponent<BaseMapTile>();
            baseMapTile.OnMapTileEnabled += BaseMapTile_OnMapTileEnabled;
            baseMapTile.OnMapTileDisabled += BaseMapTile_OnMapTileDisabled;

            var meshFilters = tileObject.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            foreach (var meshFilter in meshFilters)
            {
                var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.enabled = false;
                meshCollider.convex = true;

                tileMeshCollidersToBeActive.Enqueue(meshCollider);
            }
        }

        private void BaseMapTile_OnMapTileDisabled(GameObject tileObject)
        {
            var meshColliders = gameObject.GetComponentsInChildren<MeshCollider>(includeInactive: true);
            foreach (var meshCollider in meshColliders)
            {
                meshCollider.enabled = false;
            }
        }

        private void BaseMapTile_OnMapTileEnabled(GameObject tileObject)
        {
            var meshColliders = gameObject.GetComponentsInChildren<MeshCollider>(includeInactive: true);
            foreach (var meshCollider in meshColliders)
            {
                tileMeshCollidersToBeActive.Enqueue(meshCollider);
            }
        }

        public bool TryGetMinimumHeight(out float yInWorldSpace)
        {
            float? minimumHeight = null;

            var count = tileObjects.Count(x => x != null && x.activeInHierarchy);
            if (count < 10)
            {
                yInWorldSpace = 0;
                return false;
            }

            foreach (var tileObject in tileObjects)
            {
                if (tileObject != null && tileObject.activeInHierarchy)
                {
                    var bounds = tileObject.GetComponentInChildren<MeshRenderer>().bounds;
                    var minY = bounds.min.y;

                    if (minimumHeight.HasValue)
                    {
                        minimumHeight = Mathf.Min(minimumHeight.Value, minY);
                    }
                    else
                    {
                        minimumHeight = minY;
                    }
                }
            }

            if (minimumHeight.HasValue)
            {
                yInWorldSpace = minimumHeight.Value;
                return true;
            }
            else
            {
                yInWorldSpace = 0;
                return false;
            }
        }
    }
}

