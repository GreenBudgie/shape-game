using System;

public partial class IgnitionComponent : Component
{
    public override ComponentType Type => ComponentTypeRegistry.Ignition;

    public override void Prepare(SpawnableContext context)
    {
        Spawnable.EntityDamaged += OnEntityDamaged;
    }

    public override void Apply(SpawnableContext context)
    {
        FireDisplay.Attach((IAreaAware)Spawnable);
    }

    private void OnEntityDamaged(EntityDamagedEvent @event)
    {
        var burningSeconds = Spawnable.Context.CalculateStat<BurningStat>();
        if (burningSeconds > 0)
        {
            @event.Entity.EffectController.AddEffect(EntityEffectTypeRegistry.Burning, burningSeconds);
        }
    }
    
}