/// <summary>
/// Source of incoming damage. Sources need to be specified to allow multiple damage sources to inflict damage at
/// the same frame. Otherwise, invulnerability frames would cancel all but first dealt damage.
/// </summary>
public enum DamageSource
{
    
    Projectile,
    Fire,
    Explosion,
    Contact
    
}