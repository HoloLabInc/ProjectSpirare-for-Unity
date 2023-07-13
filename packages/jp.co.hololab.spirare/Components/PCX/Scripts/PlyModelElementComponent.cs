using UnityEngine;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.Networking;

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

        private string cacheFolderPath => Path.Combine(Application.temporaryCachePath, "ply");

        public override WrapMode WrapMode
        {
            get => WrapMode.Default;
            set { }
        }

        private void Awake()
        {
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
                return;
            }

            if (DisplayType == PomlDisplayType.Occlusion)
            {
                DisableRenderer();
                Debug.LogWarning("Occlusion with point cloud is not supprted");
                return;
            }

            if (_currentModelSource != element.Src)
            {
                UnloadPly();

                var (result, savedPath) = await SaveToFileAsync();
                if (result == false)
                {
                    return;
                }

                LoadPly(savedPath);
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

        private async Task<(bool Success, string savedPath)> SaveToFileAsync()
        {
            var src = element.Src;

            var extension = element.GetSrcFileExtension();
            var filename = GetCacheFilename(src, extension);
            var savePath = Path.Combine(cacheFolderPath, filename);

            // TODO use SpirareHttpClient

            using (var request = UnityWebRequest.Get(src))
            {
                request.downloadHandler = new DownloadHandlerFile(savePath);
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return (true, savePath);
                }
                else
                {
                    Debug.LogError(request.error);
                    return (false, null);
                }
            }
        }

        private string GetCacheFilename(string url, string extension)
        {
            var urlBytes = Encoding.UTF8.GetBytes(url);

            using (var provider = MD5.Create())
            {
                var hash = provider.ComputeHash(urlBytes);
                var hashString = BitConverter.ToString(hash).Replace("-", string.Empty);
                return hashString + extension;
            }
        }

        private void LoadPly(string filePath)
        {
            var importer = new RuntimePlyImporter();

            switch (renderMode)
            {
                case RenderMode.Mesh:
                    var mesh = importer.ImportAsMesh(filePath);
                    meshFilter.mesh = mesh;
                    break;
                case RenderMode.PointCloud:
                    var cloud = importer.ImportAsPointCloudData(filePath);
                    pointCloudRenderer.sourceData = cloud;
                    break;
            }
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
