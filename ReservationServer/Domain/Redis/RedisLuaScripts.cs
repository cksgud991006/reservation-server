using StackExchange.Redis;
namespace ReservationServer.Domain.Redis;
public static class RedisLuaScripts
{
    public static readonly LuaScript ReserveSeatScript = LuaScript.Prepare(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Scripts", "SeatInventory.lua")));
}