// This file includes code derived from Pcx - Point cloud importer & renderer for Unity
// (https://github.com/keijiro/Pcx) under the Unlicense License.
// The original code was developed by keijiro.

// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HoloLab.Spirare.Pcx
{
    /// A container class for texture-baked point clouds.
    public sealed class BakedPointCloud : ScriptableObject
    {
        #region Public properties

        /// Number of points
        public int pointCount { get { return _pointCount; } }

        /// Position map texture
        public Texture2D positionMap { get { return _positionMap; } }

        /// Color map texture
        public Texture2D colorMap { get { return _colorMap; } }

        public Bounds bounds { get { return _bounds; } }

        #endregion

        #region Serialized data members

        [SerializeField] int _pointCount;
        [SerializeField] Texture2D _positionMap;
        [SerializeField] Texture2D _colorMap;
        [SerializeField] Bounds _bounds;

        #endregion

        public void Initialize(List<Vector3> positions, List<Color32> colors)
        {
            _pointCount = positions.Count;

            _bounds = CalcBounds(positions);

            var width = Mathf.CeilToInt(Mathf.Sqrt(_pointCount));

            _positionMap = new Texture2D(width, width, TextureFormat.RGBAHalf, false);
            _positionMap.name = "Position Map";
            _positionMap.filterMode = FilterMode.Point;

            _colorMap = new Texture2D(width, width, TextureFormat.RGBA32, false);
            _colorMap.name = "Color Map";
            _colorMap.filterMode = FilterMode.Point;

            var i1 = 0;
            var i2 = 0U;

            for (var y = 0; y < width; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var i = i1 < _pointCount ? i1 : (int)(i2 % _pointCount);
                    var p = positions[i];

                    _positionMap.SetPixel(x, y, new Color(p.x, p.y, p.z));
                    _colorMap.SetPixel(x, y, colors[i]);

                    i1++;
                    i2 += 132049U; // prime
                }
            }

            _positionMap.Apply(false, true);
            _colorMap.Apply(false, true);
        }

        private static Bounds CalcBounds(List<Vector3> positions)
        {
            var minX = positions.Min(p => p.x);
            var minY = positions.Min(p => p.y);
            var minZ = positions.Min(p => p.z);

            var maxX = positions.Max(p => p.x);
            var maxY = positions.Max(p => p.y);
            var maxZ = positions.Max(p => p.z);

            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
            return bounds;
        }
    }
}

