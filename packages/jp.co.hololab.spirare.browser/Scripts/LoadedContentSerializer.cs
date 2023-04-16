using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloLab.Spirare.Browser
{
    internal static class LoadedContentSerializer
    {
        public static string Serialize(IEnumerable<LoadedContentInfo> contentInfoList)
        {
            var serializableContentList = new SerializableLoadedContentList()
            {
                content = contentInfoList
                    .Select(x => new SerializableLoadedContent(x))
                    .ToList()
            };

            return JsonUtility.ToJson(serializableContentList);
        }

        public static bool TryDeserialize(string json, out List<SerializableLoadedContent> contentList)
        {
            try
            {
                var list = JsonUtility.FromJson<SerializableLoadedContentList>(json);
                contentList = list.content;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                contentList = null;
                return false;
            }
        }
    }

    [Serializable]
    internal class SerializableLoadedContentList
    {
        public List<SerializableLoadedContent> content;
    }

    [Serializable]
    internal class SerializableLoadedContent
    {
        public string url;
        public bool autoReload;
        public float autoReloadInterval;

        public SerializableLoadedContent(LoadedContentInfo contentInfo)
        {
            url = contentInfo.Url;
            autoReload = contentInfo.AutoReload;
            autoReloadInterval = contentInfo.AutoReloadInterval;
        }
    }
}
