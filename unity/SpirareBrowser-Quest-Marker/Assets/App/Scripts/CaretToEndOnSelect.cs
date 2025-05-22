using System;
using TMPro;
using UnityEngine;

namespace HoloLab.Spirare.Quest
{
    [RequireComponent(typeof(TMP_InputField))]
    public class CaretToEndOnSelect : MonoBehaviour
    {
        private TMP_InputField input;

        private void Awake()
        {
            input = GetComponent<TMP_InputField>();
            input.onSelect.AddListener(MoveCaretToEnd);
        }

        private void OnDestroy()
        {
            input.onSelect.RemoveListener(MoveCaretToEnd);
        }

        private void MoveCaretToEnd(string text)
        {
            var len = input.text.Length;

            input.caretPosition = len;
            input.stringPosition = len;
            input.selectionStringAnchorPosition = len;
            input.selectionStringFocusPosition = len;
        }
    }
}

