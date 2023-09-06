using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace HoloLab.Spirare.Browser.HttpServer
{
    public class HttpQueries
    {
        private readonly Dictionary<string, List<string>> queries;

        public Dictionary<string, List<string>> Queries => queries;

        public HttpQueries(Dictionary<string, List<string>> queries)
        {
            this.queries = queries;
        }

        public bool TryGetValues(string key, out List<string> values)
        {
            return queries.TryGetValue(key, out values);
        }

        public bool TryGetValue(string key, out string value)
        {
            if (queries.TryGetValue(key, out var values))
            {
                if (values.Count > 0)
                {
                    value = values[0];
                    return true;
                }
            }

            value = null;
            return false;
        }
    }

    public static class HttpQueryParser
    {
        private class QueryPair
        {
            public string Key;
            public string Value;
        }

        public static HttpQueries ParseQueryString(string queryString)
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

            var queryDictionary = queryList
                .GroupBy(x => x.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Value).ToList()
                );

            return new HttpQueries(queryDictionary);
        }
    }
}
