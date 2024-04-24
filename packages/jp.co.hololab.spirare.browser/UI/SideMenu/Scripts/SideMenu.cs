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
        private RectTransform menuContentRoot;

        [SerializeField]
        private Button closeButton;

        private RectTransform rectTransform;

        private float defaultMenuWidth;
        private float menuWidth;

        private MotionHandle motionHandle;

        private void Start()
        {
            defaultMenuWidth = menuScrollView.rect.width;

            var safeArea = Screen.safeArea;
            var scaleX = transform.lossyScale.x;
            var scaleY = transform.lossyScale.y;
            var safeOffsetX = safeArea.x / scaleX;
            var safeOffsetY = safeArea.y / scaleY;

            menuWidth = defaultMenuWidth + safeOffsetX;

            rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(menuWidth, 0);

            menuContentRoot.anchoredPosition = new Vector2(0, -safeOffsetY);

            motionHandle = LMotion.Create(0f, 0f, 0.01f)
                .BindToAnchoredPositionX(menuScrollView);

            openButton.onClick.AddListener(OnOpenButtonClick);
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void Update()
        {
            if (motionHandle.IsActive() == false && menuScrollView.gameObject.activeSelf)
            {
                for (var i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began && IsOutsideTouched(touch))
                    {
                        OnCloseButtonClick();
                        break;
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

