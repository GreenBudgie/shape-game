/// <summary>
/// Used to respond to fast player movements, when default physics cannot handle it.
///
/// All collisions with player should be handled via this interface, and not via BodyEntered signal!
///
/// Also, you need to enable collision layer number 10: player_collider.
/// </summary>
public interface IPlayerCollisionDetector
{

    public void CollideWithPlayer(Player player);

}