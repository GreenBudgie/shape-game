public partial class PointerArea(CollisionObject2D target) : Node2D
{

    private const float DelayBeforeRemovePointer = 0.2f;

    private CollisionObject2D _target = target;

    public Color PointerColor { get; set; } = Colors.White;
    
    /// <summary>
    /// Whether to show pointer when target is below the visible screen area
    /// </summary>
    public bool ShowBelow { get; set; } = true;
    
    /// <summary>
    /// Whether to show pointer when target is above the visible screen area
    /// </summary>
    public bool ShowAbove { get; set; } = true;

    /// <summary>
    /// Whether to only show pointer when target is moving towards the visible area
    /// </summary>
    public bool ShowOnlyWhenMovingIn { get; set; } = true;

    private Pointer? _pointer;
    private Vector2? _prevGlobalTargetPosition;
    private float _delayBeforeRemove = DelayBeforeRemovePointer;
    
    public override void _Ready()
    {
        TreeExiting += ForceRemovePointer;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_delayBeforeRemove > 0)
        {
            _delayBeforeRemove -= (float)delta;
        }

        if (ShowBelow && IsMovingUpOrIgnored() && _target.IsBelowPlayableArea() ||
            ShowAbove && IsMovingDownOrIgnored() && _target.IsAbovePlayableArea())
        {
            SpawnPointer();
        }
        else
        {
            RemovePointer();
        }
        
        _prevGlobalTargetPosition = _target.GlobalPosition;
    }

    private bool IsMovingDownOrIgnored()
    {
        if (!ShowOnlyWhenMovingIn)
        {
            return true;
        }
        
        if (!_prevGlobalTargetPosition.HasValue)
        {
            return false;
        }

        return _prevGlobalTargetPosition.Value.Y < _target.GlobalPosition.Y;
    }
    
    private bool IsMovingUpOrIgnored()
    {
        if (!ShowOnlyWhenMovingIn)
        {
            return true;
        }
        
        if (!_prevGlobalTargetPosition.HasValue)
        {
            return false;
        }

        return _prevGlobalTargetPosition.Value.Y > _target.GlobalPosition.Y;
    }

    private void SpawnPointer()
    {
        if (_pointer != null && IsInstanceValid(_pointer))
        {
            return;
        }

        _delayBeforeRemove = DelayBeforeRemovePointer;
        _pointer = Pointer.Create(_target, PointerColor);
        ShapeGame.Instance.AddChild(_pointer);
    }

    private void ForceRemovePointer()
    {
        _delayBeforeRemove = 0;
        RemovePointer();
    }

    private void RemovePointer()
    {
        if (_pointer == null || _delayBeforeRemove > 0 || !IsInstanceValid(_pointer))
        {
            return;
        }
        
        _pointer.Remove();
        _pointer = null;
    }
    
}
