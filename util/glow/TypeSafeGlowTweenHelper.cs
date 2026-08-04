public static class TypeSafeGlowTweenHelper
{
    
    public static PropertyTweener TweenGlowColor(this Tween tween, IGlow node, Color color, float duration)
    {
        return tween.TweenProperty((GodotObject)node, IGlow.ColorProperty, color, duration);
    }

    public static PropertyTweener TweenGlowStrength(this Tween tween, IGlow node, float finalVal, float duration)
    {
        return tween.TweenProperty((GodotObject)node, IGlow.StrengthProperty, finalVal, duration);
    }
    
    public static PropertyTweener TweenGlowRadius(this Tween tween, IGlow node, float finalVal, float duration)
    {
        return tween.TweenProperty((GodotObject)node, IGlow.RadiusProperty, finalVal, duration);
    }
    
    public static PropertyTweener TweenGlowFadeOut(this Tween tween, IGlow node, float duration)
    {
        return tween.TweenProperty((GodotObject)node, IGlow.ColorAlphaProperty, 0, duration);
    }
    
    public static PropertyTweener TweenGlowFadeIn(this Tween tween, IGlow node, float duration)
    {
        return tween.TweenProperty((GodotObject)node, IGlow.ColorAlphaProperty, 1, duration);
    }

}