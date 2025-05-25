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
            var volume = 1.5f - speed / 1000f;
            var sound = SoundManager.Instance.PlayPositionalSound(this, _hitWallSound);
            sound.PitchScale = Clamp(pitch, 0.75f, 1.25f);
            sound.VolumeLinear = Clamp(volume, 0.2f, 1);
            return;
        }
        
        if (body is not Player)
        {
            return;
        }
        
        QueueFree();
    }
    
    
}