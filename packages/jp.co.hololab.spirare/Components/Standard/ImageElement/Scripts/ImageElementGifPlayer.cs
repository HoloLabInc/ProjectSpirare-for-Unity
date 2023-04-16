using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace HoloLab.Spirare
{
    public sealed class ImageElementGifPlayer : MonoBehaviour
    {
#if MGGIF_PRESENT
        public List<Texture2D> TextureList = new List<Texture2D>();
        public List<float> FrameSwitchTimeList = new List<float>();

        private Renderer targetRenderer;

        private bool loaded;

        private int currentFrameIndex = 0;
        private float playTime;

        public void Initialize(Renderer targetRenderer)
        {
            this.targetRenderer = targetRenderer;
        }

        public async Task LoadAsync(byte[] data, int delayMilliseconds = 20)
        {
            using (var decoder = new MG.GIF.Decoder(data))
            {
                var delaySum = 0;

                while (true)
                {
                    var image = decoder.NextImage();
                    if (image == null)
                    {
                        break;
                    }

                    var texture = image.CreateTexture();
                    delaySum += image.Delay;

                    TextureList.Add(texture);
                    FrameSwitchTimeList.Add(delaySum / 1000f);

                    await UniTask.Delay(delayMilliseconds);
                }

                loaded = true;
            }
        }

        private void Update()
        {
            if (loaded == false)
            {
                return;
            }

            playTime += Time.deltaTime;

            if (playTime < FrameSwitchTimeList[currentFrameIndex])
            {
                return;
            }

            // Find the frame to display.
            while (true)
            {
                currentFrameIndex += 1;
                if (currentFrameIndex >= TextureList.Count)
                {
                    currentFrameIndex = 0;
                    playTime = 0;
                    break;
                }

                var frameSwitchTime = FrameSwitchTimeList[currentFrameIndex];

                if (playTime < frameSwitchTime)
                {
                    break;
                }
            }

            targetRenderer.material.mainTexture = TextureList[currentFrameIndex];
        }
#endif
    }
}
