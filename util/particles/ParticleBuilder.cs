public abstract partial class ParticleBuilder<T> : GpuParticles2D where T : ParticleBuilder<T>
{
    protected ParticleProcessMaterial ProcMaterial;

    public ParticleBuilder()
    {
        ProcMaterial = (ParticleProcessMaterial)ProcessMaterial;
    }

    public override void _Ready()
    {
        Finished += QueueFree;
        Restart();
    }

    public void Spawn()
    {
        ShapeGame.Instance.AddChild(this);
    }

    public void SpawnDeferred()
    {
        Callable.From(Spawn).CallDeferred();
    }

    public T RectangleShape(Rect2 area)
    {
        ProcMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
        var halfRectSize = area.Size / 2f;
        ProcMaterial.EmissionBoxExtents = new Vector3(halfRectSize.X, halfRectSize.Y, 1);
        return (T)this;
    }

    public T CircleShape(float radius)
    {
        ProcMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere;
        ProcMaterial.EmissionSphereRadius = radius;
        return (T)this;
    }

    public T Color(Color color)
    {
        Modulate = color;
        return (T)this;
    }

    public T WithTexture(Texture2D texture)
    {
        Texture = texture;
        return (T)this;
    }

    public T WithAmount(int amount, int amountDelta = 0)
    {
        Amount = RandomUtils.DeltaRange(amount, amountDelta);
        return (T)this;
    }

    public T WithAmountPerPixel(float amountPerPixel)
    {
        var area = 0f;
        if (ProcMaterial.EmissionShape == ParticleProcessMaterial.EmissionShapeEnum.Box)
        {
            area = ProcMaterial.EmissionBoxExtents.X * ProcMaterial.EmissionBoxExtents.Y * 4;
        }
        if (ProcMaterial.EmissionShape == ParticleProcessMaterial.EmissionShapeEnum.Sphere)
        {
            area = Pi * ProcMaterial.EmissionSphereRadius * ProcMaterial.EmissionSphereRadius;
        }

        if (area == 0)
        {
            GD.PushWarning($"Emission shape {ProcMaterial.EmissionShape} is not supported");
        }

        Amount = RoundToInt(Sqrt(area) * amountPerPixel);
        return (T)this;
    }

    public T WithScale(float scale, float scaleDelta = 0)
    {
        ProcMaterial.ScaleMin = scale - scaleDelta;
        ProcMaterial.ScaleMax = scale + scaleDelta;
        return (T)this;
    }
    
    public T WithLifetime(float lifetime)
    {
        Lifetime = lifetime;
        return (T)this;
    }

    public InheritVelocityConfiguration InheritVelocity(RigidBody2D body)
    {
        return new InheritVelocityConfiguration((T)this, body, ProcMaterial);
    }

    public class InheritVelocityConfiguration(T builder, RigidBody2D body, ParticleProcessMaterial procMaterial)
    {
        private float _velocitySpreadFactor = 0.08f;
        private float _minVelocity = 300f;
        private float _velocityDelta = 150f;
        private float _maxVelocity = 2000f;

        /// <summary>
        /// Sets the spread factor that scales with the rigid body’s velocity.
        /// Higher values reduce the spread at higher velocities.
        /// </summary>
        /// <param name="value">The velocity spread factor.</param>
        public InheritVelocityConfiguration VelocitySpreadFactor(float value)
        {
            _velocitySpreadFactor = value;
            return this;
        }

        /// <summary>
        /// Sets the minimum initial velocity of particles.
        /// </summary>
        /// <param name="value">The minimum velocity.</param>
        public InheritVelocityConfiguration MinVelocity(float value)
        {
            _minVelocity = value;
            return this;
        }

        /// <summary>
        /// Sets the delta added to the minimum velocity to compute the upper bound.
        /// </summary>
        /// <param name="value">The velocity delta.</param>
        public InheritVelocityConfiguration VelocityDelta(float value)
        {
            _velocityDelta = value;
            return this;
        }

        /// <summary>
        /// Sets the maximum initial velocity of particles.
        /// </summary>
        /// <param name="value">The maximum velocity.</param>
        public InheritVelocityConfiguration MaxVelocity(float value)
        {
            _maxVelocity = value;
            return this;
        }

        /// <summary>
        /// Applies the configuration to the particle process material and returns the parent builder.
        /// </summary>
        /// <returns>The parent builder for chaining.</returns>
        public T Configure()
        {
            var normalizedDir = body.LinearVelocity.Normalized();
            var velocityLen = body.LinearVelocity.Length();

            procMaterial.Spread = 180 - Clamp(velocityLen * _velocitySpreadFactor, 0, 180);
            procMaterial.Direction = new Vector3(normalizedDir.X, normalizedDir.Y, 0);

            var velocity = Clamp(velocityLen, _minVelocity, _maxVelocity);
            procMaterial.InitialVelocityMin = _minVelocity;
            procMaterial.InitialVelocityMax = Clamp(velocity, _minVelocity + _velocityDelta, _maxVelocity);

            return builder;
        }
    }
}