
namespace FAM.Application.Common.Interfaces
{
    public interface ITimeZoneService
    {
        DateTimeOffset GetCurrentTime(string timeZoneId);
        DateTimeOffset ConvertUtcToTimeZone(DateTimeOffset utcDateTime, string timeZoneId);
        string GetSystemTimeZone();
    }
}
