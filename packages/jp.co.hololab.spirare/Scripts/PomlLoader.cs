using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Spirare
{
    public class PomlLoadOptions
    {
        public enum DisplayModeType
        {
            Normal = 0,
            AR,
        }

        public Material OcclusionMaterial;
        public DisplayModeType DisplayMode;

        public int DefaultLayer;
        public int ScreenSpaceLayer;
    }

    public class PomlLoader
    {
        private PomlLoaderSettings pomlLoaderSettings;

        private ModelElementObjectFactory modelElementObjectFactory;
        private ImageElementObjectFactory imageElementObjectFactory;
        private VideoElementObjectFactory videoElementObjectFactory;
        private TextElementObjectFactory textElementObjectFactory;
        private GeometryElementObjectFactory geometryElementObjectFactory;
        private ScreenSpaceElementObjectFactory screenSpaceElementObjectFactory;

        private SpaceReferenceElementComponentFactory spaceReferenceElementComponentFactory;
        private GeoReferenceElementComponentFactory geoReferenceElementComponentFactory;
        private ScriptElementComponentFactory scriptElementComponentFactory;

        private PomlLoadOptions loadOptions;

        public PomlLoader(PomlLoaderSettings pomlLoaderSettings, PomlLoadOptions.DisplayModeType displayMode)
        {
            this.pomlLoaderSettings = pomlLoaderSettings;

            modelElementObjectFactory = pomlLoaderSettings.modelElementObjectFactory;
            imageElementObjectFactory = pomlLoaderSettings.imageElementObjectFactory;
            videoElementObjectFactory = pomlLoaderSettings.videoElementObjectFactory;
            textElementObjectFactory = pomlLoaderSettings.textElementObjectFactory;
            geometryElementObjectFactory = pomlLoaderSettings.geometryElementObjectFactory;
            screenSpaceElementObjectFactory = pomlLoaderSettings.screenSpaceElementObjectFactory;

            spaceReferenceElementComponentFactory = pomlLoaderSettings.spaceReferenceElementComponentFactory;
            geoReferenceElementComponentFactory = pomlLoaderSettings.geoReferenceElementComponentFactory;
            scriptElementComponentFactory = pomlLoaderSettings.scriptElementComponentFactory;

            loadOptions = new PomlLoadOptions()
            {
                OcclusionMaterial = pomlLoaderSettings.occlusionMaterial,
                DisplayMode = displayMode,
                DefaultLayer = pomlLoaderSettings.DefaultLayer,
                ScreenSpaceLayer = pomlLoaderSettings.ScreenSpaceLayer,
            };
        }

        public async Task<PomlComponent> LoadXmlAsync(string xml, string uri)
        {
            var targetGameObject = new GameObject("poml");
            try
            {
                return await LoadXmlAsync(xml, uri, targetGameObject);
            }
            catch
            {
                UnityEngine.Object.Destroy(targetGameObject);
                throw;
            }
        }

        public async Task<PomlComponent> LoadXmlAsync(string xml, string uri, GameObject targetGameObject)
        {
            var (succeeded, poml) = await Task.Run(() => (PomlParser.TryParse(xml, uri, out var p), p));
            if (succeeded == false)
            {
                throw new FormatException("Parsing poml is failed.");
            }
            var contentsStore = new ElementStore();
            var pomlComponent = targetGameObject.AddComponent<PomlComponent>().Initialize(contentsStore, poml, this, uri);
            await LoadScene(poml.Scene, pomlComponent);
            // await LoadResource(poml.Resource, pomlComponent);
            return pomlComponent;
        }

        private async Task LoadScene(PomlScene scene, PomlComponent pomlComponent)
        {
            var sceneOrigin = pomlComponent.transform;
            foreach (var element in scene.Elements)
            {
                await LoadElement(element, sceneOrigin, null, pomlComponent);
            }
        }

        /*
        private async Task LoadResource(PomlResource pomlResource, PomlComponent pomlComponent)
        {
            var resourceObject = new GameObject("resource");
            var resourceRoot = resourceObject.transform;
            resourceRoot.localScale = Vector3.zero;
            resourceRoot.SetParent(pomlComponent.transform, false);

            var contentsStore = pomlComponent.ElementStore;
            foreach (var element in pomlResource.Elements)
            {
                var t = await LoadElement(element, resourceRoot, null, pomlComponent);
                if (t == null)
                {
                    continue;
                }

                var resource = new Resource()
                {
                    Id = element.Id,
                    GameObject = t.gameObject
                };
                contentsStore.RegisterResource(resource);
            }
        }
        */

        internal async Task<Transform> LoadElement(PomlElement element, Transform parentTransform, PomlObjectElementComponent parentElementComponent, PomlComponent pomlComponent)
        {
            var elementComponent = GenerateElementComponent(element, parentTransform, parentElementComponent, pomlComponent);
            if (elementComponent != null)
            {
                pomlComponent.RegisterComponent(elementComponent);
                return null;
            }

            var go = GenerateElementObject(element, parentTransform);
            if (go == null)
            {
                Debug.LogWarning($"{element.ElementType} is not supported");
                return null;
            }

            if (go.TryGetComponent<PomlObjectElementComponent>(out var pomlObjectElementComponent) == false)
            {
                pomlObjectElementComponent = go.AddComponent<PomlObjectElementComponent>();
                pomlObjectElementComponent.Initialize(element);
            }

            // TODO: pass pomlComponent when initialization
            pomlObjectElementComponent.PomlComponent = pomlComponent;
            pomlComponent.RegisterComponent(pomlObjectElementComponent);

            // Load child elements
            foreach (var child in element.Children)
            {
                await LoadElement(child, go.transform, pomlObjectElementComponent, pomlComponent);
            }

            return go.transform;
        }


        private GameObject GenerateElementObject(PomlElement element, Transform parentTransform)
        {
            switch (element.ElementType)
            {
                case PomlElementType.Element:
                    return InstantiateEmptyElement(element, parentTransform);
                case PomlElementType.Model:
                    if (element is PomlModelElement modelElement)
                    {
                        if (modelElementObjectFactory == null)
                        {
                            Debug.LogWarning($"{nameof(ModelElementObjectFactory)} is not specified");
                        }
                        else
                        {
                            return modelElementObjectFactory.CreateObject(modelElement, loadOptions, parentTransform);
                        }
                    }
                    break;
                case PomlElementType.Text:
                    if (element is PomlTextElement textElement)
                    {
                        if (textElementObjectFactory == null)
                        {
                            Debug.LogWarning($"{nameof(TextElementObjectFactory)} is not specified");
                        }
                        else
                        {
                            return textElementObjectFactory.CreateObject(textElement, loadOptions, parentTransform);
                        }
                    }
                    break;
                case PomlElementType.Audio:
                    if (element is PomlAudioElement audioElement)
                    {
                        return InstantiateAudio(audioElement, parentTransform);
                    }
                    break;
                case PomlElementType.Image:
                    if (element is PomlImageElement imageElement)
                    {
                        if (imageElementObjectFactory == null)
                        {
                            Debug.LogWarning($"{nameof(ImageElementObjectFactory)} is not specified");
                        }
                        else
                        {
                            return imageElementObjectFactory.CreateObject(imageElement, loadOptions, parentTransform);
                        }
                    }
                    break;
                case PomlElementType.Video:
                    if (element is PomlVideoElement videoElement)
                    {
                        if (videoElementObjectFactory == null)
                        {
                            Debug.LogWarning($"{nameof(VideoElementObjectFactory)} is not specified");
                        }
                        else
                        {
                            return videoElementObjectFactory.CreateObject(videoElement, loadOptions, parentTransform);
                        }
                    }
                    break;
                case PomlElementType.Geometry:
                    if (element is PomlGeometryElement geometryElement)
                    {
                        if (geometryElementObjectFactory == null)
                        {
                            Debug.LogWarning($"{nameof(GeometryElementObjectFactory)} is not specified");
                        }
                        else
                        {
                            return geometryElementObjectFactory.CreateObject(geometryElement, geoReferenceElementComponentFactory, loadOptions, parentTransform);
                        }
                    }
                    break;
                case PomlElementType.Cesium3dTiles:
                    if (element is PomlCesium3dTilesElement cesium3dTilesElement)
                    {
                        return InstantiateCesium3dTiles(cesium3dTilesElement, parentTransform);
                    }
                    break;
                case PomlElementType.ScreenSpace:
                    if (element is PomlScreenSpaceElement screenSpaceElement)
                    {
                        return InstantiateScreenSpaceElement(screenSpaceElement, parentTransform);
                    }
                    break;
            }
            return null;
        }

        private PomlElementComponent GenerateElementComponent(PomlElement element, Transform parentTransform, PomlObjectElementComponent parentElementComponent, PomlComponent pomlComponent)
        {
            switch (element.ElementType)
            {
                case PomlElementType.SpaceReference:
                    if (element is PomlSpaceReferenceElement spaceReferenceElement)
                    {
                        if (spaceReferenceElementComponentFactory == null)
                        {
                            Debug.LogWarning("SpaceReferenceElementComponentFactory is not specified");
                        }
                        else
                        {
                            var spaceReferenceElementComponent = spaceReferenceElementComponentFactory.AddComponent(parentTransform.gameObject, spaceReferenceElement);
                            return spaceReferenceElementComponent;
                        }
                    }
                    break;
                case PomlElementType.GeoReference:
                    if (element is PomlGeoReferenceElement geoReferenceElement)
                    {
                        if (geoReferenceElementComponentFactory == null)
                        {
                            Debug.LogWarning("GeoReferenceElementComponentFactory is not specified");
                        }
                        else
                        {
                            var geoReferenceElementComponent = geoReferenceElementComponentFactory.AddComponent(parentTransform.gameObject, geoReferenceElement);
                            return geoReferenceElementComponent;
                        }
                    }
                    break;
                case PomlElementType.Script:
                    if (element is PomlScriptElement scriptElement)
                    {
                        if (scriptElementComponentFactory == null)
                        {
                            Debug.LogWarning("ScriptElementComponentFactory is not specified");
                        }
                        else
                        {
                            var scriptElementComponent = scriptElementComponentFactory.AddComponent(parentTransform.gameObject, scriptElement, parentElementComponent, pomlComponent);
                            return scriptElementComponent;
                        }
                    }
                    break;
            }
            return null;
        }

        private static GameObject InstantiateEmptyElement(PomlElement element, Transform parentTransform)
        {
            var go = new GameObject("element");
            go.transform.SetParent(parentTransform, false);
            return go;
        }

        private static GameObject InstantiateAudio(PomlAudioElement element, Transform parentTransform)
        {
            var go = new GameObject("audio");
            go.transform.SetParent(parentTransform, false);

            var audioComponent = go.AddComponent<AudioElementComponent>().Initialize(element);
            return go;
        }

        private GameObject InstantiateCesium3dTiles(PomlCesium3dTilesElement cesium3dTilesElement, Transform parentTransform)
        {
            var factory = pomlLoaderSettings.cesium3dTilesElementFactory;
            if (factory == null)
            {
                return null;
            }

            var elementObject = factory.Create(cesium3dTilesElement, loadOptions, parentTransform);
            return elementObject;
        }

        private GameObject InstantiateScreenSpaceElement(PomlScreenSpaceElement screenSpaceElement, Transform parentTransform)
        {
            if (screenSpaceElementObjectFactory == null)
            {
                Debug.LogWarning($"{nameof(ScreenSpaceElementObjectFactory)} is not specified");
                return null;
            }
            else
            {
                return screenSpaceElementObjectFactory.CreateObject(screenSpaceElement, loadOptions, parentTransform);
            }
        }
    }
}
