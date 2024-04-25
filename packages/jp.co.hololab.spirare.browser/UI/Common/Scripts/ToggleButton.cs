using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if LITMOTION_PRESENT
using LitMotion;
using LitMotion.Extensions;
#endif

namespace HoloLab.Spirare.Browser.UI
{
    [RequireComponent(typeof(Button))]
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField]
        private bool isOn;

        public bool IsOn
        {
            get
            {
                return isOn;
            }
            set
            {
                if (isOn != value)
                {
                    isOn = value;
                    InvokeOnToggle();
                    ChangeButtonState();
                }
            }
        }

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private RectTransform handle;

        [SerializeField]
        private Color backgroundColorOff = new Color(0.74f, 0.74f, 0.74f);

        [SerializeField]
        private Color backgroundColorOn = new Color(0.3843137f, 0.6313726f, 0.8f);

        private float handlePositionX;

#if LITMOTION_PRESENT
        private CompositeMotionHandle motionHandle;
#endif

        private const float toggleDuration = 0.36f;

        public event Action<bool> OnToggle;

        private void OnValidate()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColorOff;
            }
        }

        private void Awake()
        {
#if !LITMOTION_PRESENT
            Debug.LogWarning($"LitMotion should be imported to use {nameof(ToggleButton)}.");
#endif
            handlePositionX = Mathf.Abs(handle.anchoredPosition.x);

            ChangeButtonStateWithoutAnimation();

            var button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }

        public void Toggle()
        {
            isOn = !isOn;
            InvokeOnToggle();
            ChangeButtonState();
        }

        public void ChangeStateWithoutAnimation(bool isOn)
        {
            if (this.isOn != isOn)
            {
                this.isOn = isOn;
                InvokeOnToggle();
                ChangeButtonStateWithoutAnimation();
            }
        }

        private void OnButtonClick()
        {
            Toggle();
        }

        private void InvokeOnToggle()
        {
            try
            {
                OnToggle?.Invoke(isOn);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void ChangeButtonState()
        {
#if LITMOTION_PRESENT
            if (motionHandle != null)
            {
                motionHandle.Complete();
            }

            var backgroundColor = isOn ? backgroundColorOn : backgroundColorOff;
            var handleDestination = isOn ? handlePositionX : -handlePositionX;

            motionHandle = new CompositeMotionHandle();

            LMotion.Create(backgroundImage.color, backgroundColor, toggleDuration)
                .WithEase(Ease.OutQuad)
                .BindWithState(backgroundImage, (x, target) => target.color = x)
                .AddTo(motionHandle);

            LMotion.Create(handle.anchoredPosition.x, handleDestination, toggleDuration / 2)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPositionX(handle)
                .AddTo(motionHandle);
#else
            ChangeButtonStateWithoutAnimation();
#endif
        }

        private void ChangeButtonStateWithoutAnimation()
        {
#if LITMOTION_PRESENT
            if (motionHandle != null)
            {
                motionHandle.Complete();
                motionHandle = null;
            }
#endif

            var backgroundColor = isOn ? backgroundColorOn : backgroundColorOff;
            var handleDestination = isOn ? handlePositionX : -handlePositionX;
            backgroundImage.color = backgroundColor;
            handle.anchoredPosition = new Vector2(handleDestination, handle.anchoredPosition.y);
        }
    }
}

