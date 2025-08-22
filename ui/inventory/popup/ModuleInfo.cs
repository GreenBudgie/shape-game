public partial class ModuleInfo : Control
{

    private const float MaxWidth = 840;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://c47f74u04f8m2");
    
    private Module _module = null!;

    private RichTextLabel _title = null!;
    private RichTextLabel _description = null!;
    private VBoxContainer _statsContainer = null!;

    private Tween? _scaleTween;
    private bool _isRemoving;
    
    public static ModuleInfo Create(Module module)
    {
        var node = Scene.Instantiate<ModuleInfo>();
        node._module = module;
        return node;
    }

    public override void _Ready()
    {
        _title = GetNode<RichTextLabel>("%Title");
        _description = GetNode<RichTextLabel>("%Description");
        _statsContainer = GetNode<VBoxContainer>("%StatsContainer");

        if (_module.Stats.Count == 0)
        {
            _statsContainer.QueueFree();
            return;
        }

        _title.Clear();
        _title.PushBold();
        _title.AppendText(_module.Name);
        _title.Pop();
        
        _description.Text = _module.Description;
        
        foreach (var stat in _module.Stats)
        {
            AddStat(stat.Info);
        }

        Scale = Vector2.Zero;
        PlayScaleEffect(finalScale: 1, xDuration: 0.1f, yDuration: 0.2f);
    }

    public override void _Process(double delta)
    {
        if (_isRemoving)
        {
            return;
        }
        
        GlobalPosition = GetGlobalMousePosition();
    }

    public void Remove()
    {
        _isRemoving = true;
        var tween = PlayScaleEffect(finalScale: 0, xDuration: 0.2f, yDuration: 0.1f);
        tween.Finished += QueueFree;
    }

    private Tween PlayScaleEffect(float finalScale, float xDuration, float yDuration)
    {
        _scaleTween?.Kill();
        _scaleTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        _scaleTween.TweenProperty(this, "scale:x", finalScale, xDuration);
        _scaleTween.Parallel().TweenProperty(this, "scale:y", finalScale, yDuration);
        return _scaleTween;
    }

    private void AddStat(StatInfo stat)
    {
        _statsContainer.AddChild(StatContainer.Create(stat));
    }

}