public partial class EnemySquareProjectile : RigidBody2D
{

    public override void _Ready()
    {
        BodyEntered += HandleBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (this.IsBelowPlayableArea())
        {
            QueueFree();
        }
    }
    
    private void HandleBodyEntered(Node body)
    {
        if (body is not Player)
        {
            return;
        }
        
        QueueFree();
    }
    
    
}