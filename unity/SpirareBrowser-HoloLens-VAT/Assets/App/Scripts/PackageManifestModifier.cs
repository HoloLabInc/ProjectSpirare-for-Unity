#if UNITY_EDITOR

using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace HoloLab.Spirare.Browser.Vuforia
{
    /// <summary>
    /// Add the access settings for the document folder to the Package.appxmanifest generated after the build.
    /// </summary>
    class PackageManifestModifier
    {
        private static FileTypeAssociation[] associations = new FileTypeAssociation[]
        {
            new FileTypeAssociation()
            {
                Name = "vat",
                DisplayName = "VAT map data",
                FileTypes = new string[]
                {
                    "xml",
                    "dat",
                }
            }
        };

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuildProject)
        {
            if (associations.Length == 0)
            {
                return;
            }

            var manifestPath = Path.Combine(pathToBuildProject, Application.productName, "Package.appxmanifest");
            var manifest = File.ReadAllText(manifestPath);

            manifest = AddDocumentsFolderAccess(manifest);
            manifest = AddExtensions(manifest);

            foreach (var association in associations)
            {
                manifest = AddfileTypeAssociation(manifest, association);
            }

            File.WriteAllText(manifestPath, manifest);
        }


        /// <summary>
        /// Add the documentsLibrary capability.
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns></returns>
        private static string AddDocumentsFolderAccess(string manifest)
        {
            var documentsCapabilityXml = @"<uap:Capability Name=""documentsLibrary"" />";
            if (!Regex.IsMatch(manifest, documentsCapabilityXml))
            {
                var regex = new Regex(@"<Capabilities>");
                manifest = InsertAfter(manifest, regex, $"{Environment.NewLine}    {documentsCapabilityXml}");
            }

            return manifest;
        }

        /// <summary>
        /// Add the Extensions tag.
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns></returns>
        private static string AddExtensions(string manifest)
        {
            if (!Regex.IsMatch(manifest, @"<Extensions>"))
            {
                var regex = new Regex(@"</uap:VisualElements>");
                var extensionsXml =
@"
      <Extensions>
      </Extensions>";
                manifest = InsertAfter(manifest, regex, extensionsXml);
            }

            return manifest;
        }

        /// <summary>
        /// Add the FileTypeAssociation tag.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="association"></param>
        /// <returns></returns>
        private static string AddfileTypeAssociation(string manifest, FileTypeAssociation association)
        {
            if (!Regex.IsMatch(manifest, @"<uap:Extension Category=""windows.fileTypeAssociation"">"))
            {
                var regex = new Regex(@"<Extensions>");
                var extensionsXml =
@"
        <uap:Extension Category=""windows.fileTypeAssociation"">
        </uap:Extension>";
                manifest = InsertAfter(manifest, regex, extensionsXml);
            }

            if (!Regex.IsMatch(manifest, $@"<uap:FileTypeAssociation Name=""{association.Name}"">"))
            {
                var regex = new Regex(@"<uap:Extension Category=""windows.fileTypeAssociation"">(.*)\n\s*</uap:Extension>", RegexOptions.Singleline);
                var extensionsXml =
$@"          <uap:FileTypeAssociation Name=""{association.Name}"">
            <uap:SupportedFileTypes>";

                // Specify file extensions.
                foreach(var f in association.FileTypes)
                {
                    var filetype = f;
                    if (!filetype.StartsWith("."))
                    {
                        filetype = "." + filetype;
                    }

                    extensionsXml += $@"
              <uap:FileType>{filetype}</uap:FileType>";
                }

                extensionsXml += 
$@"
            </uap:SupportedFileTypes>
            <uap:DisplayName>{association.DisplayName}</uap:DisplayName>
          </uap:FileTypeAssociation>";
                manifest = InsertAfter(manifest, regex, extensionsXml);
            }

            return manifest;
        }



        private static string InsertAfter(string text, Regex regex, string insertText)
        {
            var match = regex.Match(text);
            if (match.Success)
            {
                Group group;
                if (match.Groups.Count == 1)
                {
                    group = match.Groups[0];
                }
                else
                {
                    group = match.Groups[1];
                }

                var index = group.Index + group.Length;
                text = text.Insert(index, insertText);
            }

            return text;
        }

        private class FileTypeAssociation
        {
            /// <summary>
            /// Only characters [-_.a-z0-9] can be used.
            /// </summary>
            internal string Name = "";
            internal string DisplayName = "";

            /// <summary>
            /// File extensions
            /// </summary>
            internal string[] FileTypes = new string[0];
        }
    }
}
#endif
