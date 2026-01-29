using Core.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private readonly string _systemTimeZoneId;

        public TimeZoneService()
        {
            _systemTimeZoneId = TimeZoneInfo.Local.Id; // Fetch system timezone dynamically
        }

        public DateTime ConvertUtcToTimeZone(DateTime utcDateTime, string timeZoneId)
        {
            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new ArgumentException($"Invalid TimeZoneId: {timeZoneId}");
            }
        }
        public DateTime GetCurrentTime(string timeZoneId)
        {
            return ConvertUtcToTimeZone(DateTime.UtcNow, timeZoneId);
        }

        public string GetSystemTimeZone()
        {
            return _systemTimeZoneId;
        }
    }
}