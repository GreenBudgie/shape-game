public partial class PointerArea : Node2D
{

    private const float DelayBeforeRemovePointer = 0.2f;

    [Export] private CollisionObject2D _target = null!;
    
    [Export] private Color _pointerColor;

    [Export] private bool _showBelow = true;
    
    [Export] private bool _showAbove = true;

    [Export] private bool _showOnlyWhenMovingIn = true;

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
        
        _prevGlobalTargetPosition = _target.GlobalPosition;
        
        if (_showBelow && IsMovingUpOrIgnored() && _target.IsBelowPlayableArea())
        {
            SpawnPointer();
            return;
        }
        
        if (_showAbove && IsMovingDownOrIgnored() && _target.IsAbovePlayableArea())
        {
            SpawnPointer();
            return;
        }
        
        RemovePointer();
    }

    private bool IsMovingDownOrIgnored()
    {
        if (!_showOnlyWhenMovingIn)
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
        if (!_showOnlyWhenMovingIn)
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
        _pointer = Pointer.Create(_target, _pointerColor);
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
