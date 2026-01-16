public partial class EndLevelButton : Button
{
    public override void _Ready()
    {
        ButtonDown += OnClick;
    }

    private void OnClick()
    {
        LevelManager.Instance.ForceEndLevel();
    }
}