public static class Easings
{
    public static float LerpSine(float from, float to, float weight)
    {
        return Lerp(from, to, Sin(weight * Pi * 0.5f));
    }

    public static float LerpQuad(float from, float to, float weight)
    {
        return Lerp(from, to, weight * weight);
    }

    public static float LerpCubic(float from, float to, float weight)
    {
        return Lerp(from, to, weight * weight * weight);
    }
}