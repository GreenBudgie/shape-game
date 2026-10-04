using System.Collections.Generic;

public partial class EntityEffectController(Entity owner) : Node2D
{
    
    public Entity OwnerEntity { get; } = owner;

    private Dictionary<EntityEffectType, EntityEffect> ActiveEffects { get; } = [];
    
    /// <summary>
    /// Adds an effect to the entity. If effect of this type is already applied, it can be prolonged to
    /// new duration, but only if current effect has less remaining duration.
    /// </summary>
    public void AddEffect(EntityEffectType type, float duration)
    {
        var activeEffect = GetActiveEffect(type);
        if (activeEffect != null)
        {
            if (activeEffect.Duration >= duration)
            {
                return;
            }

            activeEffect.Duration = duration;
            return;
        }

        var newEffect = type.CreateEffect(OwnerEntity, duration);
        AddChild(newEffect);
        newEffect.Connect(EntityEffect.SignalName.Removed, Callable.From(() => HandleEffectRemoval(newEffect)));
        
        ActiveEffects.Add(type, newEffect);
    }

    public EntityEffect? GetActiveEffect(EntityEffectType type)
    {
        return ActiveEffects.GetValueOrDefault(type);
    }

    private void HandleEffectRemoval(EntityEffect effect)
    {
        var type = effect.Type;
        var removed = ActiveEffects.Remove(type);
        if (!removed)
        {
            GD.PushError("An effect that wasn't registered in ActiveEffects was just removed. This shouldn't happen!");
        }
    }

}