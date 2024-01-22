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
        private GameObject currentModelObject;
        private string _currentModelSource;

        private CameraVisibleHelper[] _cameraVisibleHelpers;

        private VisualEffect splatPrefab;

        private static readonly SplatVfxSplatLoader splatLoader = new SplatVfxSplatLoader();

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

        public void Initialize(PomlModelElement element, PomlLoadOptions loadOptions, VisualEffect splatPrefab)
        {
            Initialize(element, loadOptions);
            this.splatPrefab = splatPrefab;
        }

        private async void OnEnable()
        {
            await ShowSplatModel();
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

            var (success, splatObject) = await splatLoader.LoadAsync(transform, element.Src, splatPrefab);
            currentModelObject = splatObject;

            if (success)
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
