public static class DissolveEffect
{

    private static readonly string DissolveValueParam = ShaderParameter("dissolve_value");

    private static readonly ShaderMaterial Resource = GD.Load<ShaderMaterial>("uid://ef46pfecv73h");
    
    /// <summary>
    /// Applies a dissolve effect to the sprite by changing its material. After the sprite is dissolved,
    /// the node is freed. 
    /// </summary>
    /// <param name="node">Node to free after the sprite has done dissolving</param>
    /// <param name="sprite">Sprite to apply dissolve effect</param>
    /// <param name="duration">Duration of the effect, 0.5s by default</param>
    public static void Dissolve(Node node, Sprite2D sprite, float duration = 0.5f)
    {
        sprite.Material = (ShaderMaterial)Resource.Duplicate();

        var tween = node.CreateTween();
        tween.TweenProperty(sprite.Material, DissolveValueParam, 0, duration);
        tween.Finished += node.QueueFree;
    }

}
