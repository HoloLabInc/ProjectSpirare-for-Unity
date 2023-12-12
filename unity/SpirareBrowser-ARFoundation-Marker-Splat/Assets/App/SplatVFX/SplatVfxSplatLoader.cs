using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using SplatVfx;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.VFX.Utility;
using UnityEngine.VFX;

namespace HoloLab.Spirare
{
    internal class SplatVfxSplatLoader
    {
        public enum LoadingStatus
        {
            None,
            DataFetching,
            ModelLoading,
            ModelInstantiating,
            Loaded,
            DataFetchError,
            ModelLoadError,
            ModelInstantiateError
        }

        public async Task<(bool Success, GameObject SplatObject)> LoadAsync(Transform parent, string src, VisualEffect splatPrefab, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            /*
            // Create GameObject
            var glbInstance = CreateGlbInstance(this, src, material);
            instanceReference.AddInstance(src, material, glbInstance.gameObject);

            // Search cache
            var cacheResult = await gltfImportCacheManager.GetGltfImportAsync(src, material);
            if (cacheResult.Success)
            {
                return await InstantiateModel(glbInstance, parent, cacheResult.GltfImport, onLoadingStatusChanged);
            }

            var creationTaskGenerated = gltfImportCacheManager.GenerateCreationTask(src, material);
            */

            // Data fetching
            var fetchResult = await FetchData(src, onLoadingStatusChanged);

            if (fetchResult.Success == false)
            {
                /*
                if (creationTaskGenerated)
                {
                    gltfImportCacheManager.CancelCreationTask(src, material);
                }
                UnityEngine.Object.Destroy(glbInstance.gameObject);
                */
                return (false, null);
            }

            var data = CreateSplatData(fetchResult.Data);

            var visualEffect = UnityEngine.Object.Instantiate(splatPrefab);
            var splatObject = visualEffect.gameObject;

            var binderBase = splatObject.AddComponent<VFXPropertyBinder>();
            var binder = binderBase.AddPropertyBinder<VFXSplatDataBinder>();
            binder.SplatData = data;

            //await UniTask.Yield();
            await UniTask.Delay(5000);
            // Change position after VFX is initialized
            splatObject.transform.SetParent(parent, worldPositionStays: false);

            InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);

            return (true, splatObject);

            // var glbInstance = glbObject.AddComponent<GltfastGlbInstance>();

            // return glbInstance;
            /*
            // Model loading
            var loadResult = await LoadModel(fetchResult.Data, material, onLoadingStatusChanged);

            if (loadResult.Success == false)
            {
                if (creationTaskGenerated)
                {
                    gltfImportCacheManager.CancelCreationTask(src, material);
                }
                UnityEngine.Object.Destroy(glbInstance.gameObject);
                return (false, null);
            }

            var gltfImport = loadResult.gltfImport;

            if (creationTaskGenerated)
            {
                gltfImportCacheManager.CompleteCreationTask(src, material, gltfImport);
            }
            */

            // Model instantiating
            // return await InstantiateModel(glbInstance, parent, gltfImport, onLoadingStatusChanged);
        }

        private static SplatData CreateSplatData(byte[] splatBytes)
        {
            var data = ScriptableObject.CreateInstance<SplatData>();
            // data.name = Path.GetFileNameWithoutExtension(path);

            var arrays = LoadDataArrays(splatBytes);
            data.PositionArray = arrays.position;
            data.AxisArray = arrays.axis;
            data.ColorArray = arrays.color;
            data.ReleaseGpuResources();

            return data;
        }

#pragma warning disable CS0649

        private struct ReadData
        {
            public float px, py, pz;
            public float sx, sy, sz;
            public byte r, g, b, a;
            public byte rw, rx, ry, rz;
        }

#pragma warning restore CS0649

        private static (Vector3[] position, Vector3[] axis, Color[] color) LoadDataArrays(byte[] splatBytes)
        {
            // var bytes = (Span<byte>)File.ReadAllBytes(path);
            var bytes = new Span<byte>(splatBytes);
            var count = bytes.Length / 32;

            var source = MemoryMarshal.Cast<byte, ReadData>(bytes);

            var position = new Vector3[count];
            var axis = new Vector3[count * 3];
            var color = new Color[count];

            for (var i = 0; i < count; i++)
                ParseReadData(source[i],
                              out position[i],
                              out axis[i * 3],
                              out axis[i * 3 + 1],
                              out axis[i * 3 + 2],
                              out color[i]);

            return (position, axis, color);
        }

