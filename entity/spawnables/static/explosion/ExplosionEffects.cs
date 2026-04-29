public partial class ExplosionEffects : CanvasLayer
{

    public static ExplosionEffects Instance { get; private set; } = null!;

    public ExplosionEffects()
    {
        Instance = this;
    }

    public ExplosionEffect PlayEffect(Explosion explosion)
    {
        var effect = ExplosionEffect.Create(explosion);
        AddChild(effect);
        return effect;
    }

}
