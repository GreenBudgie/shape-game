public partial class ModuleInfo : Control
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://c47f74u04f8m2");

    private Module _module = null!;

    private RichTextLabel _title = null!;
    private RichTextLabel _description = null!;
    private VBoxContainer _statsContainer = null!;
    
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
    }

    public override void _Process(double delta)
    {
        GlobalPosition = GetGlobalMousePosition();
    }

    public void Remove()
    {
        QueueFree();
    }

    private void AddStat(StatInfo stat)
    {
        _statsContainer.AddChild(StatContainer.Create(stat));
    }

}