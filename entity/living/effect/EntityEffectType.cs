public abstract class EntityEffectType
{

    public EntityEffectType()
    {
        EntityEffectTypeRegistry.Types.Add(this);
    }
    
    public abstract string Name { get; }
    
    public abstract Texture2D Icon { get; }

    public abstract EntityEffect CreateEffect(Entity owner, float duration);

}