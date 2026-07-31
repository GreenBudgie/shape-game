public partial class WorldModule : RigidBody2D
{
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://d2oibytyvv11i");

    public Module Module { get; private set; } = null!;
    
    public static WorldModule Create(Module module)
    {
        var worldModule = Scene.Instantiate<WorldModule>();
        worldModule.Module = module;
        return worldModule;
    }

    public override void _Ready()
    {
        var fillSprite = GetNode<Sprite2D>("FillSprite");
        fillSprite.Texture = Module.Shape.FillTexture;
        fillSprite.Modulate = ColorScheme.DarkOrange;
        
        var outlineSprite = GetNode<Sprite2D>("OutlineSprite");
        outlineSprite.Texture = Module.Shape.FillTexture;
        outlineSprite.Modulate = Module.Color;
        
        var moduleSprite = GetNode<Sprite2D>("ModuleSprite");
        moduleSprite.Texture = Module.Texture;

        var shape = new RectangleShape2D();
        shape.Size = Module.Shape.PixelSize;
                
        var collisionShape = GetNode<CollisionShape2D>("CollisionShape");
        collisionShape.Shape = shape;
    }
    
}