using ProductionManagement.Application.Common.Interfaces;

namespace ProductionManagement.Infrastructure.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        public DateTimeOffset GetCurrentTime(string? timeZoneId = null)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
                return DateTimeOffset.UtcNow;

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        }

        public DateTimeOffset ConvertUtcToTimeZone(DateTimeOffset utcTime, string timeZoneId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(utcTime, timeZone);
        }

        public string GetSystemTimeZone()
        {
            return TimeZoneInfo.Local.Id;
        }
    }
}
