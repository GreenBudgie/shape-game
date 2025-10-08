public partial class PointerArea : VisibleOnScreenNotifier2D
{

    [Export] private Node2D _target = null!;
    
    [Export] private Color _pointerColor;

    private Pointer? _pointer;
    
    public override void _Ready()
    {
        Callable.From(SpawnAfterCreating).CallNextFrame(GetTree());
        
        ScreenEntered += RemovePointer;
        TreeExiting += RemovePointer;
        ScreenExited += SpawnPointer;
    }

    private void SpawnAfterCreating()
    {
        if (!IsOnScreen())
        {
            SpawnPointer();
        }
    }

    private void SpawnPointer()
    {
        if (_pointer != null && IsInstanceValid(_pointer))
        {
            return;
        }
        
        _pointer = Pointer.Create(_target, _pointerColor);
        ShapeGame.Instance.AddChild(_pointer);
    }

    private void RemovePointer()
    {
        if (_pointer == null || !IsInstanceValid(_pointer))
        {
            return;
        }
        
        _pointer.Remove();
        _pointer = null;
    }
    
}
