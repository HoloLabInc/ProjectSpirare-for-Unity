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
    public class SideMenu : MonoBehaviour
    {
        [SerializeField]
        private Button openButton;

        [SerializeField]
        private RectTransform menuScrollView;

        [SerializeField]
        private LayoutElement contentPadding;

        [SerializeField]
        private Button closeButton;

        private RectTransform rectTransform;

        private float defaultMenuWidth;
        private float menuWidth;

#if LITMOTION_PRESENT
        private MotionHandle motionHandle;
#endif

        private Rect previousSafeArea;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            defaultMenuWidth = menuScrollView.rect.width;

#if LITMOTION_PRESENT
            motionHandle = LMotion.Create(0f, 0f, 0.01f)
                .BindToAnchoredPositionX(menuScrollView);
#endif

            AdjustToFitSafeArea();

            openButton.onClick.AddListener(OnOpenButtonClick);
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void Update()
        {
            CloseMenuWhenOutsideTouched();
            AdjustToFitSafeArea();
        }

        private void AdjustToFitSafeArea()
        {
            var safeArea = Screen.safeArea;
            if (safeArea == previousSafeArea)
            {
                return;
            }

            CompleteMotion();

            var scaleX = transform.lossyScale.x;
            var scaleY = transform.lossyScale.y;
            var safeOffsetX = safeArea.x / scaleX;
            var safeOffsetY = safeArea.y / scaleY;

            menuWidth = defaultMenuWidth + safeOffsetX;
            rectTransform.sizeDelta = new Vector2(menuWidth, 0);

            contentPadding.minHeight = safeOffsetY;

            previousSafeArea = safeArea;
        }

        private void CloseMenuWhenOutsideTouched()
        {
#if LITMOTION_PRESENT
            if (motionHandle.IsActive())
            {
                return;
            }
#endif

            if (menuScrollView.gameObject.activeSelf)
            {
                for (var i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began && IsOutsideTouched(touch))
                    {
                        OnCloseButtonClick();
                        return;
                    }
                }
            }
        }

        private bool IsOutsideTouched(Touch touch)
        {
            var insideTouched = RectTransformUtility.RectangleContainsScreenPoint(menuScrollView, touch.position, null);
            return !insideTouched;
        }

        private void OnOpenButtonClick()
        {
            CompleteMotion();

            menuScrollView.gameObject.SetActive(true);

#if LITMOTION_PRESENT
            motionHandle = LMotion.Create(-menuWidth, 0, 0.2f)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPositionX(menuScrollView);
#else
            menuScrollView.anchoredPosition = new Vector2(0, menuScrollView.anchoredPosition.y);
#endif
        }

        private void OnCloseButtonClick()
        {
            CompleteMotion();

#if LITMOTION_PRESENT
            motionHandle = LMotion.Create(0, -menuWidth, 0.14f)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() => menuScrollView.gameObject.SetActive(false))
                .BindToAnchoredPositionX(menuScrollView);
#else
            menuScrollView.anchoredPosition = new Vector2(-menuWidth, menuScrollView.anchoredPosition.y);
            menuScrollView.gameObject.SetActive(false);
#endif
        }

        private void CompleteMotion()
        {
#if LITMOTION_PRESENT
            if (motionHandle.IsActive())
            {
                motionHandle.Complete();
            }
#endif
        }
    }
}

