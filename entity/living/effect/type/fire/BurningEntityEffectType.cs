public class BurningEntityEffectType : EntityEffectType
{
    public override string Name => "Burning";
    
    public override Texture2D Icon => GD.Load<Texture2D>("uid://cnpef8qs1xocq");
    
    public override EntityEffect CreateEffect(Entity owner, float duration)
    {
        return new BurningEntityEffect(owner, duration);
    }
    
}