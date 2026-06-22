using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class HexSelectorAttribute : Attribute
{
    public int Radius { get; init; } = 3;
}
