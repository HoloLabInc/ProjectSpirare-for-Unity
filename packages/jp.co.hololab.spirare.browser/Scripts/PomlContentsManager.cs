using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    public class PomlContentsManager : MonoBehaviour
    {
        [SerializeField]
        private PomlLoaderSettings pomlLoaderSettings;

        [SerializeField]
        private PomlLoadOptions.DisplayModeType displayModeInEditor = PomlLoadOptions.DisplayModeType.Normal;

        [SerializeField]
        private PomlLoadOptions.DisplayModeType displayModeInBuild = PomlLoadOptions.DisplayModeType.Normal;

        /// <summary>
        /// Load previous content on startup
        /// </summary>
        [SerializeField]
        private bool loadPreviousContentOnStartup = false;

        private List<InternalLoadedContentInfo> loadedContentsList
            = new List<InternalLoadedContentInfo>();

        public IList<LoadedContentInfo> LoadedContentsList
        {
            get
            {
                return loadedContentsList.Select(x => x.ToLoadedContentInfo())
                    .ToList();
            }
        }

        public event Action<string> OnStartLoadContent;
        public event Action<PomlClientBase> OnPomlClientCreated;

        private const string loadedContentSaveKey = "PomlContentsManager_LoadedContent";

        private async void Start()
        {
            if (loadPreviousContentOnStartup)
            {
                await LoadContentList();
            }
        }

        /// <summary>
        /// Load local content
        /// </summary>
        /// <returns></returns>
        public async Task LoadLocalContentAsync(string filepath)
        {
            var (localPomlClient, id) = CreateLocalPomlClient(filepath);
            InvokePomlClientCreated(localPomlClient);

            // TODO: Add to content list for managing content visibility

            // Load content
            await localPomlClient.LoadAsync(filepath);
        }

        /// <summary>
        /// Load content
        /// </summary>
        /// <param name="url"></param>
        /// <param name="reloadIfAlreadyLoaded"></param>
        /// <returns></returns>
        public async Task<(bool Success, Exception Error)> LoadContentsAsync(string url, bool reloadIfAlreadyLoaded = true)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }

            OnStartLoadContent?.Invoke(url);

            // If reloadIfAlreadyLoaded is true, reload the URL if it's already loaded
            if (reloadIfAlreadyLoaded)
            {
                var loadedContents = loadedContentsList.FirstOrDefault(x => x.Url == url);
                if (loadedContents != null)
                {
                    return await ReloadContentAsync(loadedContents.Id, hardRefresh: true);
                }
            }

            var (webPomlClient, id) = CreatePomlClient(url);
            InvokePomlClientCreated(webPomlClient);

            // Save to file
            SaveContentList();

            // Load content
            return await webPomlClient.LoadAsync(url);
        }


        /// <summary>
        /// Reload content
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hardRefresh"></param>
        /// <returns></returns>
        public async Task<(bool Success, Exception Error)> ReloadContentAsync(string id, bool hardRefresh = false)
        {
            if (!TryGetLoadedContent(id, out var contentInfo))
            {
                return (false, new InvalidOperationException("Content is not loaded"));
            }

            var webPomlClient = contentInfo.WebPomlClient;
            return await webPomlClient.ReloadAsync(hardRefresh);
        }

        /// <summary>
        /// Remove loaded content
        /// </summary>
        /// <param name="id"></param>
        public void RemoveContent(string id)
        {
            if (!TryGetLoadedContent(id, out var contentInfo))
            {
                return;
            }

            loadedContentsList.RemoveAll(x => x.Id == id);
            Destroy(contentInfo.WebPomlClient.gameObject);

            SaveContentList();
        }

        public void SetAutoReload(string id, bool autoReload, float autoReloadInterval)
        {
            SetAutoReloadCore(id, autoReload, autoReloadInterval);

            // Save to file
            SaveContentList();
        }

        private void InvokePomlClientCreated(PomlClientBase pomlClient)
        {
            try
            {
                OnPomlClientCreated?.Invoke(pomlClient);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Create a LocalPomlClient
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private (LocalPomlClient LocalPomlClient, string ContentId) CreateLocalPomlClient(string filepath)
        {
            var go = new GameObject("LocalPomlContent");
            go.transform.SetParent(transform, false);

            var localPomlClient = go.AddComponent<LocalPomlClient>();
            SetUpPomlClientBase(localPomlClient);

            var contentId = Guid.NewGuid().ToString();

            // TODO Implement UI for managing local content

            return (localPomlClient, contentId);
        }

        /// <summary>
        /// Create a PomlClient
        /// Loading is not performed yet
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private (WebPomlClient WebPomlClient, string ContentId) CreatePomlClient(string url)
        {
            var go = new GameObject("PomlContent");
            go.transform.SetParent(transform, false);

            var webPomlClient = go.AddComponent<WebPomlClient>();
            SetUpPomlClientBase(webPomlClient);

            var contentId = Guid.NewGuid().ToString();
            var internalLoadedContentInfo = new InternalLoadedContentInfo()
            {
                Id = contentId,
                Url = url,
                WebPomlClient = webPomlClient
            };

            loadedContentsList.Add(internalLoadedContentInfo);

            return (webPomlClient, contentId);
        }

        private void SetUpPomlClientBase(PomlClientBase pomlClientBase)
        {
            pomlClientBase.PomlLoaderSettings = pomlLoaderSettings;
            pomlClientBase.DisplayModeInEditor = displayModeInEditor;
            pomlClientBase.DisplayModeInBuild = displayModeInBuild;
        }

        private void SetAutoReloadCore(string id, bool autoReload, float autoReloadInterval)
        {
            var loadedContents = loadedContentsList.FirstOrDefault(x => x.Id == id);
            if (loadedContents == null)
            {
                return;
            }

            var webPomlClient = loadedContents.WebPomlClient;
            webPomlClient.AutoReloadInterval = autoReloadInterval;
            webPomlClient.AutoReload = autoReload;

            loadedContents.AutoReload = autoReload;
            loadedContents.AutoReloadInterval = autoReloadInterval;
        }


        private void SaveContentList()
        {
            var json = LoadedContentSerializer.Serialize(LoadedContentsList);

            PlayerPrefs.SetString(loadedContentSaveKey, json);
            PlayerPrefs.Save();
        }

        private async Task LoadContentList()
        {
            var json = PlayerPrefs.GetString(loadedContentSaveKey, "");
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            if (LoadedContentSerializer.TryDeserialize(json, out var serializableLoadedContents))
            {
                var clientList = new List<(WebPomlClient WebPomlClient, string ContentId, SerializableLoadedContent Content)>();
                foreach (var content in serializableLoadedContents)
                {
                    var client = CreatePomlClient(content.url);
                    InvokePomlClientCreated(client.WebPomlClient);

                    clientList.Add((client.WebPomlClient, client.ContentId, content));

                    SetAutoReloadCore(client.ContentId, content.autoReload, content.autoReloadInterval);
                }

                foreach (var client in clientList)
                {
                    await client.WebPomlClient.LoadAsync(client.Content.url);
                }
            }
        }

        private bool TryGetLoadedContent(string id, out InternalLoadedContentInfo internalLoadedContentInfo)
        {
            internalLoadedContentInfo = loadedContentsList.FirstOrDefault(x => x.Id == id);
            return internalLoadedContentInfo != null;
        }
    }
}
