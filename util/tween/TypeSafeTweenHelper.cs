/**
 * Helper class with extension functions to not accidentally pass incorrect property name or value type to the tween
 */
public static class TypeSafeTweenHelper
{
    
    public static PropertyTweener TweenPosition(this Tween tween, GodotObject node, Vector2 finalVal, float duration)
    {
        return tween.TweenProperty(node, PositionProperty, finalVal, duration);
    }

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
    
    /**
     * Tweens alpha to 1
     */
    public static PropertyTweener FadeIn(this Tween tween, GodotObject node, float duration)
    {
        return tween.TweenAlpha(node, 1f, duration);
    }
    
    /**
     * Tweens alpha to 0
     */
    public static PropertyTweener FadeOut(this Tween tween, GodotObject node, float duration)
    {
        return tween.TweenAlpha(node, 0f, duration);
    }
    
    public static PropertyTweener TweenOffsetPosition(this Tween tween, Control node, Vector2 finalVal, float duration)
    {
        return tween.TweenProperty(node, OffsetTransformPosition, finalVal, duration);
    }
    
    public static PropertyTweener TweenOffsetPositionReset(this Tween tween, Control node, float duration)
    {
        return tween.TweenProperty(node, OffsetTransformPosition, Vector2.Zero, duration);
    }

    public static PropertyTweener TweenOffsetRotation(this Tween tween, Control node, float finalVal, float duration)
    {
        return tween.TweenProperty(node, OffsetTransformRotation, finalVal, duration);
    }
    
    public static PropertyTweener TweenOffsetRotationReset(this Tween tween, Control node, float duration)
    {
        return tween.TweenProperty(node, OffsetTransformRotation, 0, duration);
    }
    
    public static PropertyTweener TweenOffsetScale(this Tween tween, Control node, float finalVal, float duration)
    {
        return tween.TweenProperty(node, OffsetTransformScale, new Vector2(finalVal, finalVal), duration);
    }
    
    public static PropertyTweener TweenOffsetScaleReset(this Tween tween, Control node, float duration)
    {
        return tween.TweenProperty(node, OffsetTransformScale, Vector2.One, duration);
    }
    
}