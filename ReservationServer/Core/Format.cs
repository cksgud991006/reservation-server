using System;
using System.Globalization;

namespace ReservationServer.Core;

public static class Format
{
    // Ensure your DateTimeFormat constant includes the seconds (:ss)
    public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

    public static string FormatDate(string time)
    {
        if (string.IsNullOrWhiteSpace(time))
        {
            return null;
        }

        // AssumeUniversal/AdjustToUniversal is required here: the trailing "Z" in
        // DateTimeFormat is just a literal character match, not a UTC designator, so
        // DateTimeStyles.None was parsing the value as local time and silently shifting
        // it by the machine's UTC offset on every round trip.
        if (!DateTime.TryParseExact(time, DateTimeFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime formattedDate))
        {
            return null;
        }

        string formattedDataStr = formattedDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);

        return formattedDataStr;
    }
}
