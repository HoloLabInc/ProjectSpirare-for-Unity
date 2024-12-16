using UnityEngine;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.VFX;

namespace HoloLab.Spirare.Components.SplatVfx
{
    public sealed class SplatModelElementComponent : ModelElementComponent
    {
        private enum ModelType
        {
            None,
            Splat,
            PointCloud
        }

        private GameObject currentModelObject;
        private string _currentModelSource;

        private CameraVisibleHelper[] _cameraVisibleHelpers;

        private VisualEffect splatPrefab;
        private PointCloudPlyComponent pointCloudPrefab;

        private static readonly SplatVfxSplatLoader splatLoader = new SplatVfxSplatLoader();
        private static readonly SplatVfxSpzLoader spzLoader = new SplatVfxSpzLoader();
        private static readonly SplatVfxPlyLoader splatPlyLoader = new SplatVfxPlyLoader();
        private static readonly PointCloudPlyLoader pointCloudPlyLoader = new PointCloudPlyLoader();

        private const string hiddenLayerName = "SpirareHidden";
        private int HiddenLayer => ConvertLayerNameToLayer(hiddenLayerName);

        public override WrapMode WrapMode
        {
            get
            {
                return WrapMode.Default;
            }
            set
            {
                return;
            }
        }

        public void Initialize(PomlModelElement element, PomlLoadOptions loadOptions, VisualEffect splatPrefab, PointCloudPlyComponent pointCloudPrefab)
        {
            Initialize(element, loadOptions);
            this.splatPrefab = splatPrefab;
            this.pointCloudPrefab = pointCloudPrefab;
        }

        private async void OnEnable()
        {
            await ShowSplatModel();
        }

        private void OnDestroy()
        {
            if (currentModelObject != null)
            {
                Destroy(currentModelObject);
                currentModelObject = null;
            }
        }

        public override bool IsWithinCamera(Camera camera)
        {
            return _cameraVisibleHelpers?.Any(x => x.IsInsideCameraBounds(camera)) ?? false;
        }

        protected override async Task UpdateGameObjectCore()
        {
            if (_currentModelSource == element.Src && currentDisplayType == DisplayType)
            {
                return;
            }

            if (currentModelObject != null)
            {
                Destroy(currentModelObject);
                currentModelObject = null;
                ChangeLoadingStatus(PomlElementLoadingStatus.NotLoaded);
            }

            currentDisplayType = DisplayType;

            if (DisplayType != PomlDisplayType.Visible)
            {
                return;
            }

            _cameraVisibleHelpers = null;

            bool success = false;
            GameObject modelObject = null;
            ModelType modelType = ModelType.None;

            var cancellationToken = this.GetCancellationTokenOnDestroy();

            switch (element.GetSrcFileExtension())
            {
                case ".splat":
                    (success, modelObject) = await splatLoader.LoadAsync(transform, element.Src, splatPrefab, cancellationToken: cancellationToken);
                    if (success)
                    {
                        modelType = ModelType.Splat;
                    }
                    break;
                case ".spz":
                    (success, modelObject) = await spzLoader.LoadAsync(transform, element.Src, splatPrefab, cancellationToken: cancellationToken);
                    if (success)
                    {
                        modelType = ModelType.Splat;
                    }
                    break;
                case ".ply":
                    SplatVfxPlyLoader.LoadErrorType error;
                    (error, modelObject) = await splatPlyLoader.LoadAsync(transform, element.Src, splatPrefab, cancellationToken: cancellationToken);

                    switch (error)
                    {
                        case SplatVfxPlyLoader.LoadErrorType.None:
                            success = true;
                            if (success)
                            {
                                modelType = ModelType.Splat;
                            }
                            break;
                        case SplatVfxPlyLoader.LoadErrorType.InvalidHeader:
                            // Load as point cloud
                            (success, modelObject) = await pointCloudPlyLoader.LoadAsync(transform, element.Src, pointCloudPrefab, cancellationToken: cancellationToken);
                            if (success)
                            {
                                modelType = ModelType.PointCloud;
                            }
                            break;
                        case SplatVfxPlyLoader.LoadErrorType.UnknownError:
                        case SplatVfxPlyLoader.LoadErrorType.DataFetchError:
                        case SplatVfxPlyLoader.LoadErrorType.InvalidBody:
                        case SplatVfxPlyLoader.LoadErrorType.Cancelled:
                        default:
                            success = false;
                            break;
                    }
                    break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                if (modelObject != null)
                {
                    Destroy(modelObject);
                    return;
                }
            }

            currentModelObject = modelObject;

            if (success && modelType == ModelType.Splat)
            {
                await ShowSplatModel();
            }

            _currentModelSource = element.Src;
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

        public override void PlayAnimation(WrapMode wrap)
        {
        }

        public override void StopAnimation()
        {
        }

        public override bool TryGetCurrentAnimation(out int index)
        {
            index = -1;
            return false;
        }

        private async UniTask ShowSplatModel()
        {
            if (currentModelObject == null)
            {
                return;
            }
            currentModelObject.layer = HiddenLayer;
            currentModelObject.transform.SetParent(null, worldPositionStays: false);

            await UniTask.Yield();
            await UniTask.Yield();

            if (currentModelObject == null)
            {
                return;
            }

            currentModelObject.transform.SetParent(transform, worldPositionStays: false);
            currentModelObject.layer = 0;
        }

        private static int ConvertLayerNameToLayer(string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
            {
                return 0;
            }
            else
            {
                return layer;
            }
        }
    }
}
