public partial class ToggleLevelProgressButton : Button
{
    public override void _Pressed()
    {
        var progress = LevelManager.Instance.ToggleLevelProgress();
        if (progress)
        {
            Text = "Stop Level\nProgress";
        }
        else
        {
            Text = "Start Level\nProgress";
        }
    }
}
