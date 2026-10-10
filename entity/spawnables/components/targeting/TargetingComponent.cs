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

        Target.HealthController.Connect(HealthController.SignalName.Destroyed, Callable.From(Remove));
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsInstanceValid(Target))
        {
            Remove();
            return;
        }

        const float homingSpeed = 2500f;
        const float speedChangeRate = 15000f;
        const float minTurnRate = 5f;
        const float maxTurnRate = 14f;

        var step = (float)delta;
        var velocity = _rigidBodySpawnable.LinearVelocity;
        var speed = velocity.Length();

        var directionToTarget = GlobalPosition.DirectionTo(Target.GlobalPosition);
        var direction = speed > 0 ? velocity / speed : directionToTarget;

        var distance = GlobalPosition.DistanceTo(Target.GlobalPosition);
        var angle = direction.AngleTo(directionToTarget);
        var turnRate = Clamp(speed / Max(distance, 1), minTurnRate, maxTurnRate);
        var turn = Clamp(angle, -turnRate * step, turnRate * step);

        var targetSpeed = Min(homingSpeed, maxTurnRate * distance / Max(2 * Abs(Sin(angle)), 0.05f));
        var desiredVelocity = direction.Rotated(turn) * MoveToward(speed, targetSpeed, speedChangeRate * step);
        _rigidBodySpawnable.ApplyCentralForce(_rigidBodySpawnable.Mass * (desiredVelocity - velocity) / step);
    }

    public override void _ExitTree()
    {
        Remove();
    }

    private void Remove()
    {
        if (IsQueuedForDeletion())
        {
            return;
        }
        
        Target = null;

        if (IsInstanceValid(_particles))
        {
            _particles.Emitting = false;
        }
        
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