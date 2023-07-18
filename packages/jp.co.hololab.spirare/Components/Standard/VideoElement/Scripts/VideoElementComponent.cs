using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace HoloLab.Spirare
{
    public sealed class VideoElementComponent : SpecificObjectElementComponentBase<PomlVideoElement>, IWithinCamera
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
        private CameraVisibleHelper _cameraVisibleHelper;

        private static event Action<VideoElementComponent> onVideoPlay;

        public override void Initialize(PomlVideoElement element, PomlLoadOptions loadOptions)
        {
            base.Initialize(element, loadOptions);
            _cameraVisibleHelper = videoPlane.AddComponent<CameraVisibleHelper>();

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

        public bool IsWithinCamera(Camera camera)
        {
            return _cameraVisibleHelper.IsInsideCameraBounds(camera);
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
            var result = await SpirareHttpClient.Instance.DownloadToFileAsync(src, enableCache: true, extension: extension);
            if (result.Success)
            {
                var filepath = result.Data;
                var url = $"file://{filepath}";
                videoPlayer.url = url;
                videoPlayer.Prepare();
            }
            else
            {
                Debug.LogError(result.Error);
            }
        }
    }
}
