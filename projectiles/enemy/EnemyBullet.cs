public abstract partial class EnemyBullet : RigidBody2D
{
    
    public override void _Ready()
    {
        BodyEntered += HandleBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (this.IsOutsidePlayableArea())
        {
            QueueFree();
        }
    }
    
    private void HandleBodyEntered(Node body)
    {
        QueueFree();
    }
    
}