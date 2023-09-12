using GLTFast;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class GltfastGlbInstance : MonoBehaviour
    {
        private GltfastGlbLoader gltfastGlbLoader;

        private string src;
        internal string Src => src;

        private Material material;
        internal Material Material => material;

        private GltfImport gltfImport;
        internal GltfImport GltfImport => gltfImport;

        internal void Initialize(GltfastGlbLoader gltfastGlbLoader, string src, Material material)
        {
            this.gltfastGlbLoader = gltfastGlbLoader;
            this.src = src;
            this.material = material;
        }

        internal void SetGltfImport(GltfImport gltfImport)
        {
            this.gltfImport = gltfImport;
        }

        private void OnDestroy()
        {
            gltfastGlbLoader.RemoveInstanceReference(this);
        }
    }
}
