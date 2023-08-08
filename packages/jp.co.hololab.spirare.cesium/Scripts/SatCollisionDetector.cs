using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloLab.Spirare.Cesium
{
    internal static class SatCollisionDetector
    {
        public struct Rect
        {
            public Vector2 TopLeft;
            public Vector2 Right;
            public Vector2 Down;
        }

        private struct Range
        {
            public float Min;
            public float Max;
        }

        public static bool Intersects(Rect rect1, Rect rect2)
        {
            var axes = new[] { rect1.Right, rect1.Down, rect2.Right, rect2.Down };
            foreach (var axis in axes)
            {
                var intersect = IntersectWithAxis(rect1, rect2, axis);
                if (intersect == false)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IntersectWithAxis(Rect rect1, Rect rect2, Vector2 axis)
        {
            var range1 = ProjectOntoAxis(rect1, axis);
            var range2 = ProjectOntoAxis(rect2, axis);

            if (range1.Max < range2.Min || range2.Max < range1.Min)
            {
                return false;
            }

            return true;
        }

        private static Range ProjectOntoAxis(Rect rect, Vector2 axis)
        {
            var topLeft = rect.TopLeft;
            var topRight = topLeft + rect.Right;
            var bottomLeft = topLeft + rect.Down;
            var bottomRight = bottomLeft + rect.Right;
            var points = new[] { topLeft, topRight, bottomLeft, bottomRight };

            var projections = points.Select(x => Vector2.Dot(x, axis)).ToArray();

            var min = Mathf.Min(projections);
            var max = Mathf.Max(projections);

            return new Range() { Max = max, Min = min };
        }
    }
}
