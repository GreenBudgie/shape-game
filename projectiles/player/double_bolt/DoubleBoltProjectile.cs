public partial class DoubleBoltProjectile : Projectile
{

    public override void _Ready()
    {
        BodyEntered += HandleBodyEntered;
    }

    public override float GetDamage()
    {
        return 3;
    }

    private void HandleBodyEntered(Node body)
    {
        if (body is not Enemy enemy)
        {
            return;
        }
        
        QueueFree();
    }
    
}