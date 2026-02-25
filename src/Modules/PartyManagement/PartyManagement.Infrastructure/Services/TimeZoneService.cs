using PartyManagement.Application.Common.Interfaces;

namespace InventoryManagement.Infrastructure.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private readonly string _systemTimeZoneId;

        public TimeZoneService()
        {
            _systemTimeZoneId = TimeZoneInfo.Local.Id; // Fetch system timezone dynamically
        }

        public DateTimeOffset ConvertUtcToTimeZone(DateTimeOffset utcDateTime, string timeZoneId)
        {   try
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(utcDateTime, timeZone);
        }
        catch (TimeZoneNotFoundException)    
        {
            throw new ArgumentException($"Invalid TimeZoneId: {timeZoneId}");
        }
        }

        public DateTimeOffset GetCurrentTime(string timeZoneId)
        {
             return ConvertUtcToTimeZone(DateTimeOffset.UtcNow, timeZoneId);
        }

        // public DateTime ConvertUtcToTimeZone(DateTime utcDateTime, string timeZoneId)
        // {
        //     try
        //     {
        //         var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        //         return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        //     }
        //     catch (TimeZoneNotFoundException)
        //     {
        //         throw new ArgumentException($"Invalid TimeZoneId: {timeZoneId}");
        //     }
        // }
        // public DateTime GetCurrentTime(string timeZoneId)
        // {
        //     return ConvertUtcToTimeZone(DateTime.UtcNow, timeZoneId);
        // }



        public string GetSystemTimeZone()
        {
            return _systemTimeZoneId;
        }

      
    }
}