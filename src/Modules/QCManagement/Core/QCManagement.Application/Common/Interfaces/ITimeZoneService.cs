namespace QCManagement.Application.Common.Interfaces
{
    public interface ITimeZoneService
    {
        DateTimeOffset GetCurrentTime(string? timeZoneId = null);
        DateTimeOffset ConvertUtcToTimeZone(DateTimeOffset utcTime, string timeZoneId);
        string GetSystemTimeZone();
    }
}
