using HoloLab.Spirare.Browser;
using HoloLab.UniWebServer;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class PomlContentController : MonoBehaviour, IHttpController
{
    [SerializeField]
    private PomlContentsManager pomlContentManager = null;

    [SerializeField]
    private TextAsset rootPageHtml = null;

    [SerializeField]
    private TextAsset rootPageScriptHtml = null;

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

        var args = new string[]
        {
             loadFormHtml.text,
             contentListHtml,
             rootPageScriptHtml.text,
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

            var queries = ParseQueryString(body);

            var queryPair = queries.FirstOrDefault(x => x.Key == "url");
            if (queryPair != null)
            {
                _ = pomlContentManager.LoadContentsAsync(queryPair.Value);
                Debug.Log(queryPair.Value);
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

    [Route("/content/:id/auto-reload")]
    public async Task SetAutoReload(string id, HttpListenerRequest request, HttpListenerResponse response)
    {
        var autoReloadInterval = 5;

        if (request.HttpMethod == "POST")
        {
            var reader = new StreamReader(request.InputStream);
            var body = await reader.ReadToEndAsync();

            var queries = ParseQueryString(body);
            var queryPair = queries.FirstOrDefault(x => x.Key == "enabled");
            if (queryPair != null)
            {
                if (bool.TryParse(queryPair.Value, out var enabled))
                {
                    Debug.Log(enabled);
                    pomlContentManager.SetAutoReload(id, enabled, autoReloadInterval);
                }
            }
        }

        response.Redirect("/");
    }


    private string CreateContentHtml(LoadedContentInfo contentInfo)
    {
        var args = new string[]
        {
            contentInfo.Id,
            contentInfo.Url,
            contentInfo.AutoReload ? "checked" : ""
        };
        var html = string.Format(contentHtml.text, args);
        return html;
    }

    private class QueryPair
    {
        public string Key;
        public string Value;
    }

    private static List<QueryPair> ParseQueryString(string queryString)
    {
        var queryList = new List<QueryPair>();

        var queries = queryString.Split('&');
        foreach (var query in queries)
        {
            var tokens = query.Split('=');
            if (tokens.Length != 2)
            {
                continue;
            }

            var key = tokens[0];
            var value = tokens[1];

            var queryPair = new QueryPair()
            {
                Key = UnityWebRequest.UnEscapeURL(key),
                Value = UnityWebRequest.UnEscapeURL(value)
            };

            queryList.Add(queryPair);
        }

        return queryList;
    }
}
