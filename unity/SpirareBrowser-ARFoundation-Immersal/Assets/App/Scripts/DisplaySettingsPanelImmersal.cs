using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.Immersal
{
    public class DisplaySettingsPanelImmersal : MonoBehaviour
    {
        [SerializeField]
        private RectTransform logoTransform;

        private RectTransform rectTransform;

        private Vector2 initialAnchoredPosition;
        private int previousScreenWidth;
        private int previousScreenHeight;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            initialAnchoredPosition = rectTransform.anchoredPosition;
        }

        private void Update()
        {
            var width = Screen.width;
            var height = Screen.height;

            if (width == previousScreenWidth && height == previousScreenHeight)
            {
                return;
            }

            var panelCorners = new Vector3[4];
            rectTransform.GetWorldCorners(panelCorners);

            var logoCorners = new Vector3[4];
            logoTransform.GetWorldCorners(logoCorners);

            var offset = 0f;
            if (panelCorners[0].x < logoCorners[2].x)
            {
                offset = logoCorners[2].y / transform.lossyScale.y;
            }

            rectTransform.anchoredPosition = new Vector2(initialAnchoredPosition.x, initialAnchoredPosition.y + offset);

            previousScreenWidth = width;
            previousScreenHeight = height;
        }
    }
}

