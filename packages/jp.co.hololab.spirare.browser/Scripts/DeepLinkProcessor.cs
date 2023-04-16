using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    [RequireComponent(typeof(PomlContentsManager))]
    public class DeepLinkProcessor : MonoBehaviour
    {
        private PomlContentsManager pomlContentsManager;

        private void Awake()
        {
            pomlContentsManager = GetComponent<PomlContentsManager>();
        }

        private void Start()
        {
            Application.deepLinkActivated += OnDeepLinkActivated;

            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                OnDeepLinkActivated(Application.absoluteURL);
            }
        }

        private async void OnDeepLinkActivated(string url)
        {
            // The example of URL link
            // spirare:http://samplecontent

            var colonIndex = url.IndexOf(':');
            var link = url.Substring(colonIndex + 1);

            await pomlContentsManager.LoadContentsAsync(link);
        }
    }
}
