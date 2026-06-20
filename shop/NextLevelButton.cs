public partial class NextLevelButton : TextureRect
{
    public override void _Process(double delta)
    {
        var player = Player.FindPlayer();
        if (player == null)
        {
            return;
        }

        var center = player.GetGlobalCenter();
        var rect = GetGlobalRect();

        if (!rect.HasPoint(center))
        {
            return;
        }
        
        if (Input.IsActionJustPressed("inventory_left_click"))
        {
            LevelManager.Instance.StartNextLevel();
        }
    }
}
