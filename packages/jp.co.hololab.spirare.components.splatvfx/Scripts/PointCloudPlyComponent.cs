using HoloLab.Spirare.Pcx;
using UnityEngine;
using UnityEngine.VFX;

namespace HoloLab.Spirare.Components.SplatVfx
{
    public class PointCloudPlyComponent : MonoBehaviour
    {
        private enum RenderMode { Mesh, PointCloud, VFXGraph }

        [SerializeField]
        private RenderMode renderMode = RenderMode.PointCloud;

        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private PointCloudRenderer pointCloudRenderer;

        [SerializeField]
        private VisualEffect pointCloudVfxPrefab;

        [SerializeField]
        private PointCloudRenderSettings pointCloudRenderSettings;

        private MeshRenderer meshRenderer;
        private Material meshMaterial;
        private VisualEffect pointCloudVfx;

        private void Awake()
        {
            if (renderMode != RenderMode.PointCloud)
            {
                GameObject.Destroy(pointCloudRenderer.gameObject);
            }

            if (renderMode != RenderMode.Mesh)
            {
                GameObject.Destroy(meshFilter.gameObject);
            }

            switch (renderMode)
            {
                case RenderMode.Mesh:
                    meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                    meshMaterial = meshRenderer.material;
                    break;
            }

            if (pointCloudRenderSettings != null)
            {
                PointCloudRenderSettings_OnPointSizeChanged(pointCloudRenderSettings.PointSize);
                pointCloudRenderSettings.OnPointSizeChanged += PointCloudRenderSettings_OnPointSizeChanged;
            }
        }

        public bool LoadPlyFromFile(string filePath)
        {
            var importer = new RuntimePlyImporter();

            switch (renderMode)
            {
                case RenderMode.Mesh:
                    var mesh = importer.ImportAsMesh(filePath);
                    meshFilter.mesh = mesh;
                    return mesh != null;
                case RenderMode.PointCloud:
                    var cloud = importer.ImportAsPointCloudData(filePath);
                    pointCloudRenderer.sourceData = cloud;
                    return cloud != null;
                case RenderMode.VFXGraph:
                    var bakedCloud = importer.ImportAsBakedPointCloud(filePath);
                    if (bakedCloud == null)
                    {
                        return false;
                    }

                    pointCloudVfx = Instantiate(pointCloudVfxPrefab, transform);
                    pointCloudVfx.transform.localScale = new Vector3(-1, 1, 1);
                    pointCloudVfx.SetUInt("PointCount", (uint)bakedCloud.pointCount);
                    pointCloudVfx.SetTexture("PositionMap", bakedCloud.positionMap);
                    pointCloudVfx.SetTexture("ColorMap", bakedCloud.colorMap);

                    if (pointCloudRenderSettings != null)
                    {
                        PointCloudRenderSettings_OnPointSizeChanged(pointCloudRenderSettings.PointSize);
                    }
                    return true;
            }
            return false;
        }

        private void PointCloudRenderSettings_OnPointSizeChanged(float pointSize)
        {
            switch (renderMode)
            {
                case RenderMode.Mesh:
                    meshMaterial.SetFloat("_PointSize", pointSize / 2);
                    break;
                case RenderMode.PointCloud:
                    pointCloudRenderer.pointSize = pointSize / 2;
                    break;
                case RenderMode.VFXGraph:
                    if (pointCloudVfx != null)
                    {
                        // pointCloudVfx.SetFloat("PointSize", pointSize);
                        // TODO
                    }
                    break;
            }
        }
    }
}

