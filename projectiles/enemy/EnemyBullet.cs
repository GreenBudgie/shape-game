public abstract partial class EnemyBullet : Projectile
{
    
    public override void _Ready()
    {
        BodyEntered += HandleBodyEntered;
    }

    
    private void HandleBodyEntered(Node body)
    {
        QueueFree();
    }
    
}