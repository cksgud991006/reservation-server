using System;
using System.Globalization;

public static class Format
{
    // Ensure your DateTimeFormat constant includes the seconds (:ss)
    public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ"; 
    /*
    public static DateTime? FormatDate(string? time)
    {
        if (string.IsNullOrWhiteSpace(time))
        {
            return null;
        }

        // This will now properly parse strings like "2024-07-01T10:00:00Z"
        if (!DateTime.TryParseExact(time, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime formattedDate))
        {
            return null;
        }

        return DateTime.SpecifyKind(formattedDate, DateTimeKind.Utc);
    }

    public static string? GetDateString(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return null; 
        }

        var utcDate = dateTime.Value.ToUniversalTime();

        string result = utcDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
        
        return result;
    }
    */
    public static string FormatDate(string time)
    {
        if (string.IsNullOrWhiteSpace(time))
        {
            return null;
        }

        // This will now properly parse strings like "2024-07-01T10:00:00Z"
        if (!DateTime.TryParseExact(time, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime formattedDate))
        {
            return null;
        }

        string formattedDataStr = formattedDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);

        return formattedDataStr;
    }
}