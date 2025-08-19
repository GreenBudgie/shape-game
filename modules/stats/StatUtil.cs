using System.Globalization;

public static class StatUtil
{
    
    private const float StatPrecision = 0.1f;

    public static string FormatStat(this float number)
    {
        return Snapped(number, StatPrecision).ToString(CultureInfo.InvariantCulture);
    }
    
}