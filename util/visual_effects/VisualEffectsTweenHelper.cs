public static class VisualEffectsTweenHelper
{
    public static PropertyTweener TweenBaseTint(this Tween tween, VisualEffects effects, Color tint, float duration)
    {
        return tween.TweenProperty(effects.Material, VisualEffects.BaseTintProperty, tint, duration);
    }

    public static PropertyTweener TweenGlowColor(this Tween tween, VisualEffects effects, Color color, float duration)
    {
        return tween.TweenProperty(effects.Material, VisualEffects.GlowColorProperty, color, duration);
    }

    public static PropertyTweener TweenGlowRadius(this Tween tween, VisualEffects effects, float radius, float duration)
    {
        return tween.TweenProperty(effects.Material, VisualEffects.GlowRadiusProperty, radius, duration);
    }

    public static PropertyTweener TweenGlowFadeIn(this Tween tween, VisualEffects effects, float duration)
    {
        return tween.TweenProperty(effects.Material, VisualEffects.GlowColorAlphaProperty, 1f, duration);
    }

    public static PropertyTweener TweenGlowFadeOut(this Tween tween, VisualEffects effects, float duration)
    {
        return tween.TweenProperty(effects.Material, VisualEffects.GlowColorAlphaProperty, 0f, duration);
    }

    public static PropertyTweener TweenMaskProgress(this Tween tween, VisualEffects effects, float progress, float duration)
    {
        return tween.TweenProperty(effects.Material, VisualEffects.MaskProgressProperty, progress, duration);
    }

    public static PropertyTweener TweenDissolveValue(this Tween tween, VisualEffects effects, float value, float duration)
    {
        return tween.TweenProperty(effects.Material, VisualEffects.DissolveValueProperty, value, duration);
    }
}
