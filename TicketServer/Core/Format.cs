using System;
using System.Globalization;

namespace TicketServer.Core;

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

        // This will now properly parse strings like "2024-07-01T10:00:00Z"
        if (!DateTime.TryParseExact(time, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime formattedDate))
        {
            return null;
        }

        string formattedDataStr = formattedDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);

        return formattedDataStr;
    }
}
