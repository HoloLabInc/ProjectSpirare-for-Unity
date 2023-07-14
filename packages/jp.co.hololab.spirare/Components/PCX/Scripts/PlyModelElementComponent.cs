using UnityEngine;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace HoloLab.Spirare.Pcx
{
    public sealed class PlyModelElementComponent : ModelElementComponent
    {
        private enum RenderMode { Mesh, PointCloud }

        [SerializeField]
        private RenderMode renderMode = RenderMode.PointCloud;

        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private PointCloudRenderer pointCloudRenderer;

        private MeshRenderer meshRenderer;

        private string _currentModelSource;

        private SpirareHttpClient spirareHttpClient;

        public override WrapMode WrapMode
        {
            get => WrapMode.Default;
            set { }
        }

        private void Awake()
        {
            spirareHttpClient = SpirareHttpClient.Instance;

            switch (renderMode)
            {
                case RenderMode.Mesh:
                    meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                    GameObject.Destroy(pointCloudRenderer);
                    break;
                case RenderMode.PointCloud:
                    GameObject.Destroy(meshFilter.gameObject);
                    break;
            }
        }

        public override bool ChangeAnimation(int animationIndex)
        {
            return false;
        }

        public override bool ChangeAnimation(string animationName)
        {
            return false;
        }

        public override bool IsAnimationPlaying()
        {
            return false;
        }

        public override bool IsWithinCamera(Camera camera)
        {
            // Not supported
            return false;
        }

        public override void PlayAnimation(WrapMode wrap)
        {
        }

        public override void StopAnimation()
        {
        }

        public override bool TryGetCurrentAnimation(out int index)
        {
            index = 0;
            return false;
        }

        protected override async Task UpdateGameObjectCore()
        {
            if (_currentModelSource == element.Src && currentDisplayType == DisplayType)
            {
                return;
            }

            currentDisplayType = DisplayType;

            if (DisplayType == PomlDisplayType.None)
            {
                DisableRenderer();
                ChangeLoadingStatus(PomlElementLoadingStatus.NotLoaded);
                return;
            }

            if (DisplayType == PomlDisplayType.Occlusion)
            {
                DisableRenderer();
                ChangeLoadingStatus(PomlElementLoadingStatus.NotLoaded);
                Debug.LogWarning("Occlusion with point cloud is not supprted");
                return;
            }

            if (_currentModelSource != element.Src)
            {
                UnloadPly();
                ChangeLoadingStatus(PomlElementLoadingStatus.NotLoaded);

                var loadResult = await LoadPlyAsync();
                if (loadResult == false)
                {
                    Debug.LogWarning($"Failed to load {element.Src}");
                }
            }

            EnableRenderer();

            _currentModelSource = element.Src;
        }

        private void EnableRenderer()
        {
            SetRendererActive(true);
        }

        private void DisableRenderer()
        {
            SetRendererActive(false);
        }

        private void SetRendererActive(bool enabled)
        {
            switch (renderMode)
            {
                case RenderMode.Mesh:
                    meshRenderer.enabled = enabled;
                    break;
                case RenderMode.PointCloud:
                    pointCloudRenderer.enabled = enabled;
                    break;
            }
        }

        private async UniTask<bool> LoadPlyAsync()
        {
            bool loadResult;

            var src = element.Src;
            if (src.StartsWith("file://"))
            {
                ChangeLoadingStatus(PomlElementLoadingStatus.DataFetching);
                ChangeLoadingStatus(PomlElementLoadingStatus.Loading);
                loadResult = LoadPlyFromFile(src);
            }
            else
            {
                // Download from URL
                ChangeLoadingStatus(PomlElementLoadingStatus.DataFetching);
                var result = await spirareHttpClient.DownloadToFileAsync(src, enableCache: true);
                if (result.Success == false)
                {
                    ChangeLoadingStatus(PomlElementLoadingStatus.DataFetchError);
                    return false;
                }

                // Load from file
                ChangeLoadingStatus(PomlElementLoadingStatus.Loading);
                loadResult = LoadPlyFromFile(result.Data);
            }

            if (loadResult)
            {
                ChangeLoadingStatus(PomlElementLoadingStatus.Loaded);
            }
            else
            {
                ChangeLoadingStatus(PomlElementLoadingStatus.LoadError);
            }
            return loadResult;
        }

        private bool LoadPlyFromFile(string filePath)
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

        private void UnloadPly()
        {
            switch (renderMode)
            {
                case RenderMode.Mesh:
                    var mesh = meshFilter.mesh;
                    meshFilter.mesh = null;
                    Destroy(mesh);
                    break;
                case RenderMode.PointCloud:
                    var cloud = pointCloudRenderer.sourceData;
                    pointCloudRenderer.sourceData = null;
                    Destroy(cloud);
                    break;
            }
        }
    }
}
