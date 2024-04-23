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
        private RectTransform menuContent;

        [SerializeField]
        private Button closeButton;

        private float menuWidth;
        private MotionHandle motionHandle;

        private void Start()
        {
            menuWidth = menuContent.rect.width;

            motionHandle = LMotion.Create(-menuWidth, -menuWidth, 0.01f)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() => menuContent.gameObject.SetActive(false))
                .BindToAnchoredPositionX(menuContent);

            openButton.onClick.AddListener(OnOpenButtonClick);
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void OnOpenButtonClick()
        {
            CompleteMotion();

            menuContent.gameObject.SetActive(true);

            motionHandle = LMotion.Create(-menuWidth, 0, 0.2f)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPositionX(menuContent);
        }

        private void OnCloseButtonClick()
        {
            CompleteMotion();

            motionHandle = LMotion.Create(0, -menuWidth, 0.14f)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() => menuContent.gameObject.SetActive(false))
                .BindToAnchoredPositionX(menuContent);
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

