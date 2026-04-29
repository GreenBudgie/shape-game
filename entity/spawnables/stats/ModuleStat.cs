[GlobalClass]
public abstract partial class ModuleStat : Resource
{

    public abstract string Name { get; }

    public abstract float Value { get; }

    /// <summary>
    /// Introduces randomness to the stat.
    ///
    /// If set to more than 0, value can deviate in this delta range.
    /// </summary>
    public virtual float ValueDelta => 0;

    public abstract Texture2D Icon { get; }

    public virtual string FormattedValue => Value.FormatStat();
    
    /// <summary>
    /// Called right after the context is created.
    /// Called only once for each stat in the context,
    /// even if there are multiple stats present on projectile and modifiers.
    /// </summary>
    public virtual void Apply(ShotContext context)
    {
    }

}