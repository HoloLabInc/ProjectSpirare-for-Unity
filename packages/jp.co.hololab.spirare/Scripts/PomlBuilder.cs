using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using HoloLab.PositioningTools.CoordinateSystem;

namespace HoloLab.Spirare
{
    public static class PomlBuilder
    {
        public static async Task<string> BuildPomlStringAsync(GameObject gameObject)
        {
            var poml = BuildPoml(gameObject);
            return await PomlWriter.WriteAsync(poml).ConfigureAwait(false);
        }

        public static Poml BuildPoml(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            var elements = new List<PomlElement>();
            foreach (Transform child in gameObject.transform)
            {
                var element = BuildElement(child);
                elements.Add(element);
            }
            var coordinateReferenceElements = BuildCoordinateReferenceElements(gameObject.transform);
            elements.AddRange(coordinateReferenceElements);

            var scene = new PomlScene(elements);
            var resource = new PomlResource(Array.Empty<PomlElement>());
            return new Poml(scene, resource);
        }

        private static PomlElement BuildElement(Transform transform)
        {
            bool ignoreChildren = false;
            PomlElement element;
            if (transform.TryGetComponent<ModelElementComponent>(out var mec))
            {
                element = new PomlModelElement()
                {
                    Src = mec.ModelSource,
                };
                ignoreChildren = true;
            }
            else if (transform.TryGetComponent<TextElementComponent>(out var tec))
            {
                var textElement = new PomlTextElement(tec.GetText());
                var fontSize = tec.GetFontSize();
                if (fontSize != null)
                {
                    textElement.FontSize = fontSize;
                }
                element = textElement;
            }
            else
            {
                element = new PomlEmptyElement();
            }
            element.Position = CoordinateUtility.ToSpirareCoordinate(transform.localPosition, directional: true);
            element.Scale = CoordinateUtility.ToSpirareCoordinate(transform.localScale, directional: false);
            element.Rotation = CoordinateUtility.ToSpirareCoordinate(transform.localRotation);

            var children = new List<PomlElement>();
            if (ignoreChildren)
            {
                element.Children = children;
            }
            else
            {
                foreach (Transform child in transform)
                {
                    var childElement = BuildElement(child);
                    children.Add(childElement);
                }
                element.Children = children;
            }

            var coordinateReferenceElements = BuildCoordinateReferenceElements(transform);
            children.AddRange(coordinateReferenceElements);

            return element;
        }

        private static List<PomlElement> BuildCoordinateReferenceElements(Transform transform)
        {
            var coordinateReferenceElements = new List<PomlElement>();

            if (transform.TryGetComponent<WorldCoordinateOrigin>(out var wco))
            {
                var geoPos = wco.GeodeticPosition;
                var enuRotation = CoordinateUtility.ToSpirareCoordinate(wco.EnuRotation);
                var reference = new PomlGeoReferenceElement()
                {
                    Latitude = geoPos.Latitude,
                    Longitude = geoPos.Longitude,
                    EllipsoidalHeight = geoPos.EllipsoidalHeight,
                    EnuRotation = enuRotation,
                };
                coordinateReferenceElements.Add(reference);
            }

            if (transform.TryGetComponent<SpaceReferenceComponent>(out var ico))
            {
                var reference = new PomlSpaceReferenceElement()
                {
                    SpaceType = ico.SpaceType,
                    SpaceId = ico.SpaceId
                };
                coordinateReferenceElements.Add(reference);
            }
            return coordinateReferenceElements;
        }
    }
}
