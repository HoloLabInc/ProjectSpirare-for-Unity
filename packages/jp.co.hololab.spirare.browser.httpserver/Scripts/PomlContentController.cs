using HoloLab.UniWebServer;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare.Browser.HttpServer
{
    public class PomlContentController : MonoBehaviour, IHttpController
    {
        [SerializeField]
        private PomlContentsManager pomlContentManager = null;

        [SerializeField]
        private TextAsset rootPageHtml = null;

        [SerializeField]
        private TextAsset loadFormHtml = null;

        [SerializeField]
        private TextAsset contentHtml = null;

        [Route("/")]
        public string RootPage()
        {
            var contentList = pomlContentManager.LoadedContentsList;

            var contentListHtml = "";
            if (contentList.Count > 0)
            {
                contentListHtml = contentList
                    .Select(CreateContentHtml)
                    .Aggregate((a, b) => $"{a}\n{b}");
            }

            var scriptHtml = "";

            var args = new string[]
            {
                loadFormHtml.text,
                contentListHtml,
                scriptHtml
            };
            var html = string.Format(rootPageHtml.text, args);
            return html;
        }

        [Route("/load")]
        public async Task LoadContent(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod == "POST")
            {
                var reader = new StreamReader(request.InputStream);
                var body = await reader.ReadToEndAsync();

                var queries = HttpQueryParser.ParseQueryString(body);

                if (queries.TryGetValue("url", out var url))
                {
                    _ = pomlContentManager.LoadContentsAsync(url);
                    Debug.Log($"Load POML: {url}");
                }
            }

            response.Redirect("/");
        }

        [Route("/content/:id/reload")]
        public void ReloadContent(string id, HttpListenerResponse response)
        {
            _ = pomlContentManager.ReloadContentAsync(id);

            response.Redirect("/");
        }

        [Route("/content/:id/remove")]
        public void RemoveContent(string id, HttpListenerResponse response)
        {
            pomlContentManager.RemoveContent(id);

            response.Redirect("/");
        }

        private string CreateContentHtml(LoadedContentInfo contentInfo)
        {
            var args = new string[]
            {
                contentInfo.Id,
                contentInfo.Url
            };
            var html = string.Format(contentHtml.text, args);
            return html;
        }
    }
}
