using System;
using UnityEngine;

namespace HoloLab.Spirare
{
    public enum PomlElementLoadingStatus
    {
        NotLoaded,
        DataFetching,
        Loading,
        Loaded,
        DataFetchError,
        LoadError,
        Error,
    }

    public abstract class ModelElementComponent : SpecificObjectElementComponentBase<PomlModelElement>, IWithinCamera
    {
        [SerializeField]
        protected string modelSource;

        public string ModelSource
        {
            get => modelSource;
            set => modelSource = value;
        }

        private PomlElementLoadingStatus loadingStatus = PomlElementLoadingStatus.NotLoaded;
        public PomlElementLoadingStatus LoadingStatus => loadingStatus;

        public Action<PomlElementLoadingStatus> LoadingStatusChanged;

        protected void ChangeLoadingStatus(PomlElementLoadingStatus status)
        {
            if (loadingStatus != status)
            {
                loadingStatus = status;
                try
                {
                    LoadingStatusChanged?.Invoke(status);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public abstract WrapMode WrapMode { get; set; }

        public abstract bool ChangeAnimation(int animationIndex);

        public abstract bool ChangeAnimation(string animationName);

        public abstract bool TryGetCurrentAnimation(out int index);

        public abstract bool IsAnimationPlaying();

        public abstract void PlayAnimation(WrapMode wrap);
        public abstract void StopAnimation();

        public abstract bool IsWithinCamera(Camera camera);
    }
}
