public partial class PlayerHealContainer : HBoxContainer
{
    public override void _Ready()
    {
        var label = new Label()
        {
            Text = "Heal"
        };
        AddChild(label);

        var heal1 = new Button()
        {
            Text = "1"
        };
        heal1.Pressed += () => Heal(1);
        AddChild(heal1);
        
        var heal10 = new Button()
        {
            Text = "10"
        };
        heal10.Pressed += () => Heal(10);
        AddChild(heal10);
        
        var heal100 = new Button()
        {
            Text = "100"
        };
        heal100.Pressed += () => Heal(100);
        AddChild(heal100);
    }

    private void Heal(float amount)
    {
        Player.FindPlayer()?.HealthController.ChangeHealth(amount);
    }
}
