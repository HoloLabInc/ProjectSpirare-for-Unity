using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;

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

        private MotionHandle motionHandle;

        private Rect previousSafeArea;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            defaultMenuWidth = menuScrollView.rect.width;

            motionHandle = LMotion.Create(0f, 0f, 0.01f)
                .BindToAnchoredPositionX(menuScrollView);

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
            if (motionHandle.IsActive())
            {
                return;
            }

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

            motionHandle = LMotion.Create(-menuWidth, 0, 0.2f)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPositionX(menuScrollView);
        }

        private void OnCloseButtonClick()
        {
            CompleteMotion();

            motionHandle = LMotion.Create(0, -menuWidth, 0.14f)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() => menuScrollView.gameObject.SetActive(false))
                .BindToAnchoredPositionX(menuScrollView);
        }

        private void CompleteMotion()
        {
            if (motionHandle.IsActive())
            {
                motionHandle.Complete();
            }
        }
    }
}

