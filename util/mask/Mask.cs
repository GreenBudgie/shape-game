public sealed class Mask
{
    
    private static readonly Shader Shader = GD.Load<Shader>("uid://b22gfqbbxeq1j");

    private static readonly StringName ProgressParam = "progress";
    public static readonly NodePath ProgressShaderParam = ShaderParameter(ProgressParam);
    
    public readonly ShaderMaterial Material;

    private Mask(ShaderMaterial material)
    {
        Material = material;
    }

    public Mask Axis(MaskAxis axis)
    {
        Material.SetShaderParameter("axis", (int)axis);
        return this;
    }

    public Mask Origin(MaskOrigin origin)
    {
        Material.SetShaderParameter("origin", (int)origin);
        return this;
    }

    public float Progress
    {
        get => (float)Material.GetShaderParameter(ProgressParam);
        set => Material.SetShaderParameter(ProgressParam, Clamp(value, 0f, 1f));
    }

    public static Mask Attach(CanvasItem node)
    {
        var material = new ShaderMaterial
        {
            Shader = Shader
        };

        node.Material = material;

        return new Mask(material);
    }
}