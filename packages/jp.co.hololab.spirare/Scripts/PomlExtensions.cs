using System;
using System.IO;

namespace HoloLab.Spirare
{
    public static class PomlExtensions
    {
        public static string GetSrcFileExtension(this PomlElement pomlElement)
        {
            // If filename is specified, get the extension from filename.
            var extension = pomlElement.GetFilenameExtension();

            // If there is no extension from the filename, get the extension from src.
            if (extension == null)
            {
                extension = pomlElement.GetSrcExtension();
            }

            // Convert to lowercase.
            if (extension != null)
            {
                extension = extension.ToLower();
            }
            return extension;
        }

        private static string GetFilenameExtension(this PomlElement pomlElement)
        {
            var filename = pomlElement.Filename;
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }

            var extension = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(extension))
            {
                return null;
            }

            return extension;
        }

        private static string GetSrcExtension(this PomlElement pomlElement)
        {
            var src = pomlElement.Src;
            try
            {
                var url = new Uri(src);
                var localPath = url.LocalPath;
                var extension = Path.GetExtension(localPath);
                return extension;
            }
            catch (Exception)
            {
                var extension = Path.GetExtension(src);
                return extension;
            }
        }
    }
}
