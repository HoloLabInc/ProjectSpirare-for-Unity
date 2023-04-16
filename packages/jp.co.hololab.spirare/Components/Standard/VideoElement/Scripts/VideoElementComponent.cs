using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HoloLab.Spirare
{
    public sealed class VideoElementComponent : SpecificObjectElementComponentBase<PomlVideoElement>
    {
        [SerializeField]
        private GameObject videoPlane = null;

        [SerializeField]
        private Canvas uiCanvas = null;

        [SerializeField]
        private Button playButton = null;

        private VideoPlayer videoPlayer;

        private Renderer videoPlaneRenderer;
        private Collider videoPlaneCollider;

        private static event Action<VideoElementComponent> onVideoPlay;

        private string cacheFolderPath => Path.Combine(Application.temporaryCachePath, "video");

        public override void Initialize(PomlVideoElement element, PomlLoadOptions loadOptions)
        {
            base.Initialize(element, loadOptions);

            videoPlaneRenderer = videoPlane.GetComponent<Renderer>();
            videoPlaneCollider = videoPlane.GetComponent<Collider>();
            videoPlayer = videoPlane.GetComponent<VideoPlayer>();

            // Hide until the video is loaded.
            SetVideoObjectVisibility(false);

            videoPlayer.errorReceived += VideoPlayer_ErrorReceived;
            videoPlayer.loopPointReached += _ =>
            {
                if (playButton != null)
                {
                    playButton.gameObject.SetActive(true);
                }
            };

            onVideoPlay += OnVideoPlay;

            var pomlElementComponent = GetComponent<PomlObjectElementComponent>();
            pomlElementComponent.OnSelect += OnSelect;
        }

        private void OnEnable()
        {
            // If it becomes inactive after calling Prepare, the Prepare process stops.
            // Therefore, call Prepare again in OnEnable.
            if (videoPlayer != null)
            {
                videoPlayer.Prepare();
            }
        }

        public void TogglePlay()
        {
            if (videoPlayer.isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Play()
        {
            if (playButton != null)
            {
                playButton.gameObject.SetActive(false);
            }
            videoPlayer.Play();
            onVideoPlay?.Invoke(this);
        }

        public void Pause()
        {
            videoPlayer.Pause();
            if (playButton != null)
            {
                playButton.gameObject.SetActive(true);
            }
        }

        protected override async Task UpdateGameObjectCore()
        {
            if (DisplayType == PomlDisplayType.None || DisplayType == PomlDisplayType.Occlusion)
            {
                Pause();
                SetVideoObjectVisibility(false);
                return;
            }

            var url = element.Src;
            if (string.IsNullOrEmpty(url))
            {
                Pause();
                SetVideoObjectVisibility(false);
                return;
            }

            var urlChanged = videoPlayer.url != url;
            videoPlayer.url = url;

            if (urlChanged)
            {
                videoPlayer.Prepare();
                await UniTask.WaitUntil(() => videoPlayer.isPrepared);
            }

            UpdateDisplaySize();

            if (urlChanged)
            {
                await ShowThumbnail();
            }

            SetVideoObjectVisibility(true);
        }

        private void UpdateDisplaySize()
        {
            var width = element.Width;
            var height = element.Height;
            if (width == 0 && height == 0)
            {
                width = 1f;
            }
            var aspect = (float)videoPlayer.width / videoPlayer.height;
            if (width == 0)
            {
                width = height * aspect;
            }
            else if (height == 0)
            {
                height = width / aspect;
            }
            videoPlane.transform.localScale = new Vector3(width, height, 1f);

            var uiSize = Mathf.Min(width, height);
            uiCanvas.transform.localScale = new Vector3(uiSize, uiSize, 1);
        }

        private void SetVideoObjectVisibility(bool active)
        {
            videoPlaneRenderer.enabled = active;
            videoPlaneCollider.enabled = active;
            uiCanvas.gameObject.SetActive(active);
        }

        private void OnSelect()
        {
            TogglePlay();
        }

        private void OnVideoPlay(VideoElementComponent element)
        {
            // Stop playing when another video starts playing.
            if (element != this)
            {
                if (videoPlayer.isPlaying)
                {
                    Pause();
                }
            }
        }

        private async void VideoPlayer_ErrorReceived(VideoPlayer source, string message)
        {
            Debug.LogError($"VideoPlayer error: {message}");

            // If the loading fails, download the video file and play it.
            if (videoPlayer.isPrepared == false)
            {
                await DownloadAndPrepare();
            }
        }

        private async UniTask ShowThumbnail()
        {
            var completionSource = new UniTaskCompletionSource();

            videoPlayer.sendFrameReadyEvents = true;
            VideoPlayer.FrameReadyEventHandler frameReadyEventHandler = (source, index) =>
            {
                completionSource.TrySetResult();
            };

            videoPlayer.frameReady += frameReadyEventHandler;

            videoPlayer.frame = 0;
            videoPlayer.Play();

            // Wait for the first frame to load.
            await completionSource.Task;

            videoPlayer.sendFrameReadyEvents = false;
            videoPlayer.frameReady -= frameReadyEventHandler;

            videoPlayer.Pause();
        }

        private async Task DownloadAndPrepare()
        {
            var src = element.Src;

            var extension = element.GetSrcFileExtension();
            var filename = GetCacheFilename(src, extension);
            var savePath = Path.Combine(cacheFolderPath, filename);

            using (var request = UnityWebRequest.Get(src))
            {
                request.downloadHandler = new DownloadHandlerFile(savePath);
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    videoPlayer.url = savePath;
                    videoPlayer.Prepare();
                }
                else
                {
                    Debug.LogError(request.error);
                }
            }
        }

        private string GetCacheFilename(string url, string extension)
        {
            var urlBytes = Encoding.UTF8.GetBytes(url);

            using (var provider = MD5.Create())
            {
                var hash = provider.ComputeHash(urlBytes);
                var hashString = BitConverter.ToString(hash).Replace("-", string.Empty);
                return hashString + extension;
            }
        }
    }
}
