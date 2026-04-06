using LogisticsManagement.Application.Common.Interfaces;

namespace LogisticsManagement.Infrastructure.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private readonly string _systemTimeZoneId;

        public TimeZoneService()
        {
            _systemTimeZoneId = TimeZoneInfo.Local.Id;
        }

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
            return _systemTimeZoneId;
        }
    }
}
