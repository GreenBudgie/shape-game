public partial class TrailParticles : ParticleBuilder<TrailParticles>
{
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://crj414c55niyf");

    private Node2D _owner = null!;
    
    public static TrailParticles Create(Node2D owner)
    {
        var node = Scene.Instantiate<TrailParticles>();
        node.GlobalPosition = owner.GlobalPosition;
        node._owner = owner;
        return node;
    }
    
    public override void _Ready()
    {
        _owner.TreeExiting += Remove;
    }

    public override void _Process(double delta)
    {
        if (IsInstanceValid(_owner))
        {
            GlobalPosition = _owner.GlobalPosition;
        }
    }

    public void Spawn()
    {
        ShapeGame.Instance.AddChild(this);
    }

    private void Remove()
    {
        Emitting = false;
        GetTree().CreateTimer(Lifetime).Timeout += QueueFree;
    }
    
}
