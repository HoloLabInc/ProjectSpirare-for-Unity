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

        private void Start()
        {
            menuWidth = menuContent.rect.width;

            LMotion.Create(-menuWidth, -menuWidth, 0.01f)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() => menuContent.gameObject.SetActive(false))
                .BindToAnchoredPositionX(menuContent);

            openButton.onClick.AddListener(OnOpenButtonClick);
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void OnOpenButtonClick()
        {
            LMotion.Create(-menuWidth, 0, 0.2f)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPositionX(menuContent);
            menuContent.gameObject.SetActive(true);
        }

        private async void OnCloseButtonClick()
        {
            var handle = LMotion.Create(0, -menuWidth, 0.14f)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPositionX(menuContent);
            await handle;
            menuContent.gameObject.SetActive(false);
        }
    }
}

