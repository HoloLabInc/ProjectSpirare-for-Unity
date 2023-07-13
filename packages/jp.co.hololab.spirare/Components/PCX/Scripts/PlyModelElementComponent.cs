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
        private RenderMode importMode = RenderMode.PointCloud;

        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private PointCloudRenderer pointCloudRenderer;

        private string _currentModelSource;

        private CameraVisibleHelper[] _cameraVisibleHelpers;

        private string cacheFolderPath => Path.Combine(Application.temporaryCachePath, "ply");

        public override WrapMode WrapMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void Awake()
        {
            switch (importMode)
            {
                case RenderMode.Mesh:
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
            return _cameraVisibleHelpers?.Any(x => x.IsInsideCameraBounds(camera)) ?? false;
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

            /*
            if (currentModelObject != null)
            {
                Destroy(currentModelObject);
                currentModelObject = null;
                ChangeLoadingStatus(PomlElementLoadingStatus.NotLoaded);
            }
            */

            currentDisplayType = DisplayType;

            if (DisplayType == PomlDisplayType.None)
            {
                return;
            }

            if (DisplayType == PomlDisplayType.Occlusion)
            {
                Debug.LogWarning("Occlusion with point cloud is not supprted");
                return;
            }

            _cameraVisibleHelpers = null;

            var (result, savedPath) = await SaveToFileAsync();
            if (result == false)
            {
                return;
            }

            LoadPly(savedPath);

            _currentModelSource = element.Src;

            /*
            _cameraVisibleHelpers = currentModelObject.GetComponentsInChildren<Renderer>(true)
                .Select(renderer =>
                {
                    return renderer.gameObject.AddComponent<CameraVisibleHelper>();
                })
                .ToArray();
            */

            await UniTask.Yield();
        }

        private async Task<(bool Success, string savedPath)> SaveToFileAsync()
        {
            var src = element.Src;

            var extension = element.GetSrcFileExtension();
            var filename = GetCacheFilename(src, extension);
            var savePath = Path.Combine(cacheFolderPath, filename);

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

            switch (importMode)
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
    }
}
