public partial class BurningEntityEffect(Entity owner, float duration) : EntityEffect(owner, duration)
{

    private const float BurnDamage = 1f;
    private const float BurnDamageFrequencySec = 1f;
    
    public override EntityEffectType Type => EntityEffectTypeRegistry.Burning;

    private FireDisplay _fireDisplay = null!;
    private double _burnTimer;

    protected override void OnAttach()
    {
        _fireDisplay = FireDisplay.Attach(OwnerEntity);
    }

    protected override void OnUpdate(double delta)
    {
        _burnTimer += delta;
        if (_burnTimer >= BurnDamageFrequencySec)
        {
            _burnTimer = 0;
            OwnerEntity.HealthController.Damage(BurnDamage);
        }
    }

    protected override void OnRemove()
    {
        _fireDisplay.Stop();
        QueueFree();
    }
}