public static class TypeSafeGlowTweenHelper
{

    public static PropertyTweener TweenGlowStrength(this Tween tween, IGlow node, float finalVal, float duration)
    {
        return tween.TweenProperty((GodotObject)node, IGlow.StrengthProperty, finalVal, duration);
    }
    
    public static PropertyTweener TweenGlowRadius(this Tween tween, IGlow node, float finalVal, float duration)
    {
        return tween.TweenProperty((GodotObject)node, IGlow.RadiusProperty, finalVal, duration);
    }

}