// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using System;

#if WINDOWS_UWP && ENABLE_DOTNET
#else
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
#endif

namespace HoloLab.Spirare.Tools.Yaml
{
    public class YamlDeserializer
    {
#if WINDOWS_UWP && ENABLE_DOTNET
        public bool TryDeserialize<T>(string text, out T value)
        {
            throw new NotImplementedException();
        }

        public bool TryDeserializeFromPersistentDataPath<T>(string relativeFilepath, out T value)
        {
            throw new NotImplementedException();
        }

        public bool TryDeserializeFromFile<T>(string filepath, out T value)
        {
            throw new NotImplementedException();
        }

#else
        private IDeserializer deserializer;

        public YamlDeserializer()
        {
            deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
        }

        /// <summary>
        /// Parse a YAML-formatted string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryDeserialize<T>(string text, out T value)
        {
            try
            {
                value = deserializer.Deserialize<T>(text);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Parse a YAML file located under the Persistent Data Path
        /// For HoloLens, the Persistent Data Path is LocalAppData\<package name>\LocalState
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativeFilepath">Relative path</param>
        /// <param name="value">Parsed data</param>
        /// <returns></returns>
        public bool TryDeserializeFromPersistentDataPath<T>(string relativeFilepath, out T value)
        {
            var filepath = Path.Combine(Application.persistentDataPath, relativeFilepath);
            return TryDeserializeFromFile(filepath, out value);
        }

        /// <summary>
        /// Parse a YAML file at the specified file path
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filepath">File path</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryDeserializeFromFile<T>(string filepath, out T value)
        {
            try
            {
                using (var fs = new FileStream(filepath, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        value = deserializer.Deserialize<T>(sr);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                value = default;
                return false;
            }
        }
#endif
    }
}
