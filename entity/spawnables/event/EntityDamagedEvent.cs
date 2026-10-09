public class EntityDamagedEvent(Entity entity, float damage)
{

    public Entity Entity { get; } = entity;

    public float Damage { get; } = damage;

}