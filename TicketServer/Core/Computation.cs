namespace TicketServer.Core
{
    public static class Computation
    {
        public static string ComputeFlightId(string flightId, string time)
        {
            // Return the formatted string using UTC time
            return $"{flightId.Trim()}-{time}";
        }

        public static string ComputeBookingId(string flightId, string userId) 
        {
            // Generate a unique booking ID based on flight ID and user ID
            return $"{flightId}-{userId}-{Guid.NewGuid()}";
        }

        public static string ComputeFlightNumberByFlightId(string flightId)
        {
            var parts = flightId.Split('-');
            return parts[0];
        }

        public static string ComputeDepartureTimeByFlightId(string flightId)
        {
            string[] parts = flightId.Split('-');
            
            return string.Join('-', parts[1..]);
        }

        public static string ComputeFlightIdByBookingId(string bookingId)
        {
            var parts = bookingId.Split('-');
            return $"{parts[0]}-{parts[1]}";
        }

        public static string ComputeUserIdByBookingId(string bookingId)
        {
            var parts = bookingId.Split('-');
            return parts[2];
        }
    }
}