using HoloLab.Spirare.Pcx;
using UnityEngine;

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
        private PointCloudVfxComponent pointCloudVfxPrefab;

        [SerializeField]
        private PointCloudRenderSettings pointCloudRenderSettings;

        private MeshRenderer meshRenderer;
        private Material meshMaterial;
        private PointCloudVfxComponent pointCloudVfx;

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

                if (renderMode == RenderMode.VFXGraph)
                {
                    pointCloudRenderSettings.AddReferrer(this);
                }
            }
        }

        private void OnDestroy()
        {
            if (pointCloudRenderSettings != null)
            {
                if (renderMode == RenderMode.VFXGraph)
                {
                    pointCloudRenderSettings.RemoveReferrer(this);
                }
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
                    pointCloudVfx.SetBakedPointCloud(bakedCloud);

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
                        pointCloudVfx.SetPointSize(pointSize);
                    }
                    break;
            }
        }
    }
}

