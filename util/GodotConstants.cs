public static class GodotConstants
{

    private const string ShaderParameterPrefix = "shader_parameter";

    public static string ShaderParameter(string parameter)
    {
        return $"{ShaderParameterPrefix}/{parameter}";
    }

}