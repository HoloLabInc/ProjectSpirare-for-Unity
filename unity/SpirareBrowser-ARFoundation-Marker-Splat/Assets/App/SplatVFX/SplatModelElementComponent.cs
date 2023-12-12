using UnityEngine;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.VFX;

namespace HoloLab.Spirare
{
    public sealed class SplatModelElementComponent : ModelElementComponent
    {
        private bool localModel;
        private Animation _animation;
        private AnimationState[] _animationStates;

        private GameObject currentModelObject;
        private string _currentModelSource;

        private CameraVisibleHelper[] _cameraVisibleHelpers;

        private VisualEffect splatPrefab;

        private static readonly SplatVfxSplatLoader splatLoader = new SplatVfxSplatLoader();

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

        public void Initialize(PomlModelElement element, PomlLoadOptions loadOptions, VisualEffect splatPrefab)
        {
            Initialize(element, loadOptions);
            this.splatPrefab = splatPrefab;
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

            var loadResult = await splatLoader.LoadAsync(transform, element.Src, splatPrefab);
            // onLoadingStatusChanged: OnLoadingStatusChanged);
            currentModelObject = loadResult.SplatObject;

            _currentModelSource = element.Src;

            await UniTask.Yield();
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

        /*
        private void OnLoadingStatusChanged(GltfastGlbLoader.LoadingStatus loadingStatus)
        {
            PomlElementLoadingStatus pomlElementLoadingStatus;
            switch (loadingStatus)
            {
                case GltfastGlbLoader.LoadingStatus.DataFetching:
                    pomlElementLoadingStatus = PomlElementLoadingStatus.DataFetching;
                    break;
                case GltfastGlbLoader.LoadingStatus.ModelLoading:
                case GltfastGlbLoader.LoadingStatus.ModelInstantiating:
                    pomlElementLoadingStatus = PomlElementLoadingStatus.Loading;
                    break;
                case GltfastGlbLoader.LoadingStatus.Loaded:
                    pomlElementLoadingStatus = PomlElementLoadingStatus.Loaded;
                    break;
                case GltfastGlbLoader.LoadingStatus.DataFetchError:
                    pomlElementLoadingStatus = PomlElementLoadingStatus.DataFetchError;
                    break;
                case GltfastGlbLoader.LoadingStatus.ModelLoadError:
                case GltfastGlbLoader.LoadingStatus.ModelInstantiateError:
                    pomlElementLoadingStatus = PomlElementLoadingStatus.LoadError;
                    break;
                default:
                    return;
            }

            ChangeLoadingStatus(pomlElementLoadingStatus);
        }
        */
    }
}
