public partial class SoundManager : Node
{

    public static SoundManager Instance { get; private set; } = null!;

    public override void _Ready()
    {
        Instance = this;
    }
    
    public AudioStreamPlayer PlaySound(AudioStream sound)
    {
        var soundNode = new AudioStreamPlayer();
        GetTree().Root.AddChild(soundNode);
        soundNode.Stream = sound;
        soundNode.Play();
        return soundNode;
    }

    public PositionalSound PlayPositionalSound(Node2D node, AudioStream sound)
    {
        return PlayPositionalSound(node.GlobalPosition, sound);
    }
    
    public PositionalSound PlayPositionalSound(Vector2 globalPosition, AudioStream sound)
    {
        var soundNode = new PositionalSound();
        GetTree().Root.AddChild(soundNode);
        soundNode.GlobalPosition = globalPosition;
        soundNode.Stream = sound;
        soundNode.Play();
        return soundNode;
    }
    
}