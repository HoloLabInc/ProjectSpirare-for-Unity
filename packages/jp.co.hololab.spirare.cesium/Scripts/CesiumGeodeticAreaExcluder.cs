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
        private float northHeading;

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

        public float NorthHeading
        {
            get
            {
                return northHeading;
            }
            set
            {
                northHeading = value;
                UpdateBounds();
            }
        }

        private CesiumGeoreference cesiumGeoreference;

        private SatCollisionDetector.Rect? displayArea;

        protected override void OnEnable()
        {
            cesiumGeoreference = GetComponentInParent<CesiumGeoreference>();

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

            if (displayArea.HasValue == false)
            {
                return false;
            }

            var tileRect = BoundsToRect(tile.bounds);
            return !SatCollisionDetector.Intersects(tileRect, displayArea.Value);
        }

        private void UpdateBounds()
        {
            if (cesiumGeoreference == null)
            {
                return;
            }

            var upperLeftPoint = GeodeticPositionToUnityPosition(cesiumGeoreference, upperLeftLatitude, upperLeftLongitude, 0);
            var lowerRightPoint = GeodeticPositionToUnityPosition(cesiumGeoreference, lowerRightLatitude, lowerRightLongitude, 0);

            var axis1 = CreateVector2FromAngle(-northHeading);
            var axis2 = CreateVector2FromAngle(-northHeading + 90);

            var upperLeft2D = new Vector2((float)upperLeftPoint.x, (float)upperLeftPoint.z);
            var lowerRight2D = new Vector2((float)lowerRightPoint.x, (float)lowerRightPoint.z);

            var vector1 = Vector2.Dot(lowerRight2D - upperLeft2D, axis1) * axis1;
            var vector2 = Vector2.Dot(lowerRight2D - upperLeft2D, axis2) * axis2;

            displayArea = new SatCollisionDetector.Rect()
            {
                Corner = upperLeft2D,
                Vector1 = vector1,
                Vector2 = vector2,
            };
        }

        private static SatCollisionDetector.Rect BoundsToRect(Bounds bounds)
        {
            var center = new Vector2(bounds.center.x, bounds.center.z);
            var extents = new Vector2(bounds.extents.x, bounds.extents.z);

            return new SatCollisionDetector.Rect()
            {
                Corner = center - extents,
                Vector1 = new Vector2(extents.x * 2, 0),
                Vector2 = new Vector2(0, extents.y * 2)
            };
        }

        private static Vector2 CreateVector2FromAngle(float angleInDegrees)
        {
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        }

        private static double3 GeodeticPositionToUnityPosition(CesiumGeoreference cesiumGeoreference, double latitude, double longitude, double ellipsoidalHeight)
        {
            var ecef = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(longitude, latitude, ellipsoidalHeight));
            var unityPosition = cesiumGeoreference.TransformEarthCenteredEarthFixedPositionToUnity(ecef);
            return unityPosition;
        }
    }
}
