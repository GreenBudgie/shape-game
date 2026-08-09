public abstract class PolysteroidSize
{

    public PolysteroidSize()
    {
        PolysteroidSizeRegistry.Sizes.Add(this);
    }
    
    public abstract Texture2D Texture { get; }
    
    public abstract PackedScene CollisionPolygonScene { get; }
    
    public abstract PackedScene AreaScene { get; }
    
    public abstract float Health { get; }
    
    public abstract float Gravity { get; }
    
    public abstract float Mass { get; }
    
}