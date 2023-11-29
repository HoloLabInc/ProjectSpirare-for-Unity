using UnityEngine;

namespace HoloLab.Spirare.Cesium3DMaps
{
    public class CesiumRectangleMapCreditDefault : AbstractCesiumRectangleMapCredit
    {
        [SerializeField]
        private Canvas canvas;

        private RectTransform canvasRectTransform;

        private void Awake()
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        }

        public override void SetWidth(float width)
        {
            var sizeDeltaX = width / canvasRectTransform.localScale.x;
            canvasRectTransform.sizeDelta = new Vector2(sizeDeltaX, canvasRectTransform.sizeDelta.y);
        }
    }
}
