using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

public partial class CrystalManager : Node
{

    private static readonly StringName FallingCrystalsGroup = "falling_crystals"; 

    [Signal]
    public delegate void CrystalAmountChangedEventHandler();

    private const float MaxPitch = 3f;
    private const float PitchRandomizationDelta = 0.05f;
    private const float PitchIncreaseDelta = 0.12f;
    private const float PitchDecreaseDelta = 0.4f;
    private const double TimeToDecreasePitch = 0.4f;

    public static CrystalManager Instance { get; private set; } = null!;

    [Export] private AudioStream _collectSound = null!;

    private float _currentPitch = 1f;
    private double _timeToDecreasePitch = TimeToDecreasePitch;

    private int _crystals;

    public int Crystals
    {
        get => _crystals;
        set
        {
            _crystals = value;
            EmitSignalCrystalAmountChanged();
        }
    }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (IsEqualApprox(_currentPitch, 1f))
        {
            return;
        }

        _timeToDecreasePitch -= delta;

        if (_timeToDecreasePitch > 0)
        {
            return;
        }

        _currentPitch = Max(_currentPitch - PitchDecreaseDelta, 1f);
        _timeToDecreasePitch = TimeToDecreasePitch;
    }

    public void CollectCrystal()
    {
        var sound = SoundManager.Instance.PlaySound(_collectSound);
        sound.RandomizePitchOffset(PitchRandomizationDelta);
        _currentPitch = Min(_currentPitch + PitchIncreaseDelta, MaxPitch);
        _timeToDecreasePitch = TimeToDecreasePitch;

        Crystals++;
    }

    public IEnumerable<FallingCrystal> GetFallingCrystals()
    {
        return GetTree().GetNodesInGroup(FallingCrystalsGroup).Cast<FallingCrystal>();
    }
    
    public int GetFallingCrystalsCount()
    {
        return GetTree().GetNodeCountInGroup(FallingCrystalsGroup);
    }

    public void ClearFallingCrystals()
    {
        foreach (var crystal in GetTree().GetNodesInGroup(FallingCrystalsGroup))
        {
            crystal.QueueFree();
        }
    }

}