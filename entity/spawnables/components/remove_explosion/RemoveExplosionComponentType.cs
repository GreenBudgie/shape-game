public class RemoveExplosionComponentType : ComponentType
{
    public override RemoveExplosionComponent CreateComponent(ISpawnable spawnable)
    {
        return new RemoveExplosionComponent { Spawnable = spawnable };
    }

    public override bool Supports(ISpawnable spawnable)
    {
        return !spawnable.IsProxy;
    }
}