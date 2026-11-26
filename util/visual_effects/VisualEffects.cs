using System;

public sealed class VisualEffects
{
    private static readonly Shader TextureShader = GD.Load<Shader>("uid://lmxpktqjq24x");
    private static readonly Shader CanvasGroupShader = GD.Load<Shader>("uid://dgkaqppo0v43c");

    private static readonly StringName BaseTintParam = "base_tint";
    private static readonly StringName LocalRectParam = "local_rect";
    private static readonly StringName GlowEnabledParam = "glow_enabled";
    private static readonly StringName GlowColorParam = "glow_color";
    private static readonly StringName GlowRadiusParam = "glow_radius";
    private static readonly StringName GlowStrengthParam = "glow_strength";
    private static readonly StringName PulseEnabledParam = "pulse_enabled";
    private static readonly StringName PulseRadiusDeltaParam = "pulse_radius_delta";
    private static readonly StringName PulseStrengthDeltaParam = "pulse_strength_delta";
    private static readonly StringName PulsesPerSecondParam = "pulses_per_second";
    private static readonly StringName MaskEnabledParam = "mask_enabled";
    private static readonly StringName MaskProgressParam = "mask_progress";
    private static readonly StringName MaskAxisParam = "mask_axis";
    private static readonly StringName MaskOriginParam = "mask_origin";
    private static readonly StringName DissolveEnabledParam = "dissolve_enabled";
    private static readonly StringName DissolveValueParam = "dissolve_value";
    private static readonly StringName DissolveTextureParam = "dissolve_texture";
    private static readonly StringName StretchEnabledParam = "stretch_enabled";
    private static readonly StringName StretchAmountParam = "stretch_amount";
    private static readonly StringName StretchDirectionParam = "stretch_direction";

    public static readonly NodePath BaseTintProperty = ShaderParameter(BaseTintParam);
    public static readonly NodePath LocalRectProperty = ShaderParameter(LocalRectParam);
    public static readonly NodePath GlowEnabledProperty = ShaderParameter(GlowEnabledParam);
    public static readonly NodePath GlowColorProperty = ShaderParameter(GlowColorParam);
    public static readonly NodePath GlowRadiusProperty = ShaderParameter(GlowRadiusParam);
    public static readonly NodePath GlowStrengthProperty = ShaderParameter(GlowStrengthParam);
    public static readonly NodePath PulseEnabledProperty = ShaderParameter(PulseEnabledParam);
    public static readonly NodePath PulseRadiusDeltaProperty = ShaderParameter(PulseRadiusDeltaParam);
    public static readonly NodePath PulseStrengthDeltaProperty = ShaderParameter(PulseStrengthDeltaParam);
    public static readonly NodePath PulsesPerSecondProperty = ShaderParameter(PulsesPerSecondParam);
    public static readonly NodePath MaskEnabledProperty = ShaderParameter(MaskEnabledParam);
    public static readonly NodePath MaskProgressProperty = ShaderParameter(MaskProgressParam);
    public static readonly NodePath MaskAxisProperty = ShaderParameter(MaskAxisParam);
    public static readonly NodePath MaskOriginProperty = ShaderParameter(MaskOriginParam);
    public static readonly NodePath DissolveEnabledProperty = ShaderParameter(DissolveEnabledParam);
    public static readonly NodePath DissolveValueProperty = ShaderParameter(DissolveValueParam);
    public static readonly NodePath DissolveTextureProperty = ShaderParameter(DissolveTextureParam);
    public static readonly NodePath StretchEnabledProperty = ShaderParameter(StretchEnabledParam);
    public static readonly NodePath StretchAmountProperty = ShaderParameter(StretchAmountParam);
    public static readonly NodePath StretchDirectionProperty = ShaderParameter(StretchDirectionParam);
    public static readonly NodePath BaseTintAlphaProperty = $"{BaseTintProperty}:a";
    public static readonly NodePath GlowColorAlphaProperty = $"{GlowColorProperty}:a";

    public ShaderMaterial Material { get; }

    private VisualEffects(ShaderMaterial material)
    {
        Material = material;
    }

    public Color BaseTint
    {
        get => (Color)Material.GetShaderParameter(BaseTintParam);
        set => Material.SetShaderParameter(BaseTintParam, value);
    }

