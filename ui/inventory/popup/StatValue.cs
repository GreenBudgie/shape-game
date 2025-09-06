public partial class StatValue : Label
{
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dnqq1umhkun6x");

    public static StatValue Create(ModuleStat stat)
    {
        var node = Scene.Instantiate<StatValue>();
        node.Text = stat.FormattedValue;
        return node;
    }
    
}
