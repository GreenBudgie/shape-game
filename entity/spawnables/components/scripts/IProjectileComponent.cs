public interface IProjectileComponent
{

    public Node Node { get; }

    public void Prepare(ShotContext context)
    {
    }

    public void Apply(ShotContext context)
    {
    }

}