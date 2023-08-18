using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace HoloLab.Spirare
{
    public sealed class Poml
    {
        public PomlScene Scene { get; }
        public PomlResource Resource { get; }

        public Poml(PomlScene scene, PomlResource resource)
        {
            Scene = scene;
            Resource = resource;
        }
    }

    public sealed class PomlScene
    {
        public IEnumerable<PomlElement> Elements { get; }

        public string WsRecvUrl { get; set; }

        public PomlScene(IEnumerable<PomlElement> elements)
        {
            Elements = elements;
        }
    }

    public sealed class PomlResource
    {
        public IEnumerable<PomlElement> Elements { get; }

        public PomlResource(IEnumerable<PomlElement> elements)
        {
            Elements = elements;
        }
    }

    public abstract class PomlElement
    {
        public PomlElementType ElementType { get; }
        public ElementAttributeType Attribute { get; set; }

        public Dictionary<string, string> CustomAttributes { get; } = new Dictionary<string, string>();

        public string Id { get; set; }
        public string WsRecvUrl { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        // TODO: Refactor.
        public string Src { get; set; }
        public string Filename { get; set; }

        public PomlRotationMode RotationMode { get; set; }

        /// <summary>
        /// Magnification to change the scale according to the distance (no change if null)
        /// </summary>
        public float? ScaleByDistance { get; set; }

        /// <summary>
        /// Minimum scale (used when changing scale with ScaleByDistance)
        /// </summary>
        public Vector3? MinScale { get; set; }

        /// <summary>
        /// Maximum scale (used when changing scale with ScaleByDistance)
        /// </summary>
        public Vector3? MaxScale { get; set; }

        public PomlDisplayType Display { set; get; }

        public PomlArDisplayType ArDisplay { set; get; }


        public PomlElement Parent { get; set; }

        public IEnumerable<PomlElement> Children { get; set; }

        public string WebLink { get; set; }

        public PomlDisplayType DisplayInHierarchy
        {
            get
            {
                if (Parent == null)
                {
                    return Display;
                }

                return GetHigherPriority(Parent.DisplayInHierarchy, Display);
            }
        }

        public PomlDisplayType ArDisplaySelf
        {
            get
            {
                switch (ArDisplay)
                {
                    case PomlArDisplayType.SameAsDisplay:
                        return Display;
                    case PomlArDisplayType.Visible:
                        return PomlDisplayType.Visible;
                    case PomlArDisplayType.None:
                        return PomlDisplayType.None;
                    case PomlArDisplayType.Occlusion:
                        return PomlDisplayType.Occlusion;
                    default:
                        return PomlDisplayType.Visible;
                }
            }
        }

        public PomlDisplayType ArDisplayInHierarchy
        {
            get
            {
                if (Parent == null)
                {
                    return ArDisplaySelf;
                }

                return GetHigherPriority(Parent.ArDisplayInHierarchy, ArDisplaySelf);
            }
        }

        protected PomlElement(PomlElementType elementType)
        {
            ElementType = elementType;
        }

        private PomlDisplayType GetHigherPriority(PomlDisplayType displayType1, PomlDisplayType displayType2)
        {
            var priorityOrder = new PomlDisplayType[] {
                PomlDisplayType.None,
                PomlDisplayType.Occlusion,
                PomlDisplayType.Visible
            };

            foreach (var displayType in priorityOrder)
            {
                if (displayType1 == displayType || displayType2 == displayType)
                {
                    return displayType;
                }
            }

            return displayType1;
        }
    }

    public sealed class PomlEmptyElement : PomlElement
    {
        public PomlEmptyElement() : base(PomlElementType.Element)
        {
        }
    }

    public sealed class PomlModelElement : PomlElement
    {
        public PomlModelElement() : base(PomlElementType.Model)
        {
        }
    }

    public sealed class PomlImageElement : PomlElement
    {
        /// <summary>Width [m]</summary>
        public float Width { get; set; }
        /// <summary>Height [m]</summary>
        public float Height { get; set; }

        public PomlImageElement() : base(PomlElementType.Image)
        {
        }
    }

    public sealed class PomlVideoElement : PomlElement
    {
        /// <summary>Width [m]</summary>
        public float Width { get; set; }
        /// <summary>Height [m]</summary>
        public float Height { get; set; }

        public PomlVideoElement() : base(PomlElementType.Video)
        {
        }
    }

    public sealed class PomlTextElement : PomlElement
    {
        public string Text { get; set; }
        public string FontSize { get; set; } = "";
        public Color FontColor { get; set; } = Color.white;
        public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 0);
        public string TextAlign { get; set; } = "";

        /// <summary>Text area width [m]</summary>
        public float Width { get; set; }
        /// <summary>Text area height [m]</summary>
        public float Height { get; set; }

        public float BorderWidth { get; set; }

        public PomlTextElement(string text) : base(PomlElementType.Text)
        {
            Text = text;
        }
    }

    public sealed class PomlGeometryElement : PomlElement
    {
        public List<PomlGeometry> Geometries { get; } = new List<PomlGeometry>();

        public PomlGeometryElement() : base(PomlElementType.Geometry)
        {
        }
    }

    public enum PositionType
    {
        [EnumLabel("relative")]
        Relative = 0,
        [EnumLabel("geo-location")]
        GeoLocation
    }

    public enum PomlGeometryType
    {
        Unknown = 0,
        [EnumLabel("line")]
        Line,
        [EnumLabel("polygon")]
        Polygon,
    }

    public readonly struct PomlGeodeticPosition
    {
        public PomlGeodeticPosition(double latitude, double longitude, double ellipsoidalHeight)
        {
            Latitude = latitude;
            Longitude = longitude;
            EllipsoidalHeight = ellipsoidalHeight;
        }

        public double Latitude { get; }
        public double Longitude { get; }
        public double EllipsoidalHeight { get; }
    }

    public abstract class PomlGeometry
    {
        public PomlGeometryType Type { get; }

        protected PomlGeometry(PomlGeometryType type)
        {
            Type = type;
        }
    }

    public sealed class LineGeometry : PomlGeometry
    {
        public LineGeometry() : base(PomlGeometryType.Line)
        {
        }

        public PositionType PositionType { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }

        public PomlGeodeticPosition StartGeoLocation { get; set; }
        public PomlGeodeticPosition EndGeoLocation { get; set; }

        public Color Color { get; set; }
        public float Width { get; set; }
    }

    public sealed class PolygonGeometry : PomlGeometry
    {
        public PolygonGeometry() : base(PomlGeometryType.Polygon)
        {
        }

        public PositionType PositionType { get; set; }

        public Vector3[] Vertices { get; set; }

        // public (double Latitude, double Longitude, double EllipsoidalHeight) StartGeoLocation { get; set; }
        // public (double Latitude, double Longitude, double EllipsoidalHeight) EndGeoLocation { get; set; }

        public Color Color { get; set; }
    }

    public class PomlScriptElement : PomlElement
    {
        public IReadOnlyList<string> Args;

        public PomlScriptElement() : base(PomlElementType.Script) { }
    }

    public class PomlSpaceReferenceElement : PomlElement
    {
        public string SpaceType;
        public string SpaceId;

        public PomlSpaceReferenceElement() : base(PomlElementType.SpaceReference)
        {
        }
    }

    public class PomlGeoReferenceElement : PomlElement
    {
        public double Latitude;
        public double Longitude;
        public double EllipsoidalHeight;
        public Quaternion EnuRotation;

        public PomlGeoReferenceElement() : base(PomlElementType.GeoReference)
        {
        }
    }

    public class PomlAudioElement : PomlElement
    {
        public bool Loop;
        public float PlayDistance;
        public float StopDistance;

        public PomlAudioElement() : base(PomlElementType.Audio)
        {
        }
    }

    public sealed class PomlCesium3dTilesElement : PomlElement
    {
        public PomlCesium3dTilesElement() : base(PomlElementType.Cesium3dTiles)
        {
        }
    }

    public sealed class PomlScreenSpaceElement : PomlElement
    {
        public PomlScreenSpaceElement() : base(PomlElementType.ScreenSpace)
        {
        }
    }


    [Flags]
    public enum ElementAttributeType
    {
        None = 0,
        [EnumLabel("static")]
        Static = 1,
        [EnumLabel("equipable")]
        [EnumLabel("equippable")]
        Equipable = 2,
    }

    public enum PomlElementType
    {
        [EnumLabel("#comment")]
        None = 0,
        [EnumLabel("element")]
        Element = 1,
        [EnumLabel("model")]
        Model = 2,
        [EnumLabel("text")]
        Text = 3,
        [EnumLabel("image")]
        Image = 4,
        [EnumLabel("video")]
        Video = 5,
        [EnumLabel("audio")]
        Audio = 6,
        [EnumLabel("geometry")]
        Geometry = 7,
        [EnumLabel("cesium3dtiles")]
        Cesium3dTiles = 8,
        [EnumLabel("script")]
        Script = 9,
        [EnumLabel("space-reference")]
        [EnumLabel("space-placement")] // space-placement is deprecated
        SpaceReference = 10,
        [EnumLabel("geo-reference")]
        [EnumLabel("geo-placement")] // geo-placement is deprecated
        GeoReference = 11,
        [EnumLabel("screen-space")]
        ScreenSpace = 12,
    }

    public enum PomlPrimitiveElementType
    {
        None = 0,
        [EnumLabel("cube")]
        Cube,
        [EnumLabel("sphere")]
        Sphere,
        [EnumLabel("cylinder")]
        Cylinder,
        [EnumLabel("plane")]
        Plane,
        [EnumLabel("capsule")]
        Capsule,
    }

    public enum PomlRotationMode
    {
        [EnumLabel("none")]
        None = 0,
        [EnumLabel("billboard")]
        Billboard,
        [EnumLabel("vertical-billboard")]
        VerticalBillboard,
    }

    public enum PomlDisplayType
    {
        [EnumLabel("visible")]
        Visible = 0,
        [EnumLabel("none")]
        None,
        [EnumLabel("occlusion")]
        Occlusion,
    }

    public enum PomlArDisplayType
    {
        [EnumLabel("same-as-display")]
        SameAsDisplay = 0,
        [EnumLabel("visible")]
        Visible,
        [EnumLabel("none")]
        None,
        [EnumLabel("occlusion")]
        Occlusion,
    }
}
