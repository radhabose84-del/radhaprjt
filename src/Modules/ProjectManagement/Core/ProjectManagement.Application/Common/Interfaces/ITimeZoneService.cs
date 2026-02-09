
namespace Core.Application.Common.Interfaces
{
    public interface ITimeZoneService
    {
        //DateTime GetCurrentTime(string timeZoneId);
       // DateTime ConvertUtcToTimeZone(DateTime utcDateTime, string timeZoneId);
       DateTimeOffset GetCurrentTime(string timeZoneId);
       DateTimeOffset ConvertUtcToTimeZone(DateTimeOffset utcDateTime, string timeZoneId);
       string GetSystemTimeZone();
    }  
}
