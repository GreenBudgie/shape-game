public partial class EnemySquareProjectile : RigidBody2D
{

    [Export] private AudioStream _hitWallSound = null!;
    
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
        if (body.IsInGroup("level_walls"))
        {
            var speed = LinearVelocity.Length();
            if (speed <= 100)
            {
                return;
            }

            var pitch = speed / 1000f + 0.75f;
            var sound = SoundManager.Instance.PlayPositionalSound(this, _hitWallSound);
            sound.PitchScale = Clamp(pitch, 0.75f, 1.25f);
            return;
        }
        
        if (body is not Player)
        {
            return;
        }
        
        QueueFree();
    }
    
    
}