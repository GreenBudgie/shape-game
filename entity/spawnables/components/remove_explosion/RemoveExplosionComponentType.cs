public class RemoveExplosionComponentType : ComponentType
{
    public override RemoveExplosionComponent CreateComponent(ISpawnable spawnable)
    {
        return new RemoveExplosionComponent { Spawnable = spawnable };
    }
}