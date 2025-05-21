using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace HoloLab.Spirare.Quest
{
    public class LicenseText : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text licenseText;

        [SerializeField]
        private TextAsset licenseTextAsset;

        private void Awake()
        {
            InitializeText();
        }

        private void InitializeText()
        {
            var chunks = SplitTextIntoChunks(licenseTextAsset.text, 100);

            foreach (var chunk in chunks)
            {
                var chunkText = Instantiate(licenseText, licenseText.transform.parent);
                chunkText.text = chunk;
            }

            Destroy(licenseText.gameObject);
        }

        private static List<string> SplitTextIntoChunks(string text, int numberOfLines)
        {
            var chunks = new List<string>();
            var sb = new StringBuilder();

            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                sb.AppendLine(lines[i]);

                if ((i + 1) % numberOfLines == 0 || i == lines.Length - 1)
                {
                    chunks.Add(sb.ToString());
                    sb.Clear();
                }
            }

            return chunks;
        }
    }
}

