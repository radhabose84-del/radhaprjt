using BackgroundService.Application.Notification.Common.Interfaces;

namespace BackgroundService.Infrastructure.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        public DateTimeOffset ConvertUtcToTimeZone(DateTimeOffset utcDateTime, string timeZoneId)
        {
            try
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

        public string GetSystemTimeZone()
        {
            return TimeZoneInfo.Local.Id;
        }
    }
}
