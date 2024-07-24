using System;
using System.Collections;
using System.Collections.Generic;
using CesiumForUnity;
using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps.PolySpatial
{
    [RequireComponent(typeof(Cesium3DTileset))]
    public class TilesetForPolySpatial : MonoBehaviour
    {
        private void Start()
        {
            var cesium3DTileset = GetComponent<Cesium3DTileset>();
            cesium3DTileset.OnTileGameObjectCreated += Cesium3DTileset_OnTileGameObjectCreated;
        }

        private void Cesium3DTileset_OnTileGameObjectCreated(GameObject go)
        {
            var meshFilters = go.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                var mesh = meshFilter.mesh;

                // Change triangle order
                var triangles = mesh.triangles;
                for (var i = 0; i < triangles.Length; i += 3)
                {
                    (triangles[i + 1], triangles[i]) = (triangles[i], triangles[i + 1]);
                }
                mesh.triangles = triangles;
            }
        }
    }
}
