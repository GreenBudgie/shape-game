/// <summary>
/// How the ripples of a <see cref="ForceField"/> move. Values match the ripple_mode uniform of the shader.
/// </summary>
public enum ForceFieldRippleMode
{
    /// <summary>
    /// Several sets of waves cross the field from different sides, like water stirred from all sides.
    /// </summary>
    Waves = 0,

    /// <summary>
    /// Rings spread from the center, like the field pushes things away.
    /// </summary>
    Outward = 1,

    /// <summary>
    /// Rings run into the center, like the field pulls things in.
    /// </summary>
    Inward = 2
}
