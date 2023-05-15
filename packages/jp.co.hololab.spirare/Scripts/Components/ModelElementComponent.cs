using UnityEngine;

namespace HoloLab.Spirare
{
    public abstract class ModelElementComponent : SpecificObjectElementComponentBase<PomlModelElement>, IWithinCamera
    {
        [SerializeField]
        protected string modelSource;

        public string ModelSource
        {
            get => modelSource;
            set => modelSource = value;
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
