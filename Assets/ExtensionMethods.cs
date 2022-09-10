using UnityEngine;

public static class ExtensionMethods
{
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static Color RandomOffset(this Color original, float random)
    {
        var r = Mathf.Clamp01(original.r+Random.Range(-1.0f, 1.0f) * random);
        var g = Mathf.Clamp01(original.g+Random.Range(-1.0f, 1.0f) * random);
        var b = Mathf.Clamp01(original.b+Random.Range(-1.0f, 1.0f) * random);
        
        return new Color(r, g, b);
    }
}