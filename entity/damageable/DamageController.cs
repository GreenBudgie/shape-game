public partial class DamageController : Node2D
{
    
    [Signal] public delegate void DamageReceivedEventHandler(float damage);

    [Export] private Node2D _target = null!;
    [Export] private Sprite2D _sprite = null!;

    public override void _Ready()
    {
        base._Ready();
    }
}
