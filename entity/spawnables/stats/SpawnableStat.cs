using System.Globalization;
using System.Text;

public abstract class SpawnableStat
{

    public abstract string Name { get; }
    
    public abstract Texture2D Icon { get; }
    
    public virtual string ValuePostfix => "";

    /// <summary>
    /// Plain additive stat value, +N or -N 
    /// </summary>
    public float Value { get; set; }
    
    /// <summary>
    /// Percentage stat value, +N% or -N%
    /// </summary>
    public float ValuePercent { get; set; }

    /// <summary>
    /// Multiplicative stat value, xN.
    ///
    /// 1 by default
    /// </summary>
    public float ValueMult { get; set; } = 1;

    /// <summary>
    /// Introduces additive randomness to the stat.
    ///
    /// If set to more than 0, value can deviate in this delta range, +N or -N
    /// </summary>
    public float ValueDelta { get; set; }

    public bool IsAdditive => Value != 0;

    public float Calculate(float currentValue)
    {
        var result = currentValue;
        
        // Written in three operations for clarity
        result += Value;
        result *= 1 + ValuePercent * 0.01f;
        result *= ValueMult;

        if (ValueDelta != 0)
        {
            result = RandomUtils.DeltaRange(result, ValueDelta);
        }

        return result;
    }

    public string Formatted()
    {
        var result = new StringBuilder();
        if (Value != 0)
        {
            result.Append(FormatValue(Value));
        }
        
        if (ValuePercent != 0)
        {
            result.Append(FormatValue(ValuePercent)).Append('%');
        }
        
        if (!IsEqualApprox(ValueMult, 1))
        {
            result.Append('x').Append(FormatValue(ValueMult));
        }

        if (Value != 0 && ValuePostfix != "")
        {
            result.Append(' ').Append(ValuePostfix);
        }

        return result.ToString();
    }
    
    private static string FormatValue(float number)
    {
        return number.ToString("0.##", CultureInfo.InvariantCulture);
    }
    
}