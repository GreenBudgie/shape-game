public interface IProjectile<out T> where T : Node2D
{
    
    public T Node { get; }
    
}