namespace Enemies;

public abstract partial class Enemy : CharacterBody2D
{

    public float Health { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Health = GetMaxHealth();
    }

    public void Kill()
    {
        QueueFree();
    }

    public void Damage(float damage)
    {
        Modulate = new Color(255, 0, 0);
        Health -= damage;
        if (Health <= 0)
        {
            Kill();
        }
    }

    public abstract float GetMaxHealth();

}