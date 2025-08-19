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
        
        foreach (var stat in _module.Stats)
        {
            AddStat(stat.Info);
        }
    }

    private void AddStat(StatInfo stat)
    {
        _statsContainer.AddChild(StatContainer.Create(stat));
    }

}