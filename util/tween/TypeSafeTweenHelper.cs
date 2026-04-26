/**
 * Helper class with extension functions to not accidentally pass incorrect property name or value type to the tween
 */
public static class TypeSafeTweenHelper
{

    public static PropertyTweener TweenRotationDegrees(this Tween tween, GodotObject node, float finalVal, float duration)
    {
        return tween.TweenProperty(node, RotationDegreesProperty, finalVal, duration);
    }
    
    public static PropertyTweener TweenRotationReset(this Tween tween, GodotObject node, float duration)
    {
        return tween.TweenProperty(node, RotationProperty, 0f, duration);
    }
    
    public static PropertyTweener TweenScale(this Tween tween, GodotObject node, float finalVal, float duration)
    {
        return tween.TweenProperty(node, ScaleProperty, new Vector2(finalVal, finalVal), duration);
    }
    
    public static PropertyTweener TweenScale(this Tween tween, GodotObject node, Vector2 finalVal, float duration)
    {
        return tween.TweenProperty(node, ScaleProperty, finalVal, duration);
    }
    
    /**
     * Resets scale to 1
     */
    public static PropertyTweener TweenScaleReset(this Tween tween, GodotObject node, float duration)
    {
        return tween.TweenProperty(node, ScaleProperty, Vector2.One, duration);
    }
        
    /**
     * Tweens modulate to Color(finalVal, finalVal, finalVal, 1), which means you have no control over alpha
     */
    public static PropertyTweener TweenModulate(this Tween tween, GodotObject node, float finalVal, float duration)
    {
        return tween.TweenProperty(node, ModulateProperty, new Color(finalVal, finalVal, finalVal), duration);
    }
    
    public static PropertyTweener TweenModulate(this Tween tween, GodotObject node, Color finalVal, float duration)
    {
        return tween.TweenProperty(node, ModulateProperty, finalVal, duration);
    }
    
    /**
     * Resets modulate to 1, the white color
     */
    public static PropertyTweener TweenModulateReset(this Tween tween, GodotObject node, float duration)
    {
        return tween.TweenProperty(node, ModulateProperty, Colors.White, duration);
    }
    
    public static PropertyTweener TweenAlpha(this Tween tween, GodotObject node, float finalVal, float duration)
    {
        return tween.TweenProperty(node, ModulateAlphaProperty, finalVal, duration);
    }
    
}