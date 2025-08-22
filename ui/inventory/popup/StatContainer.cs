public partial class StatContainer : HBoxContainer
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://t2fb5q106t4q");

    public static StatContainer Create(ModuleStat stat)
    {
        var node = Scene.Instantiate<StatContainer>();
        node.GetNode<TextureRect>("StatIcon").Texture = stat.Icon;
        node.GetNode<Label>("StatLabel").Text = stat.FormattedValue;
        return node;
    }
    
}
