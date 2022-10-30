using UnityEngine;

namespace Dman.Utilities
{
    public static class HDRColorConversions
    {

        public static float GetIntensity(Color hdrColor)
        {
            var maxColorComponent = hdrColor.maxColorComponent;
            float intensity = Mathf.Log(maxColorComponent) / Mathf.Log(2f);
            return intensity;
        }

        public static Color AdjustIntensity(Color color, float additionalIntensity)
        {
            float factor = Mathf.Pow(2, additionalIntensity);
            return new Color(color.r * factor, color.g * factor, color.b * factor, 1);
        }

        public static Color GetColorAtIntensity(Color color, float newIntensity)
        {
            var origialIntensity = HDRColorConversions.GetIntensity(color);
            return HDRColorConversions.AdjustIntensity(color, newIntensity - origialIntensity);
        }
    }
}
