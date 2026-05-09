public partial class EndLevelButton : Button
{
    public override void _Pressed()
    {
        LevelManager.Instance.ForceEndLevel();
    }
}