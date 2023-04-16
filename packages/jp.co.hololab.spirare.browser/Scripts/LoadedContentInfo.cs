namespace HoloLab.Spirare.Browser
{
    public class LoadedContentInfo
    {
        public string Id;
        public string Url;

        public bool AutoReload;
        public float AutoReloadInterval;
    }

    internal class InternalLoadedContentInfo
    {
        public string Id;
        public string Url;
        public WebPomlClient WebPomlClient;

        public bool AutoReload;
        public float AutoReloadInterval;

        public LoadedContentInfo ToLoadedContentInfo()
        {
            return new LoadedContentInfo()
            {
                Id = Id,
                Url = Url,
                AutoReload = AutoReload,
                AutoReloadInterval = AutoReloadInterval
            };
        }
    }
}
