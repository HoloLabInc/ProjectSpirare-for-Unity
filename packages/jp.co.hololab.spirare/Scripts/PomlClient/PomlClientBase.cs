using System;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoloLab.Spirare
{
    public abstract class PomlClientBase : MonoBehaviour
    {
        [SerializeField]
        protected PomlLoaderSettings pomlLoaderSettings;

        public PomlLoaderSettings PomlLoaderSettings
        {
            get => pomlLoaderSettings;
            set => pomlLoaderSettings = value;
        }

        [SerializeField]
        private PomlLoadOptions.DisplayModeType displayModeInEditor = PomlLoadOptions.DisplayModeType.Normal;

        public PomlLoadOptions.DisplayModeType DisplayModeInEditor
        {
            get => displayModeInEditor;
            set => displayModeInEditor = value;
        }

        [SerializeField]
        private PomlLoadOptions.DisplayModeType displayModeInBuild = PomlLoadOptions.DisplayModeType.Normal;

        public PomlLoadOptions.DisplayModeType DisplayModeInBuild
        {
            get => displayModeInBuild;
            set => displayModeInBuild = value;
        }

        [SerializeField]
        private bool autoReload;

        public bool AutoReload
        {
            get => autoReload;
            set
            {
                if (autoReload != value && isLoaded)
                {
                    if (value)
                    {
                        StartAutoReload();
                    }
                    else
                    {
                        StopAutoReload();
                    }
                }
                autoReload = value;
            }
        }

        [SerializeField]
        [Tooltip("Interval seconds for performing automatic reloading")]
        private float autoReloadInterval = 5;

        public float AutoReloadInterval
        {
            get => autoReloadInterval;
            set => autoReloadInterval = value;
        }

        private string pomlPath;
        private string latestPoml;
        private GameObject latestPomlObject;

        private bool isLoading;
        private bool isLoaded;

        private CancellationTokenSource autoReloadCancellationTokenSource;

        private const string defaultPomlLoaderSettingsGuid = "c84a74f2491edfd42ad508c023562b01";

        public event Action<PomlComponent> OnLoaded;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (pomlLoaderSettings == null)
            {
                var defaultPomlLoaderSettingsPath = AssetDatabase.GUIDToAssetPath(defaultPomlLoaderSettingsGuid);
                pomlLoaderSettings = AssetDatabase.LoadAssetAtPath(defaultPomlLoaderSettingsPath, typeof(PomlLoaderSettings)) as PomlLoaderSettings;
            }
        }
#endif

        private void OnDestroy()
        {
            StopAutoReload();
        }

        public async Task<(bool Success, Exception Error)> LoadAsync(string path)
        {
            if (isLoading)
            {
                return (false, new InvalidOperationException("Already loading"));
            }

            if (isLoaded)
            {
                Debug.LogError("The instance is already loaded.");
                return (false, new InvalidOperationException("Already loaded"));
            }

            try
            {
                isLoading = true;

                pomlPath = path;
                var xml = await GetContentXml(path);

                await LoadPomlAsync(xml, path);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return (false, ex);
            }
            finally
            {
                isLoading = false;
                isLoaded = true;

                if (autoReload)
                {
                    StartAutoReload();
                }
            }
            return (true, null);
        }

        public async Task<(bool Success, Exception Error)> ReloadAsync(bool hardRefresh = false, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return (false, new OperationCanceledException(cancellationToken));
            }

            if (isLoading)
            {
                return (false, new InvalidOperationException("Already loading"));
            }

            if (string.IsNullOrEmpty(pomlPath))
            {
                return (false, new InvalidOperationException("Poml is not specified"));
            }

            try
            {
                isLoading = true;

                var xml = await GetContentXml(pomlPath);

                if (hardRefresh == true || xml != latestPoml)
                {
                    RemoveLoadedContent();

                    await LoadPomlAsync(xml, pomlPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return (false, ex);
            }
            finally
            {
                isLoading = false;

                if (autoReload)
                {
                    ReloadAutomatically(cancellationToken).Forget();
                }
            }
            return (true, null);
        }

        private void StartAutoReload()
        {
            StopAutoReload();

            autoReloadCancellationTokenSource = new CancellationTokenSource();
            ReloadAutomatically(autoReloadCancellationTokenSource.Token).Forget();
        }

        private void StopAutoReload()
        {
            autoReloadCancellationTokenSource?.Cancel();
        }

        private async UniTask LoadPomlAsync(string poml, string url)
        {
            // For cache
            latestPoml = poml;

            var displayMode = Application.isEditor ? displayModeInEditor : displayModeInBuild;
            var pomlLoader = new PomlLoader(pomlLoaderSettings, displayMode);
            latestPomlObject = new GameObject("Poml Object");
            latestPomlObject.transform.SetParent(transform, false);

            var pomlComponent = await pomlLoader.LoadXmlAsync(poml, url, latestPomlObject);

            try
            {
                OnLoaded?.Invoke(pomlComponent);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Perform automatic reloading
        /// </summary>
        /// <returns></returns>
        private async UniTask ReloadAutomatically(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Reload at least every 1 second
            autoReloadInterval = Math.Max(1, autoReloadInterval);
            await UniTask.Delay((int)(autoReloadInterval * 1000), cancellationToken: cancellationToken);

            if (!Application.isPlaying)
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await ReloadAsync(cancellationToken: cancellationToken);
        }

        private void RemoveLoadedContent()
        {
            Destroy(latestPomlObject);
        }

        protected abstract Task<string> GetContentXml(string path);
    }
}
