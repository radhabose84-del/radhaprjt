
namespace Core.Application.Common.Interfaces
{
    public interface ITimeZoneService
    {
        DateTime GetCurrentTime(string timeZoneId);
        DateTime ConvertUtcToTimeZone(DateTime utcDateTime, string timeZoneId);
        string GetSystemTimeZone();
    }  
}
