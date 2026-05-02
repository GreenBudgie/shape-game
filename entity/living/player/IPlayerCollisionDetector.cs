/// <summary>
/// Used to respond to fast player movements, when default physics cannot handle it.
///
/// All collisions with player should be handled via this interface, and not via BodyEntered/BodyExited signals!
///
/// Also, you need to enable collision layer number 10: player_collider.
/// </summary>
public interface IPlayerCollisionDetector
{

    /// <summary>
    /// Called when current body collides with a player for the first time. NOT called repeatedly when inside player
    /// collision detector shape
    /// </summary>
    public void PlayerShapeEntered(Player player);
    
    /// <summary>
    /// Called when current body leaves the player collider shape. Can only be called if PlayerShapeEnteres
    /// has already been called
    /// </summary>
    public void PlayerShapeExited(Player player);

}