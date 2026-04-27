public static class TypeSafeGlowTweenHelper
{

    public static PropertyTweener TweenGlowColor(this Tween tween, IGlow node, float finalVal, float duration)
    {
        return tween.TweenProperty((GodotObject)node, IGlow.ColorProperty, finalVal, duration);
    }

    /**
     * Tweens glow color to 1
     */
    public static PropertyTweener GlowFadeIn(this Tween tween, IGlow node, float duration)
    {
        return tween.TweenGlowColor(node, 1f, duration);
    }

    /**
     * Tweens glow color to 0
     */
    public static PropertyTweener GlowFadeOut(this Tween tween, IGlow node, float duration)
    {
        return tween.TweenGlowColor(node, 0f, duration);
    }
}