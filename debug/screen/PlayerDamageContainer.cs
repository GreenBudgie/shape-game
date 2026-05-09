public partial class PlayerDamageContainer : HBoxContainer
{
    public override void _Ready()
    {
        var label = new Label()
        {
            Text = "Damage"
        };
        AddChild(label);

        var damage1 = new Button()
        {
            Text = "1"
        };
        damage1.Pressed += () => Damage(1);
        AddChild(damage1);

        var damage10 = new Button()
        {
            Text = "10"
        };
        damage10.Pressed += () => Damage(10);
        AddChild(damage10);

        var damage100 = new Button()
        {
            Text = "100"
        };
        damage100.Pressed += () => Damage(100);
        AddChild(damage100);
    }

    private void Damage(float amount)
    {
        Player.FindPlayer()?.HealthController.Damage(amount);
    }
}