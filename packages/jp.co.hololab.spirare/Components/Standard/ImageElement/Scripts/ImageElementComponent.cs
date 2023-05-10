using UnityEngine;
using Cysharp.Threading.Tasks;
using ExifLibrary;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace HoloLab.Spirare
{
    public sealed class ImageElementComponent : SpecificObjectElementComponentBase<PomlImageElement>, IWithinCamera
    {
        [SerializeField]
        private GameObject imagePlane = null;

        private new Renderer renderer;
        private new Collider collider;
        private CameraVisibleHelper _cameraVisibleHelper;

        public override void Initialize(PomlImageElement element, PomlLoadOptions loadOptions)
        {
            base.Initialize(element, loadOptions);
            _cameraVisibleHelper = imagePlane.AddComponent<CameraVisibleHelper>();

            // Hide until the image is fully loaded.
            renderer = imagePlane.GetComponent<Renderer>();
            collider = imagePlane.GetComponent<Collider>();
            ChangeEnabled(false, false);
        }

        public bool IsWithinCamera(Camera camera)
        {
            return _cameraVisibleHelper.IsInsideCameraBounds(camera);
        }

        protected override async Task UpdateGameObjectCore()
        {
            ChangeLayer(Layer);

            ChangeEnabled(false, false);

            if (DisplayType == PomlDisplayType.None)
            {
                return;
            }

            if (DisplayType == PomlDisplayType.Occlusion)
            {
                return;
            }

            var src = element.Src;
            var extension = element.GetSrcFileExtension();

            Texture2D texture = null;

            if (extension == ".gif")
            {
                var result = await SpirareHttpClient.Instance.GetByteArrayAsync(src, enableCache: true);
                if (result.Success)
                {
#if MGGIF_PRESENT
                    if (gameObject.TryGetComponent<ImageElementGifPlayer>(out var gifPlayer) == false)
                    {
                        gifPlayer = gameObject.AddComponent<ImageElementGifPlayer>();
                        gifPlayer.Initialize(renderer);
                    }

                    await gifPlayer.LoadAsync(result.Data);
                    texture = gifPlayer.TextureList.FirstOrDefault();
#endif
                }
            }
            else
            {
                texture = await GetTexture(src);
            }

            if (texture == null)
            {
                return;
            }

            renderer.material.mainTexture = texture;
            var width = element.Width;
            var height = element.Height;
            if (width == 0 && height == 0)
            {
                width = 1f;
            }
            var aspect = (float)texture.width / texture.height;
            if (width == 0)
            {
                width = height * aspect;
            }
            else if (height == 0)
            {
                height = width / aspect;
            }
            imagePlane.transform.localScale = new Vector3(width, height, 1f);

            ChangeEnabled(true, true);
        }

        private void ChangeLayer(int layer)
        {
            gameObject.layer = layer;
            imagePlane.layer = layer;
        }

        private void ChangeEnabled(bool rendererEnabled, bool colliderEnabled)
        {
            renderer.enabled = rendererEnabled;
            collider.enabled = colliderEnabled;
        }

        private static async UniTask<Texture2D> GetTexture(string url)
        {
            var result = await SpirareHttpClient.Instance.GetByteArrayAsync(url, enableCache: true);
            if (result.Success == false)
            {
                return null;
            }

            var texture = new Texture2D(0, 0);
            texture.LoadImage(result.Data);

            using (var stream = new MemoryStream(result.Data))
            {
                var imageFile = await ImageFile.FromStreamAsync(stream);
                var orientation = imageFile.Properties.Get<ExifEnumProperty<Orientation>>(ExifTag.Orientation);
                if (orientation == null)
                {
                    return texture;
                }

                await ApplyOrientation(texture, orientation);
            }

            return texture;
        }

        private static async UniTask ApplyOrientation(Texture2D texture2D, Orientation orientation)
        {
            if (orientation == Orientation.Normal)
            {
                return;
            }

            var newPixels = await GetOrientedPixels32(texture2D, orientation);

            // Swap the size of width and height.
            switch (orientation)
            {
                case Orientation.FlippedAndRotatedLeft:
                case Orientation.FlippedAndRotatedRight:
                case Orientation.RotatedLeft:
                case Orientation.RotatedRight:
                    var width = texture2D.width;
                    var height = texture2D.height;
#if UNITY_2021_2_OR_NEWER
                    texture2D.Reinitialize(height, width);
#else
                    texture2D.Resize(height, width);
#endif
                    break;
            }

            texture2D.SetPixels32(newPixels);
            texture2D.Apply();
        }

        private static async UniTask<Color32[]> GetOrientedPixels32(Texture2D texture, Orientation orientation)
        {
            var width = texture.width;
            var height = texture.height;
            var pixels = texture.GetPixels32();

            switch (orientation)
            {
                case Orientation.Normal:
                    return pixels;
                case Orientation.Flipped:
                    return await GetMappedPixels32(pixels, width, height, orig => (orig.Width - orig.X - 1) + orig.Y * orig.Width);
                case Orientation.Rotated180:
                    return await GetMappedPixels32(pixels, width, height, orig => (orig.Width - orig.X - 1) + (orig.Height - orig.Y - 1) * orig.Width);
                case Orientation.FlippedAndRotated180:
                    return await GetMappedPixels32(pixels, width, height, orig => orig.X + (orig.Height - orig.Y - 1) * orig.Width);
                case Orientation.FlippedAndRotatedLeft:
                    return await GetMappedPixels32(pixels, width, height, orig => (orig.Height - orig.Y - 1) + (orig.Width - orig.X - 1) * orig.Height);
                case Orientation.RotatedLeft:
                    return await GetMappedPixels32(pixels, width, height, orig => orig.Y + (orig.Width - orig.X - 1) * orig.Height);
                case Orientation.FlippedAndRotatedRight:
                    return await GetMappedPixels32(pixels, width, height, orig => orig.Y + orig.X * orig.Height);
                case Orientation.RotatedRight:
                    return await GetMappedPixels32(pixels, width, height, orig => (orig.Height - orig.Y - 1) + orig.X * orig.Height);
                default:
                    return pixels;
            }
        }

        private static async UniTask<Color32[]> GetMappedPixels32(Color32[] pixels, int width, int height, Func<(int X, int Y, int Width, int Height), int> pixelMapping)
        {
            var newPixels = await Task.Run(() =>
            {
                var newPixels = new Color32[pixels.Length];
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var index = pixelMapping((x, y, width, height));
                        newPixels[index] = pixels[x + y * width];
                    }
                }
                return newPixels;
            });
            return newPixels;
        }
    }
}
