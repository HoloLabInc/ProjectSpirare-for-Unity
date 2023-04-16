using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace HoloLab.Spirare
{
    public sealed class TextElementComponent : SpecificObjectElementComponentBase<PomlTextElement>
    {
        [SerializeField]
        private TextMeshPro textMeshPro = null;

        [SerializeField]
        private GameObject backPlate;

        private RectTransform textRectTransform;
        private Renderer backPlateRenderer;

        // Text interpolation
        private Transform cameraTransform;
        private bool needTextUpdate;
        private CancellationTokenSource updateInterplatedTextLoopTokenSource;

        private const string REG_NUM = "^[0-9.]+";
        private const string REG_UNIT = "(pt|mm|m)$";

        // TMPro base size
        // Adjusted the value with the result
        private const float TMP_BASE_SIZE = 36 / 2.5f * 0.75f;

        // Default font size [m]
        private const float DEFAULT_FONT_SIZE = 1;

        public override void Initialize(PomlTextElement element, PomlLoadOptions loadOptions)
        {
            base.Initialize(element, loadOptions);

            cameraTransform = Camera.main.transform;

            textRectTransform = textMeshPro.GetComponent<RectTransform>();
            backPlateRenderer = backPlate.GetComponent<Renderer>();

            backPlate.transform.localScale = Vector3.zero;
        }

        private async void OnEnable()
        {
            var token = this.GetCancellationTokenOnDestroy();
            try
            {
                await UniTask.DelayFrame(2, cancellationToken: token);
                UpdateBackPlate();
            }
            catch (Exception) { }
        }

        protected override async Task UpdateGameObjectCore()
        {
            await LoadGameObjectAsync();
        }

        private void SetTextObjectActive(bool active)
        {
            textMeshPro.gameObject.SetActive(active);
            backPlate.SetActive(active);
        }

        private void ChangeLayer(int layer)
        {
            gameObject.layer = layer;
            textMeshPro.gameObject.layer = layer;
            backPlate.layer = layer;
        }

        private async Task LoadGameObjectAsync()
        {
            // Cancel latest update loop
            if (updateInterplatedTextLoopTokenSource != null)
            {
                updateInterplatedTextLoopTokenSource.Cancel();
                updateInterplatedTextLoopTokenSource = null;
            }

            ChangeLayer(Layer);

            if (DisplayType == PomlDisplayType.None || DisplayType == PomlDisplayType.Occlusion)
            {
                SetTextObjectActive(false);
                return;
            }

            // Font size
            var fontSize = element.FontSize;
            textMeshPro.fontSize = GetFontSizeForTextMeshPro(fontSize);

            // Background color
            var bc = element.BackgroundColor;
            if (bc.a > 0f)
            {
                var material = backPlateRenderer.material;
                material.color = element.BackgroundColor;
            }
            else
            {
                // To allow object selection, disable Renderer and keep Collider enabled
                backPlateRenderer.enabled = false;
            }

            // Font color
            textMeshPro.color = element.FontColor;

            // Text alignment
            textMeshPro.verticalAlignment = VerticalAlignmentOptions.Middle;
            textMeshPro.horizontalAlignment = GetHorizontalAlignmentOptions(element.TextAlign);

            // Text area size
            var textRectWidth = element.Width;
            var textRectHeight = element.Height;

            // If Width is specified, perform word wrapping for the text
            if (element.Width != 0f)
            {
                textMeshPro.enableWordWrapping = true;
            }

            // If Height is specified, hide characters that don't fit in the area
            if (element.Height != 0f)
            {
                textMeshPro.overflowMode = TextOverflowModes.Truncate;

                // If only Height is specified, set a sufficiently large width
                if (element.Width == 0f)
                {
                    textRectWidth = textRectHeight * 10000;
                }
            }

            textRectTransform.sizeDelta = new Vector2(textRectWidth, textRectHeight);

            try
            {
                SetTextObjectActive(true);

                var token = this.GetCancellationTokenOnDestroy();
                await UpdateTextAsync(token);

                updateInterplatedTextLoopTokenSource = new CancellationTokenSource();
                if (needTextUpdate)
                {
                    _ = UpdateInterpolatedTextLoop(updateInterplatedTextLoopTokenSource.Token);
                }
            }
            catch (OperationCanceledException) { }
        }

        private async Task UpdateInterpolatedTextLoop(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                await UniTask.Delay(1000, cancellationToken: token);
                await UpdateTextAsync(token);
            }
        }

        private async UniTask UpdateTextAsync(CancellationToken token = default)
        {
            var interpolatedText = InterpolateText(element.Text);
            textMeshPro.text = interpolatedText;

            // Check if text update is needed
            needTextUpdate = element.Text != interpolatedText;

            // Need to wait for 1 frame for bounds to be correctly obtained
            await UniTask.DelayFrame(1, cancellationToken: token);

            UpdateBackPlate();
        }

        private void UpdateBackPlate()
        {
            if (backPlateRenderer == false)
            {
                return;
            }

            if (gameObject.activeInHierarchy == false)
            {
                backPlate.transform.localScale = Vector3.zero;
                return;
            }

            var bounds = textMeshPro.bounds;

            var p = textMeshPro.fontSize * 0.03f;
            var padding = (Left: p, Right: p, Bottom: p, Top: p);

            // Calculate the drawing position
            var pos = bounds.center;
            var offsetX = -(padding.Left / 2) + (padding.Right / 2);
            var offsetY = -(padding.Bottom / 2) + (padding.Top / 2);
            var offsetZ = -0.01f;
            backPlate.transform.localPosition = new Vector3(pos.x + offsetX, pos.y + offsetY, pos.z + offsetZ);

            // Calculate the drawing size
            var scale = bounds.extents;
            var paddingWidth = (padding.Left + padding.Right);
            var paddingHeight = (padding.Top + padding.Bottom);

            var localScaleX = Mathf.Max(element.Width, scale.x * 2) + paddingWidth;
            var localScaleY = Mathf.Max(element.Height, scale.y * 2) + paddingHeight;
            backPlate.transform.localScale = new Vector3(localScaleX, localScaleY, 1);

            // Background border
            var minScale = Mathf.Min(localScaleX, localScaleY);
            var borderRatio = Mathf.Clamp01(element.BorderWidth * 2 / minScale);

            var material = backPlateRenderer.material;
            material.SetFloat("_BorderWidth", borderRatio);
        }

        private string InterpolateText(string originalText)
        {
            var formatString = originalText;
            var interpolationIndex = 0;
            var interpolationTexts = new List<object>();

            while (true)
            {
                if (formatString.Contains("{distance}"))
                {
                    formatString = formatString.Replace("{distance}", $"{{{interpolationIndex}}}");
                    interpolationIndex++;

                    var distance = Vector3.Distance(cameraTransform.position, transform.position);
                    interpolationTexts.Add(distance.ToString("f0"));
                }
                else
                {
                    break;
                }
            }

            return string.Format(formatString, interpolationTexts.ToArray());
        }

        internal string GetText()
        {
            return (textMeshPro != null) ? textMeshPro.text : "";
        }

        internal string GetFontSize()
        {
            return (element.FontSize != null) ? element.FontSize : "";
        }

        private static float GetFontSizeForTextMeshPro(string fontSize)
        {
            var fontSizeInMeters = GetFontSizeInMeters(fontSize);
            return fontSizeInMeters * TMP_BASE_SIZE;
        }

        private static float GetFontSizeInMeters(string fontSize)
        {
            // Retrieve the numeric part
            Regex numReg = new Regex(REG_NUM);
            Match numMatch = numReg.Match(fontSize);
            if (numMatch.Success == false)
            {
                return DEFAULT_FONT_SIZE;
            }
            var size = float.Parse(numMatch.Value);

            // Retrieve the unit part
            Regex unitReg = new Regex(REG_UNIT);
            Match unitMatch = unitReg.Match(fontSize);
            if (unitMatch.Success == false)
            {
                return DEFAULT_FONT_SIZE;
            }
            var unit = unitMatch.Value;

            // Calculate fontSize based on the unit
            switch (unit)
            {
                case "m":
                    return size;

                case "mm":
                    return size * 0.001f;

                case "pt":
                    // 1 inch (25.4mm) = 72 points
                    const float pointInMillimeters = 25.4f / 72;
                    return size * pointInMillimeters * 0.001f;

                default:
                    return DEFAULT_FONT_SIZE;
            }
        }

        private static HorizontalAlignmentOptions GetHorizontalAlignmentOptions(string textAlign)
        {
            switch (textAlign)
            {
                case "center":
                    return HorizontalAlignmentOptions.Center;
                case "right":
                    return HorizontalAlignmentOptions.Right;
                case "left":
                    return HorizontalAlignmentOptions.Left;
                default:
                    return HorizontalAlignmentOptions.Center;
            }
        }
    }
}