        [BurstCompile]
        private static void ParseReadData(in ReadData src,
                           out Vector3 position,
                           out Vector3 axis1,
                           out Vector3 axis2,
                           out Vector3 axis3,
                           out Color color)
        {
            var rv = (math.float4(src.rx, src.ry, src.rz, src.rw) - 128) / 128;
            // var q = math.quaternion(-rv.x, -rv.y, rv.z, rv.w);
            var q = math.quaternion(rv.x, -rv.y, rv.z, -rv.w);
            //position = math.float3(src.px, src.py, -src.pz);
            //position = math.float3(src.px, -src.py, src.pz);
            position = math.float3(src.px, -src.py, src.pz);
            axis1 = math.mul(q, math.float3(src.sx, 0, 0));
            axis2 = math.mul(q, math.float3(0, src.sy, 0));
            axis3 = math.mul(q, math.float3(0, 0, src.sz));
            color = (Vector4)math.float4(src.r, src.g, src.b, src.a) / 255;
        }



        /*
        internal void RemoveInstanceReference(GltfastGlbInstance glbInstance)
        {
            var src = glbInstance.Src;
            var material = glbInstance.Material;

            instanceReference.RemoveInstance(src, material, glbInstance.gameObject);

            var referenceCount = instanceReference.GetInstanceCount(src, material);
            if (referenceCount == 0)
            {
                gltfImportCacheManager.RemoveCache(src, material);
                glbInstance.GltfImport?.Dispose();
            }
        }

        internal void ClearGltfImportCache()
        {
            gltfImportCacheManager.ClearAll();
        }
        */

        /*
        private static GltfastGlbInstance CreateGlbInstance(GltfastGlbLoader glbLoader, string src, Material material)
        {
            var glbObject = new GameObject("Glb Instance");
            glbObject.hideFlags = HideFlags.HideInHierarchy;

            var glbInstance = glbObject.AddComponent<GltfastGlbInstance>();
            glbInstance.Initialize(glbLoader, src, material);

            return glbInstance;
        }

        */

        private static async UniTask<(bool Success, byte[] Data)> FetchData(string src, Action<LoadingStatus> onLoadingStatusChanged)
        {
            InvokeLoadingStatusChanged(LoadingStatus.DataFetching, onLoadingStatusChanged);

            var result = await SpirareHttpClient.Instance.GetByteArrayAsync(src, enableCache: true);
            if (result.Success)
            {
                return (true, result.Data);
            }
            else
            {
                InvokeLoadingStatusChanged(LoadingStatus.DataFetchError, onLoadingStatusChanged);
                Debug.LogWarning($"Failed to get model data: {src}");

                return (false, null);
            }
        }

        /*
        private static async UniTask<(bool Success, GltfImport gltfImport)> LoadModel(byte[] data, Material material, Action<LoadingStatus> onLoadingStatusChanged)
        {
            InvokeLoadingStatusChanged(LoadingStatus.ModelLoading, onLoadingStatusChanged);

            IMaterialGenerator materialGenerator = null;
            if (material != null)
            {
                materialGenerator = new OcclusionMaterialGenerator(material);
            }

            try
            {
                var gltfImport = new GltfImport(materialGenerator: materialGenerator);
                var loadResult = await gltfImport.LoadGltfBinary(data);
                if (loadResult == false)
                {
                    gltfImport.Dispose();
                    gltfImport = null;

                    InvokeLoadingStatusChanged(LoadingStatus.ModelLoadError, onLoadingStatusChanged);
                }

                return (loadResult, gltfImport);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return (false, null);
            }
        }

        private static async UniTask<(bool Success, GameObject GltfObject)> InstantiateModel(GltfastGlbInstance glbInstance, Transform parent, GltfImport gltfImport, Action<LoadingStatus> onLoadingStatusChanged = null)
        {
            if (glbInstance == null)
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
                return (false, null);
            }

            glbInstance.transform.SetParent(parent, false);
            glbInstance.gameObject.hideFlags = HideFlags.None;
            glbInstance.SetGltfImport(gltfImport);

            if (parent == null)
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
                UnityEngine.Object.Destroy(glbInstance.gameObject);
                return (false, null);
            }

            InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiating, onLoadingStatusChanged);
            var instantiationResult = await gltfImport.InstantiateMainSceneAsync(glbInstance.transform, CancellationToken.None);
            if (instantiationResult)
            {
                InvokeLoadingStatusChanged(LoadingStatus.Loaded, onLoadingStatusChanged);
                return (true, glbInstance.gameObject);
            }
            else
            {
                InvokeLoadingStatusChanged(LoadingStatus.ModelInstantiateError, onLoadingStatusChanged);
                UnityEngine.Object.Destroy(glbInstance.gameObject);
                return (false, null);
            }
        }
        */

        private static void InvokeLoadingStatusChanged(LoadingStatus status, Action<LoadingStatus> onLoadingStatusChanged)
        {
            try
            {
                onLoadingStatusChanged?.Invoke(status);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
