public abstract class EnemyType
{

    public EnemyType()
    {
        EnemyTypeRegistry.Types.Add(this);
    }

    public abstract PackedScene Scene { get; }

    public abstract string Name { get; }

}