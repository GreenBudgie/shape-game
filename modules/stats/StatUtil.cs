using System.Globalization;
using System.Linq;

public static class StatUtil
{

    public static string FormatStat(this float number)
    {
        return number.ToString("0.##", CultureInfo.InvariantCulture);
    }
    
}