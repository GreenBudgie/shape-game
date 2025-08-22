[GlobalClass]
public abstract partial class ModuleStat : Resource
{

    public abstract string Name { get; }

    public abstract float Value { get; }
    
    public abstract Texture2D Icon { get; }

    public virtual string FormattedValue => Value.FormatStat();

}