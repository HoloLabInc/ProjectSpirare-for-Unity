using UnityEngine;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoloLab.Spirare
{
    [SelectionBase]
    public sealed class GltfastModelElementComponent : ModelElementComponent
    {
        private bool localModel;
        private Animation _animation;
        private AnimationState[] _animationStates;

        private GameObject currentModelObject;
        private string _currentModelSource;

        private CameraVisibleHelper[] _cameraVisibleHelpers;

        #region static properties and methods
        private static GltfastGlbLoader glbLoader = new GltfastGlbLoader();

        public static void ClearGltfImportCache()
        {
            glbLoader.ClearGltfImportCache();
        }
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateLocalModelFlag();
        }

        private void UpdateLocalModelFlag()
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            if (prefab != null)
            {
                var path = AssetDatabase.GetAssetPath(prefab);
                if (path.EndsWith(".glb"))
                {
                    localModel = true;
                    modelSource = path;
                    return;
                }
            }
            localModel = false;
        }
#endif

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
            currentModelObject = new GameObject("ModelObject");
            currentModelObject.transform.SetParent(transform, false);

            Material material = null;
            if (DisplayType == PomlDisplayType.Occlusion)
            {
                material = loadOptions.OcclusionMaterial;
            }

            await glbLoader.LoadAsync(currentModelObject, element.Src, material,
                onLoadingStatusChanged: OnLoadingStatusChanged);

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
            if (_animation == null || _animationStates == null)
            {
                return false;
            }
            if (animationIndex < 0 || animationIndex >= _animationStates.Length)
            {
                return false;
            }
            ChangeAnimationPrivate(_animationStates[animationIndex].clip);
            return true;
        }

        public override bool ChangeAnimation(string animationName)
        {
            if (_animation == null || _animationStates == null)
            {
                return false;
            }
            var state = _animationStates.FirstOrDefault(x => x.name == animationName);
            if (state == null)
            {
                return false;
            }
            ChangeAnimationPrivate(state.clip);
            return true;
        }

        private void ChangeAnimationPrivate(AnimationClip clip)
        {
            _animation.clip = clip;
            if (_animation.isPlaying)
            {
                _animation.Play();
            }
        }

        public override bool IsAnimationPlaying()
        {
            return (_animation == null) ? false : _animation.isPlaying;
        }

        public override void PlayAnimation(WrapMode wrap)
        {
            if (_animation == null)
            {
                return;
            }
            _animation.wrapMode = wrap;
            _animation.Play();
        }

        public override void StopAnimation()
        {
            if (_animation == null)
            {
                return;
            }
            _animation.Stop();
        }

        public override bool TryGetCurrentAnimation(out int index)
        {
            if (_animation == null || _animationStates == null)
            {
                index = -1;
                return false;
            }
            var current = _animation.clip;
            for (int i = 0; i < _animationStates.Length; i++)
            {
                if (_animationStates[i].name == current.name)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

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

#if UNITY_EDITOR
        [CustomEditor(typeof(GltfastModelElementComponent))]
        private sealed class GltfastModelElementComponentEditor : Editor
        {
            private GltfastModelElementComponent _component;
            private bool _awaiting;

            private void OnEnable()
            {
                _component = target as GltfastModelElementComponent;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                if (_component == null)
                {
                    return;
                }

                // If this is a model in the Unity project, hide the load button.
                if (_component.localModel)
                {
                    return;
                }

                var awaiting = _awaiting;
                if (awaiting)
                {
                    EditorGUI.BeginDisabledGroup(true);
                }

                if (GUILayout.Button("Load", GUILayout.Height(50)))
                {
                    WaitAction(() => _component.UpdateGameObject());
                }

                if (awaiting)
                {
                    EditorGUI.EndDisabledGroup();
                }
            }

            private async void WaitAction(Func<Task> func)
            {
                if (_awaiting) { return; }
                try
                {
                    _awaiting = true;
                    await func();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {
                    _awaiting = false;
                }
            }
        }
#endif
    }
}
