public static class ProjectileHelper
{

    /// <summary>
    /// All IProjectile interfaces should inherit Node2D, but it's not possible to easily handle via godot.
    /// So, this helper method is used to just cast an interface to a node.
    /// </summary>
    public static Node2D ToNode(this IProjectile projectile)
    {
        return (Node2D)projectile;
    }

}