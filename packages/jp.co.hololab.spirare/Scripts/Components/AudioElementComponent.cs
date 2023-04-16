using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace HoloLab.Spirare
{
    public sealed class AudioElementComponent : MonoBehaviour
    {
        private static Dictionary<string, AudioType> _audioTypeDic = new Dictionary<string, AudioType>()
        {
            { ".mp3", AudioType.MPEG },
            { ".wav", AudioType.WAV },
        };
        private PomlAudioElement _audioElement;
        private AudioSource _audioSource;

        private Transform _listener;
        private float _distancePrev = float.MaxValue;

        private void Awake()
        {
            if (_audioElement == null)
            {
                _audioElement = new PomlAudioElement();
            }
            _audioSource = gameObject.AddComponent<AudioSource>();
            _listener = Camera.main.transform;
        }

        private async void Start()
        {
            var extension = _audioElement.GetSrcFileExtension();
            var audioClip = await LoadAudio(_audioElement.Src, extension);
            _audioSource.clip = audioClip;
            _audioSource.loop = _audioSource.loop;

            if (_audioElement.PlayDistance == 0 && _audioElement.StopDistance == 0)
            {
                return;
            }

            UniTask.Create(async () =>
            {
                while (this != null)
                {
                    AudioControl();
                    await UniTask.Yield();
                }
            }).Forget();
        }

        internal AudioElementComponent Initialize(PomlAudioElement audioElement)
        {
            _audioElement = audioElement;
            return this;
        }

        private void AudioControl()
        {
            var distance = (transform.position - _listener.position).magnitude;

            if (distance > _audioElement.StopDistance && _distancePrev <= _audioElement.StopDistance)
            {
                _audioSource.Stop();
                _audioSource.time = 0;
            }
            else if (distance < _audioElement.PlayDistance && _distancePrev >= _audioElement.PlayDistance)
            {
                if (_audioSource.isPlaying == false)
                {
                    _audioSource.Play();
                }
            }
            _distancePrev = distance;
        }

        private static async UniTask<AudioClip> LoadAudio(string url, string extension)
        {
            if (string.IsNullOrEmpty(extension) || _audioTypeDic.TryGetValue(extension, out var audioType) == false)
            {
                Debug.LogWarning($"The audio format is not supported. (Src= \"{url}\")");
                return null;
            }
            using (var www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                await www.SendWebRequest();
                return DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }
}
