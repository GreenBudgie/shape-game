public readonly record struct UIntRange(uint Min, uint Max)
{
    public UIntRange(uint value) : this(value, value)
    {
    }

    public uint Random()
    {
        return (uint)GD.RandRange(Min, Max);
    }

    public static implicit operator UIntRange(uint from) => new(from);
}