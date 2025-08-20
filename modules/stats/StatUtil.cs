using System.Globalization;
using System.Linq;

public static class StatUtil
{
    
    private const float StatPrecision = 0.01f;

    public static string FormatStat(this float number)
    {
        return Snapped(number, StatPrecision).ToString(CultureInfo.InvariantCulture);
    }
    
}