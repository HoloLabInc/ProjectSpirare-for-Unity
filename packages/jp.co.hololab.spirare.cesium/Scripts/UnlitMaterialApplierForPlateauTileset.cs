using CesiumForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    public class UnlitMaterialApplierForPlateauTileset : MonoBehaviour
    {
        [SerializeField]
        private Shader unlitShader;

        private Cesium3DTileset tileset;

        private void Start()
        {
            tileset = GetComponent<Cesium3DTileset>();
            tileset.OnTileGameObjectCreated += OnTileGameObjectCreated;
        }

        private bool UseUnlitShader()
        {
            var url = tileset.url;
            return url.Contains("plateau");
        }

        private void OnTileGameObjectCreated(GameObject tileObject)
        {
            if (UseUnlitShader() == false)
            {
                return;
            }

            var meshRenderers = tileObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                var material = meshRenderer.material;
                var baseColorTexture = material.GetTexture("_baseColorTexture");

                if (baseColorTexture != null)
                {
                    material.shader = unlitShader;
                }
            }
        }
    }
}
