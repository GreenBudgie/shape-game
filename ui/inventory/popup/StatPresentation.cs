public partial class StatPresentation : HBoxContainer
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cmymkg60myemv");

    public static StatPresentation Create(ModuleStat stat)
    {
        var node = Scene.Instantiate<StatPresentation>();
        var statIcon = node.GetNode<TextureRect>("StatIcon");
        var nameLabel = node.GetNode<Label>("NameLabel");
        statIcon.Texture = stat.Icon;
        nameLabel.Text = stat.Name;
        return node;
    }

}
