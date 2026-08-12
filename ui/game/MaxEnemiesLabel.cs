public partial class MaxEnemiesLabel : Label
{
    
    private bool _prevIsMaxEnemiesReached;

    public override void _Ready()
    {
        Modulate = Modulate.AsTransparent();
    }

    public override void _Process(double delta)
    {
        var isMaxEnemies = LevelManager.Instance.IsMaxEnemiesReached();
        if (_prevIsMaxEnemiesReached == isMaxEnemies)
        {
            return;
        }
        
        _prevIsMaxEnemiesReached = isMaxEnemies;
        if (isMaxEnemies)
        {
            DoShow();
        }
        else
        {
            DoHide();
        }
    }

    private Tween? _tween;

    private void DoShow()
    {
        _tween?.Kill();
        _tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        _tween.FadeIn(this, 0.25f);
    }

    private void DoHide()
    {
        _tween?.Kill();
        _tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        _tween.FadeOut(this, 0.25f);
    }
}
