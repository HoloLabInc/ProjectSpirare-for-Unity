using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Spirare.Browser.ARFoundation
{
    internal static class UiUtils
    {
        public static float LogarithmicLerp(float a, float b, float t)
        {
            var logA = Mathf.Log(a);
            var logB = Mathf.Log(b);

            return Mathf.Exp(Mathf.Lerp(logA, logB, t));
        }

        public static float LogarithmicInverseLerp(float a, float b, float t)
        {
            var logA = Mathf.Log(a);
            var logB = Mathf.Log(b);
            return Mathf.InverseLerp(logA, logB, Mathf.Log(t));
        }
    }
}

