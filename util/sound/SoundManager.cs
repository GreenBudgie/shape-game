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
        
        soundNode.Finished += () => soundNode.QueueFree();
        return soundNode;
    }

    public AudioStreamPlayer2D PlayPositionalSound(Node2D node, AudioStream sound)
    {
        return PlayPositionalSound(node.GlobalPosition, sound);
    }
    
    public AudioStreamPlayer2D PlayPositionalSound(Control node, AudioStream sound)
    {
        return PlayPositionalSound(node.GlobalPosition, sound);
    }
    
    public AudioStreamPlayer2D PlayPositionalSound(Vector2 globalPosition, AudioStream sound)
    {
        var soundNode = new AudioStreamPlayer2D();
        GetTree().Root.AddChild(soundNode);
        soundNode.MaxDistance = 4000;
        soundNode.GlobalPosition = globalPosition;
        soundNode.Stream = sound;
        soundNode.Play();

        soundNode.Finished += () => soundNode.QueueFree();
        return soundNode;
    }
    
}