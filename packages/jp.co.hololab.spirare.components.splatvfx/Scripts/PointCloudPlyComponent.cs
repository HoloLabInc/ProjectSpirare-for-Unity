using HoloLab.Spirare.Pcx;
using UnityEngine;

namespace HoloLab.Spirare.Components.SplatVfx
{
    public class PointCloudPlyComponent : MonoBehaviour
    {
        private enum RenderMode { Mesh, PointCloud }

        [SerializeField]
        private RenderMode renderMode = RenderMode.PointCloud;

        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private PointCloudRenderer pointCloudRenderer;

        [SerializeField]
        private PointCloudRenderSettings pointCloudRenderSettings;

        private MeshRenderer meshRenderer;
        private Material meshMaterial;

        private void Awake()
        {
            switch (renderMode)
            {
                case RenderMode.Mesh:
                    meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                    meshMaterial = meshRenderer.material;
                    GameObject.Destroy(pointCloudRenderer);
                    break;
                case RenderMode.PointCloud:
                    GameObject.Destroy(meshFilter.gameObject);
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
            }
        }
    }
}

