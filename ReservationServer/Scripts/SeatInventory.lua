-- KEYS[1]: FlightInstanceKey
-- KEYS[2]: FlightSeatCountKey
-- KEYS[3]: SeatLayoutKey
-- KEYS[4]: FlightBookingKey
-- ARGV[1]: UserId

-- should I write error code definition here? or just return error code and message in the lua script?

-- Step 1: Check if the flight seat exists
local totalSeatCountStr = redis.call('GET', KEYS[2])
if not totalSeatCountStr then
    return {-2, "Flight capacity configuration not found"}
end

-- Step 2: Check if there are any seats available for the flight
local totalSeatCount = tonumber(totalSeatCountStr)
local bookedSeatCount = redis.call('SCARD', KEYS[4])

if (totalSeatCount - bookedSeatCount) < 1 then
    return {-2, "No seats available"}
end

-- Step 3: Check if the seat is already reserved by anyone (not just this caller)
local isOccupied = redis.call('SCARD', KEYS[4]) > 0

if isOccupied then
    return {0, "Seat is already occupied"}
else
    redis.call('SADD', KEYS[4], ARGV[1])
    return {1, "Seat booked successfully"}
end