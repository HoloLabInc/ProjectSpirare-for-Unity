using UnityEngine;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Linq;

#if UNITY_EDITOR
// using UnityEditor;
#endif

namespace HoloLab.Spirare
{
    public sealed class GltfastModelElementComponent : ModelElementComponent
    {
        private bool localModel;
        private Animation _animation;
        private AnimationState[] _animationStates;

        private GameObject currentModelObject;
        private string _currentModelSource;

        private CameraVisibleHelper[] _cameraVisibleHelpers;

        #region static properties and methods
        // private static readonly GltfastGlbLoader glbLoader = new GltfastGlbLoader();

        /*
        internal static void ClearGltfImportCache()
        {
            glbLoader.ClearGltfImportCache();
        }
        */
        #endregion

        public override WrapMode WrapMode
        {
            get
            {
                if (_animation == null) { return WrapMode.Loop; }
                return _animation.wrapMode;
            }
            set
            {
                if (_animation == null) { return; }
                _animation.wrapMode = value;
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

            if (DisplayType == PomlDisplayType.None)
            {
                return;
            }

            _cameraVisibleHelpers = null;

            Material material = null;
            if (DisplayType == PomlDisplayType.Occlusion)
            {
                material = loadOptions.OcclusionMaterial;
            }

            // var loadResult = await glbLoader.LoadAsync(transform, element.Src, material,
            // onLoadingStatusChanged: OnLoadingStatusChanged);
            currentModelObject = loadResult.GlbObject;

            _currentModelSource = element.Src;

            _animation = GetComponentInChildren<Animation>(true);
            if (_animation != null)
            {
                _animationStates = _animation.OfType<AnimationState>().ToArray();
                _animation.Play();
            }

            _cameraVisibleHelpers = currentModelObject.GetComponentsInChildren<Renderer>(true)
                .Select(renderer =>
                {
                    return renderer.gameObject.AddComponent<CameraVisibleHelper>();
                })
                .ToArray();

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
