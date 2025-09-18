public partial class EnemyRectangleProjectileParticles : GpuParticles2D
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://co10tbovvd43l");

    private EnemyRectangleProjectile _owner = null!;

    public static EnemyRectangleProjectileParticles Create(EnemyRectangleProjectile owner)
    {
        var node = Scene.Instantiate<EnemyRectangleProjectileParticles>();
        node._owner = owner;
        ShapeGame.Instance.AddChild(node);
        return node;
    }

    public override void _Ready()
    {
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
        _owner.TreeExiting += Remove;
    }

    public override void _Process(double delta)
    {
        if (IsInstanceValid(_owner))
        {
            GlobalPosition = _owner.GlobalPosition;
        }
    }

    private void Remove()
    {
        if (!IsInsideTree())
        {
            return;
        }

        Emitting = false;
        GetTree().CreateTimer(Lifetime).Timeout += QueueFree;
    }
}