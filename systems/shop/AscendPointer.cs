public partial class AscendPointer : Sprite2D
{
    
    [Export] private Area2D _ceilingArea = null!;
    
    public override void _Ready()
    {
        Modulate = Modulate.AsTransparent();
        
        GamePhaseManager.Instance.PhaseChanged += OnPhaseChange;
    }

    private Tween? _tween;

    private void OnPhaseChange(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            _tween?.Kill();
            _tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
            _tween.FadeIn(this, 0.5f);
        }
        
        if (phase == GamePhase.Level)
        {
            _tween?.Kill();
            _tween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
            _tween.FadeOut(this, 0.5f);
        }
    }
    
}
