using System.Threading.Tasks;
using GLTFast;
using GLTFast.Logging;
using GLTFast.Materials;
using UnityEngine;
using System.Threading;

namespace HoloLab.Spirare
{
    internal static class GltfastGlbLoader
    {
        public static async Task LoadAsync(GameObject go, string src, Material material = null)
        {
            var result = await SpirareHttpClient.Instance.GetByteArrayAsync(src, enableCache: true);
            if (result.Success == false)
            {
                Debug.LogWarning($"Failed to get model data: {src}");
                return;
            }

            IMaterialGenerator materialGenerator = null;
            if (material != null)
            {
                materialGenerator = new OcclusionMaterialGenerator(material);
            }

            var gltfImport = new GltfImport(materialGenerator: materialGenerator);
            var loadResult = await gltfImport.LoadGltfBinary(result.Data);
            if (loadResult && go != null)
            {
                await gltfImport.InstantiateMainSceneAsync(go.transform, CancellationToken.None);
            }
        }
    }

    internal class OcclusionMaterialGenerator : IMaterialGenerator
    {
        private ICodeLogger logger;
        private Material occlusionMaterial;

        public OcclusionMaterialGenerator(Material occlusionMaterial)
        {
            this.occlusionMaterial = occlusionMaterial;
        }

        public Material GenerateMaterial(GLTFast.Schema.Material gltfMaterial, IGltfReadable gltf, bool pointsSupport = false)
        {
            return occlusionMaterial;
        }

        public Material GetDefaultMaterial(bool pointsSupport = false)
        {
            return occlusionMaterial;
        }

        public void SetLogger(ICodeLogger logger)
        {
            this.logger = logger;
        }
    }
}
