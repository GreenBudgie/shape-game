using System;

public partial class HealthComponent : Node
{

    [Signal]
    public delegate void HealthDepletedEventHandler();
    
    [Signal]
    public delegate void HealthChangedEventHandler(double previousHealth, double newHealth);

    [Export] private double _maxHealth;

    private double _health;

    public override void _Ready()
    {
        _health = _maxHealth;
    }
    
    public void SetHealth(double newHealth)
    {
        var previousHealth = _health;
        _health = Clamp(newHealth, 0, _maxHealth);
        EmitSignal(SignalName.HealthChanged, previousHealth, _health);
        if (_health <= 0)
        { 
            HandleHealthDepletion();
        }
    }

    public void AddHealth(double healthToAdd)
    {
        if (healthToAdd == 0)
        {
            return;
        }
        SetHealth(_health + healthToAdd);
    }

    private void HandleHealthDepletion()
    {
        EmitSignal(SignalName.HealthDepleted);
    }

}