public static class GodotConstants
{

    private const string ShaderParameterPrefix = "shader_parameter";
    
    // Cached node paths for common properties to use in tweens
    public static readonly NodePath PositionProperty = "position";
    public static readonly NodePath RotationProperty = "rotation";
    public static readonly NodePath RotationDegreesProperty = "rotation_degrees";
    public static readonly NodePath ScaleProperty = "scale";
    public static readonly NodePath ModulateProperty = "modulate";
    public static readonly NodePath ModulateAlphaProperty = "modulate:a";
    public static readonly NodePath ValueProperty = "value";
    public static readonly NodePath OffsetProperty = "offset";
    public static readonly NodePath OffsetTransformPosition = "offset_transform_position";
    public static readonly NodePath OffsetTransformRotation = "offset_transform_rotation";
    public static readonly NodePath OffsetTransformScale = "offset_transform_scale";

    public static string ShaderParameter(string parameter)
    {
        return $"{ShaderParameterPrefix}/{parameter}";
    }

}