    public Vector4 LocalRect
    {
        get => (Vector4)Material.GetShaderParameter(LocalRectParam);
        set => Material.SetShaderParameter(LocalRectParam, value);
    }

    public bool GlowEnabled
    {
        get => (bool)Material.GetShaderParameter(GlowEnabledParam);
        set => Material.SetShaderParameter(GlowEnabledParam, value);
    }

    public Color GlowColor
    {
        get => (Color)Material.GetShaderParameter(GlowColorParam);
        set => Material.SetShaderParameter(GlowColorParam, value);
    }

    public float GlowRadius
    {
        get => (float)Material.GetShaderParameter(GlowRadiusParam);
        set => Material.SetShaderParameter(GlowRadiusParam, value);
    }

    public float GlowStrength
    {
        get => (float)Material.GetShaderParameter(GlowStrengthParam);
        set => Material.SetShaderParameter(GlowStrengthParam, value);
    }

    public bool PulseEnabled
    {
        get => (bool)Material.GetShaderParameter(PulseEnabledParam);
        set => Material.SetShaderParameter(PulseEnabledParam, value);
    }

    public float PulseRadiusDelta
    {
        get => (float)Material.GetShaderParameter(PulseRadiusDeltaParam);
        set => Material.SetShaderParameter(PulseRadiusDeltaParam, value);
    }

    public float PulseStrengthDelta
    {
        get => (float)Material.GetShaderParameter(PulseStrengthDeltaParam);
        set => Material.SetShaderParameter(PulseStrengthDeltaParam, value);
    }

    public float PulsesPerSecond
    {
        get => (float)Material.GetShaderParameter(PulsesPerSecondParam);
        set => Material.SetShaderParameter(PulsesPerSecondParam, value);
    }

    public bool MaskEnabled
    {
        get => (bool)Material.GetShaderParameter(MaskEnabledParam);
        set => Material.SetShaderParameter(MaskEnabledParam, value);
    }

    public float MaskProgress
    {
        get => (float)Material.GetShaderParameter(MaskProgressParam);
        set => Material.SetShaderParameter(MaskProgressParam, value);
    }

    public MaskAxis MaskAxis
    {
        get => (MaskAxis)(int)Material.GetShaderParameter(MaskAxisParam);
        set => Material.SetShaderParameter(MaskAxisParam, (int)value);
    }

    public MaskOrigin MaskOrigin
    {
        get => (MaskOrigin)(int)Material.GetShaderParameter(MaskOriginParam);
        set => Material.SetShaderParameter(MaskOriginParam, (int)value);
    }

    public bool DissolveEnabled
    {
        get => (bool)Material.GetShaderParameter(DissolveEnabledParam);
        set => Material.SetShaderParameter(DissolveEnabledParam, value);
    }

    public float DissolveValue
    {
        get => (float)Material.GetShaderParameter(DissolveValueParam);
        set => Material.SetShaderParameter(DissolveValueParam, value);
    }

    public Texture2D? DissolveTexture
    {
        get => Material.GetShaderParameter(DissolveTextureParam).As<Texture2D>();
        set => Material.SetShaderParameter(DissolveTextureParam, value is null ? default(Variant) : value);
    }

    public bool StretchEnabled
    {
        get => (bool)Material.GetShaderParameter(StretchEnabledParam);
        set => Material.SetShaderParameter(StretchEnabledParam, value);
    }

    public float StretchAmount
    {
        get => (float)Material.GetShaderParameter(StretchAmountParam);
        set => Material.SetShaderParameter(StretchAmountParam, value);
    }

    public Vector2 StretchDirection
    {
        get => (Vector2)Material.GetShaderParameter(StretchDirectionParam);
        set => Material.SetShaderParameter(StretchDirectionParam, value);
    }

    public void SetLocalRect(Rect2 rect)
    {
        LocalRect = new Vector4(rect.Position.X, rect.Position.Y, rect.Size.X, rect.Size.Y);
    }

    public static VisualEffects Attach(CanvasItem node)
    {
        if (node.Material != null)
        {
            throw new InvalidOperationException($"'{node.Name}' already has a material.");
        }

        var material = new ShaderMaterial
        {
            Shader = node is CanvasGroup ? CanvasGroupShader : TextureShader
        };

        // Initialize shader defaults for getters and TweenProperty.
        material.GetPropertyList();
        node.Material = material;
        node.UseParentMaterial = false;
        return new VisualEffects(material);
    }
}
