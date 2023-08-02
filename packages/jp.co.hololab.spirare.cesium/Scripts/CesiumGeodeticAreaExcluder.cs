using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    public class CesiumGeodeticAreaExcluder : CesiumTileExcluder
    {
        [SerializeField]
        private double upperLeftLatitude;

        [SerializeField]
        private double upperLeftLongitude;

        [SerializeField]
        private double lowerRightLatitude;

        [SerializeField]
        private double lowerRightLongitude;

        [SerializeField]
        private bool invert = false;

        public double UpperLeftLatitude
        {
            get
            {
                return upperLeftLatitude;
            }
            set
            {
                upperLeftLatitude = value;
                UpdateBounds();
            }
        }

        public double UpperLeftLongitude
        {
            get
            {
                return upperLeftLongitude;
            }
            set
            {
                upperLeftLongitude = value;
                UpdateBounds();
            }
        }

        public double LowerRightLatitude
        {
            get
            {
                return lowerRightLatitude;
            }
            set
            {
                lowerRightLatitude = value;
                UpdateBounds();
            }
        }

        public double LowerRightLongitude
        {
            get
            {
                return lowerRightLongitude;
            }
            set
            {
                lowerRightLongitude = value;
                UpdateBounds();
            }
        }

        private CesiumGeoreference cesiumGeoreference;
        private Bounds bounds;

        private BoxCollider boxCollider;

        protected override void OnEnable()
        {
            cesiumGeoreference = GetComponentInParent<CesiumGeoreference>();
            boxCollider = GetComponent<BoxCollider>();

            base.OnEnable();

            UpdateBounds();
            cesiumGeoreference.changed += UpdateBounds;
        }

        protected void Update()
        {
#if UNITY_EDITOR
            UpdateBounds();
#endif
        }

        public override bool ShouldExclude(Cesium3DTile tile)
        {
            if (!this.enabled)
            {
                return false;
            }

            if (this.invert)
            {
                return this.CompletelyContains(tile.bounds);
            }

            return !this.bounds.Intersects(tile.bounds);
        }

        private bool CompletelyContains(Bounds bounds)
        {
            return Vector3.Min(this.bounds.max, bounds.max) == bounds.max &&
                   Vector3.Max(this.bounds.min, bounds.min) == bounds.min;
        }

        private void UpdateBounds()
        {
            if (cesiumGeoreference == null)
            {
                return;
            }

            var upperLeftPoint = GeodeticPositionToUnityPosition(cesiumGeoreference, upperLeftLatitude, upperLeftLongitude, 0);
            var lowerRightPoint = GeodeticPositionToUnityPosition(cesiumGeoreference, lowerRightLatitude, lowerRightLongitude, 0);

            var minX = Mathf.Min((float)upperLeftPoint.x, (float)lowerRightPoint.x);
            var minZ = Mathf.Min((float)upperLeftPoint.z, (float)lowerRightPoint.z);
            bounds.min = new Vector3(minX, -10000, minZ);

            var maxX = Mathf.Max((float)upperLeftPoint.x, (float)lowerRightPoint.x);
            var maxZ = Mathf.Max((float)upperLeftPoint.z, (float)lowerRightPoint.z);
            bounds.max = new Vector3(maxX, 10000, maxZ);

            if (boxCollider != null)
            {
                boxCollider.center = bounds.center;
                boxCollider.size = bounds.size;
            }
        }

        private static double3 GeodeticPositionToUnityPosition(CesiumGeoreference cesiumGeoreference, double latitude, double longitude, double ellipsoidalHeight)
        {
            var ecef = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(longitude, latitude, ellipsoidalHeight));
            var unityPosition = cesiumGeoreference.TransformEarthCenteredEarthFixedPositionToUnity(ecef);
            return unityPosition;
        }
    }
}
