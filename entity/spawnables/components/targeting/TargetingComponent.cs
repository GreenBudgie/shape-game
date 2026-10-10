using System;
using System.Linq;

public partial class TargetingComponent : Component
{
    public override ComponentType Type => ComponentTypeRegistry.Targeting;
    
    public Entity? Target { get; private set; }

    private RigidBody2D _rigidBodySpawnable = null!;

    private TargetDisplay? _display;
    private TrailParticles _particles = null!;

    public override void Prepare(SpawnableContext context)
    {
        if (Spawnable.Node is not RigidBody2D rigidBody)
        {
            throw new Exception("TargetingComponent is only applicable to RigidBodies");
        }

        _rigidBodySpawnable = rigidBody;
        _particles = TrailParticles.Create(rigidBody)
            .WithTexture(ParticleTextures.Square)
            .WithScale(0.4f, 0.1f)
            .Color(ColorScheme.Orange)
            .Spawn();
    }

    public override void Apply(SpawnableContext context)
    {
        SelectTarget();
        if (Target == null)
        {
            QueueFree();
            return;
        }

        _display = new TargetDisplay();
        ShapeGame.Instance.AddChild(_display);
        _display.AttachToEntity(Target);

        Target.HealthController.Destroyed += Remove;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsInstanceValid(Target))
        {
            Remove();
            return;
        }

        const float forceStrength = 1000f;
        const float maxForce = 1000f;

        var distance = GlobalPosition.DistanceTo(Target.GlobalPosition);
        var directionToTarget = GlobalPosition.DirectionTo(Target.GlobalPosition);

        var force = directionToTarget * forceStrength;
        _rigidBodySpawnable.ApplyCentralForce(force.LimitLength(maxForce));
    }

    public override void _ExitTree()
    {
        Remove();
    }

    private void Remove()
    {
        Target = null;
        _particles.Emitting = false;
        
        if (IsInstanceValid(_display))
        {
            _display.Remove();
        }

        QueueFree();
    }

    private void SelectTarget()
    {
        if (Spawnable.Context.OriginalSource is not Player)
        {
            Target = Player.FindPlayer();
            return;
        }
        
        var aliveEnemy = EnemyManager.Instance.GetNonEnvironmentalAliveEnemies()
            .MinBy(enemy => enemy.GlobalPosition.DistanceSquaredTo(Spawnable.Node.GlobalPosition));

        Target = aliveEnemy;
    }
}