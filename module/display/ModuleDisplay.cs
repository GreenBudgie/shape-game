public partial class ModuleDisplay : Node2D
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://djv8r6n40ajfq");

    public Sprite2D Fill { get; private set; } = null!;
    public Sprite2D Outline { get; private set; } = null!;
    public Sprite2D Module { get; private set; } = null!;
    
    private ModuleType _type = null!;
    
    public static ModuleDisplay Create(ModuleType moduleType)
    {
        var node = Scene.Instantiate<ModuleDisplay>();
        node._type = moduleType;
        return node;
    }
    
    public override void _Ready()
    {
        Fill = GetNode<Sprite2D>("FillSprite");
        Fill.Texture = _type.Shape.FillTexture;
        Fill.SelfModulate = ColorScheme.DarkOrange;
        
        Outline = GetNode<Sprite2D>("OutlineSprite");
        Outline.Texture = _type.Shape.OutlineTexture;
        Outline.SelfModulate = _type.Color;
        
        Module = GetNode<Sprite2D>("ModuleSprite");
        Module.Texture = _type.Texture;
    }
}
